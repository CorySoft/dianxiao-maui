using DianxiaoMaui.Models;
using System.Collections.Concurrent;

namespace DianxiaoMaui.Services;

/// <summary>拨号引擎：协调自动外呼流程</summary>
public sealed class DialerService
{
    private static readonly Lazy<DialerService> _instance = new(() => new DialerService());
    public static DialerService Instance => _instance.Value;

    private readonly SemaphoreSlim _loopLock = new(1, 1);
    private CancellationTokenSource? _cts;
    private bool _running;

    // 状态机：0=空闲 1=拨打中 2=等待 3=已接通
    public int State { get; private set; } = 0;

    public event EventHandler<int>? StateChanged;
    public event EventHandler<string>? ProgressChanged;
    public event EventHandler<CallTask>? TaskCompleted;

    private DialerService() { }

    public bool IsRunning => _running;

    public async Task StartAsync()
    {
        if (_running) return;
        await _loopLock.WaitAsync();
        try
        {
            if (_running) return;
            _running = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _ = Task.Run(async () => await RunLoopAsync(token), token);
        }
        finally
        {
            _loopLock.Release();
        }
    }

    public void Stop()
    {
        _running = false;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        SetState(0);
    }

    private void SetState(int state)
    {
        State = state;
        StateChanged?.Invoke(this, state);
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        try
        {
            while (_running && !token.IsCancellationRequested)
            {
                var pending = await DatabaseService.Instance.GetPendingTaskAsync();
                if (pending is null)
                {
                    ProgressChanged?.Invoke(this, "待拨打：0　已接通：0");
                    await Task.Delay(1000, token);
                    continue;
                }

                // 检查黑名单
                if (await DatabaseService.Instance.IsBlacklistedAsync(pending.PhoneNumber))
                {
                    pending.Status = CallTask.STATUS_SKIPPED;
                    await DatabaseService.Instance.UpdateTaskAsync(pending);
                    continue;
                }

                await DialNumberAsync(pending);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ProgressChanged?.Invoke(this, $"错误：{ex.Message}");
        }
        finally
        {
            SetState(0);
        }
    }

    private async Task DialNumberAsync(CallTask task)
    {
        task.Status = CallTask.STATUS_DIALING;
        task.StartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await DatabaseService.Instance.UpdateTaskAsync(task);
        SetState(1);

        ProgressChanged?.Invoke(this,
            $"拨打中：{task.PhoneNumber}");

        // 通过平台代码拉起拨号器并自动点呼叫
        await PlatformDialer.DialAsync(task.PhoneNumber);

        // 等待通话结束（平台监听电话状态）
        var connected = await PlatformDialer.WaitForCallEndAsync(task.PhoneNumber, token: default);

        task.FinishedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        task.Status = CallTask.STATUS_DONE;
        task.Result = connected ? 1 : 0;
        task.DurationSec = (int)((task.FinishedAt - task.StartedAt) / 1000);

        await DatabaseService.Instance.UpdateTaskAsync(task);

        // 写入通话记录
        var log = new CallLog
        {
            PhoneNumber = task.PhoneNumber,
            Connected = connected,
            DurationSec = task.DurationSec,
            StartTime = task.StartedAt,
            EndTime = task.FinishedAt,
            TaskId = task.Id,
            SimSlot = task.SimSlot
        };
        await DatabaseService.Instance.AddCallLogAsync(log);

        TaskCompleted?.Invoke(this, task);

        SetState(0);

        // 间隔等待
        var interval = await PreferencesService.Instance.GetIntervalSecAsync();
        if (interval > 0)
            await Task.Delay(interval * 1000);
    }

    /// <summary>检查无障碍服务是否已启用</summary>
    public Task<bool> IsAccessibilityEnabledAsync()
    {
        return PlatformDialer.IsAccessibilityEnabledHandler?.Invoke() ?? Task.FromResult(false);
    }

    /// <summary>打开无障碍服务设置页</summary>
    public Task OpenAccessibilitySettingsAsync()
    {
        return PlatformDialer.OpenAccessibilitySettingsHandler?.Invoke() ?? Task.CompletedTask;
    }
}

/// <summary>平台拨号抽象（Android 下使用前台服务+无障碍自动点呼叫）</summary>
public static class PlatformDialer
{
    public static Func<string, Task>? DialHandler { get; set; }
    public static Func<string, CancellationToken, Task<bool>>? WaitForCallEndHandler { get; set; }
    public static Func<Task<bool>>? IsAccessibilityEnabledHandler { get; set; }
    public static Func<Task>? OpenAccessibilitySettingsHandler { get; set; }

    public static async Task DialAsync(string phone)
    {
        if (DialHandler is not null)
            await DialHandler(phone);
        else
        {
            // 兜底：MAUI PhoneDialer
            try
            {
                if (Microsoft.Maui.ApplicationModel.Communication.PhoneDialer.IsSupported)
                    Microsoft.Maui.ApplicationModel.Communication.PhoneDialer.Open(phone);
            }
            catch { }
        }
    }

    public static async Task<bool> WaitForCallEndAsync(string phone, CancellationToken token)
    {
        if (WaitForCallEndHandler is not null)
            return await WaitForCallEndHandler(phone, token);
        // 兜底：模拟等待 5 秒
        await Task.Delay(5000, token);
        return false;
    }
}
