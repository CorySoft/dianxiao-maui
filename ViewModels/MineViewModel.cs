using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DianxiaoMaui.Models;
using DianxiaoMaui.Services;

namespace DianxiaoMaui.ViewModels;

public partial class MineViewModel : ObservableObject
{
    private readonly DialerService _dialer = DialerService.Instance;

    [ObservableProperty]
    private ObservableCollection<CallTask> _tasks = new();

    [ObservableProperty]
    private string _progressText = "待拨打 0 个 · 已接通 0 个";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private int _pendingCount;

    [ObservableProperty]
    private int _connectedCount;

    [ObservableProperty]
    private string _pasteText = string.Empty;

    public ICommand ImportPasteCommand { get; }
    public ICommand ImportCsvCommand { get; }
    public ICommand AddNumberCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ClearTasksCommand { get; }

    public MineViewModel()
    {
        ImportPasteCommand = new AsyncRelayCommand(ImportPasteAsync);
        ImportCsvCommand = new AsyncRelayCommand(ImportCsvAsync);
        AddNumberCommand = new AsyncRelayCommand(AddNumberAsync);
        StartCommand = new AsyncRelayCommand(StartAsync);
        StopCommand = new RelayCommand(Stop);
        ClearTasksCommand = new AsyncRelayCommand(ClearTasksAsync);

        _dialer.StateChanged += OnStateChanged;
        _dialer.ProgressChanged += OnProgressChanged;
        _dialer.TaskCompleted += OnTaskCompleted;
    }

    public async Task OnAppearingAsync()
    {
        await LoadTasksAsync();
        UpdateProgress();
    }

    private async Task LoadTasksAsync()
    {
        var list = await DatabaseService.Instance.GetActiveTasksAsync();
        Tasks.Clear();
        foreach (var t in list) Tasks.Add(t);
        PendingCount = await DatabaseService.Instance.GetPendingCountAsync();
    }

    private void UpdateProgress()
    {
        ProgressText = $"待拨打 {PendingCount} 个 · 已接通 {ConnectedCount} 个";
    }

    private void OnStateChanged(object? sender, int state)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsRunning = _dialer.IsRunning;
        });
    }

    private void OnProgressChanged(object? sender, string msg)
    {
        MainThread.BeginInvokeOnMainThread(() => ProgressText = msg);
    }

    private void OnTaskCompleted(object? sender, CallTask task)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (task.Result == 1) ConnectedCount++;
            await LoadTasksAsync();
            UpdateProgress();
        });
    }

    private async Task ImportPasteAsync()
    {
        if (string.IsNullOrWhiteSpace(PasteText))
        {
            await Shell.Current.DisplayAlert("提示", "请先在上方粘贴号码（每行一个）", "确定");
            return;
        }
        var numbers = NumberImporter.ImportFromText(PasteText);
        if (numbers.Count == 0)
        {
            await Shell.Current.DisplayAlert("提示", "未识别到有效号码", "确定");
            return;
        }
        var added = await DatabaseService.Instance.ImportNumbersAsync(numbers);
        PasteText = string.Empty;
        await LoadTasksAsync();
        UpdateProgress();
        await Shell.Current.DisplayAlert("导入完成", $"成功导入 {added} 个号码", "确定");
    }

    private async Task ImportCsvAsync()
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "text/csv", "text/comma-separated-values", "application/csv" } },
                { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                { DevicePlatform.WinUI, new[] { ".csv" } },
                { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text" } },
            });
            var options = new PickOptions { PickerTitle = "选择 CSV 文件", FileTypes = customFileType };
            var result = await FilePicker.Default.PickAsync(options);
            if (result is null) return;

            await using var stream = await result.OpenReadAsync();
            var numbers = await NumberImporter.ImportFromCsvAsync(stream);
            if (numbers.Count == 0)
            {
                await Shell.Current.DisplayAlert("提示", "未识别到有效号码", "确定");
                return;
            }
            var added = await DatabaseService.Instance.ImportNumbersAsync(numbers);
            await LoadTasksAsync();
            UpdateProgress();
            await Shell.Current.DisplayAlert("导入完成", $"成功导入 {added} 个号码", "确定");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("错误", ex.Message, "确定");
        }
    }

    private async Task AddNumberAsync()
    {
        var phone = await Shell.Current.DisplayPromptAsync("添加号码", "请输入要拨打的号码", placeholder: "如 13800138000");
        if (string.IsNullOrWhiteSpace(phone)) return;
        var numbers = NumberImporter.ImportFromText(phone);
        if (numbers.Count == 0)
        {
            await Shell.Current.DisplayPromptAsync("提示", "号码无效", "确定");
            return;
        }
        await DatabaseService.Instance.ImportNumbersAsync(numbers);
        await LoadTasksAsync();
        UpdateProgress();
    }

    private async Task StartAsync()
    {
        if (!await EnsureAccessibilityAsync()) return;
        await _dialer.StartAsync();
        IsRunning = true;
    }

    private void Stop()
    {
        _dialer.Stop();
        IsRunning = false;
    }

    private async Task ClearTasksAsync()
    {
        var ok = await Shell.Current.DisplayAlert("确认", "清除全部待拨打任务？", "清除", "取消");
        if (!ok) return;
        foreach (var t in Tasks)
            await DatabaseService.Instance.DeleteTaskAsync(t.Id);
        Tasks.Clear();
        PendingCount = 0;
        UpdateProgress();
    }

    private async Task<bool> EnsureAccessibilityAsync()
    {
        var enabled = await DialerService.Instance.IsAccessibilityEnabledAsync();
        if (enabled) return true;
        var go = await Shell.Current.DisplayAlert("需要无障碍服务",
            "自动拨号需要开启本应用的无障碍服务（用于自动点击“呼叫”按钮）。是否前往设置开启？",
            "去开启", "取消");
        if (go)
        {
            await DialerService.Instance.OpenAccessibilitySettingsAsync();
        }
        return false;
    }
}