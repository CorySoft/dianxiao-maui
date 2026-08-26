using Android.App;
using Android.AccessibilityServices;
using Android.Content;
using Android.OS;
using Android.Telecom;
using Android.Views.Accessibility;
using AndroidX.Core.App;
using DianxiaoMaui.Models;
using DianxiaoMaui.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AppContext = global::Android.App.Application;
using NetUri = global::Android.Net.Uri;
using AndroidResource = global::Android.Resource.Drawable;

namespace DianxiaoMaui.Platforms.Android.Services;

/// <summary>
/// Android 端拨号/无障碍/前台服务实现
/// </summary>
public class AndroidDialerPlatform
{
    private static AndroidDialerPlatform? _instance;
    public static AndroidDialerPlatform Instance => _instance ??= new AndroidDialerPlatform();

    private const string CHANNEL_ID = "dianxiao_dialer_channel";
    private const int FOREGROUND_ID = 999;

    private AndroidDialerPlatform()
    {
        PlatformDialer.DialHandler = DialAsync;
        PlatformDialer.WaitForCallEndHandler = WaitForCallEndAsync;
        PlatformDialer.IsAccessibilityEnabledHandler = IsAccessibilityEnabledAsync;
        PlatformDialer.OpenAccessibilitySettingsHandler = OpenAccessibilitySettingsAsync;
    }

    #region 拨号

    public Task DialAsync(string phone)
    {
        var context = AppContext.Context;
        if (context == null) return Task.CompletedTask;

        StartForegroundService();

        var uri = NetUri.Parse("tel:" + phone);
        var intent = new Intent(Intent.ActionDial, uri);
        intent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
        return Task.CompletedTask;
    }

    private void StartForegroundService()
    {
        var context = AppContext.Context;
        if (context == null) return;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(CHANNEL_ID,
                "自动拨号服务", NotificationImportance.Low);
            var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(channel);
        }

        var notification = new NotificationCompat.Builder(context, CHANNEL_ID)
            .SetContentTitle("自动拨号运行中")
            .SetContentText("正在监听拨号界面，自动点击呼叫")
            .SetSmallIcon(AndroidResource.SymActionCall)
            .SetPriority(NotificationCompat.PriorityLow)
            .SetOngoing(true)
            .Build();

        var serviceIntent = new Intent(context, typeof(DialerForegroundService));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            context.StartForegroundService(serviceIntent);
        }
        else
        {
            context.StartService(serviceIntent);
        }
    }

    #endregion

    #region 通话状态监听

    private global::Android.Telephony.TelephonyManager? _telephony;
    private global::Android.Telephony.PhoneStateListener? _listener;
    private TaskCompletionSource<bool>? _callEndTcs;

    public Task<bool> WaitForCallEndAsync(string phone, CancellationToken token)
    {
        _callEndTcs = new TaskCompletionSource<bool>();

        _telephony = (global::Android.Telephony.TelephonyManager)AppContext.Context!.GetSystemService(Context.TelephonyService);
        _listener = new CallStateListener(this);
        _telephony.Listen(_listener, global::Android.Telephony.PhoneStateListenerFlags.CallState);

        token.Register(() =>
        {
            if (!_callEndTcs.Task.IsCompleted)
                _callEndTcs?.TrySetResult(false);
            StopListening();
        });

        return _callEndTcs.Task;
    }

    internal void OnCallStateChanged(global::Android.Telephony.CallState state, string? incomingNumber)
    {
        if (_callEndTcs is null) return;

        switch (state)
        {
            case global::Android.Telephony.CallState.Offhook:
                break;
            case global::Android.Telephony.CallState.Idle:
                if (_callEndTcs.Task.IsCompleted) break;
                _callEndTcs.TrySetResult(true);
                StopListening();
                break;
        }
    }

    private void StopListening()
    {
        if (_telephony != null && _listener != null)
        {
            _telephony.Listen(_listener, global::Android.Telephony.PhoneStateListenerFlags.None);
            _listener = null;
        }
        _telephony = null;
    }

    private class CallStateListener : global::Android.Telephony.PhoneStateListener
    {
        private readonly AndroidDialerPlatform _owner;
        public CallStateListener(AndroidDialerPlatform owner) => _owner = owner;

        public override void OnCallStateChanged(global::Android.Telephony.CallState state, string? incomingNumber)
        {
            base.OnCallStateChanged(state, incomingNumber);
            _owner.OnCallStateChanged(state, incomingNumber);
        }
    }

    #endregion

    #region 无障碍服务

    public Task<bool> IsAccessibilityEnabledAsync()
    {
        try
        {
            var context = AppContext.Context;
            if (context == null) return Task.FromResult(false);

            var enabledServices = global::Android.Provider.Settings.Secure.GetString(
                context.ContentResolver,
                global::Android.Provider.Settings.Secure.EnabledAccessibilityServices);
            if (string.IsNullOrEmpty(enabledServices)) return Task.FromResult(false);

            var pkg = context.PackageName;
            return Task.FromResult(enabledServices.Contains(pkg + "/com.xinghe.dianxiao.DialerAccessibilityService"));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task OpenAccessibilitySettingsAsync()
    {
        var context = AppContext.Context;
        if (context == null) return Task.CompletedTask;

        var intent = new Intent(global::Android.Provider.Settings.ActionAccessibilitySettings);
        intent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
        return Task.CompletedTask;
    }

    #endregion
}

#region 前台服务（保持无障碍服务存活）

[Service(Name = "com.xinghe.dianxiao.DialerForegroundService",
    ForegroundServiceType = (global::Android.Content.PM.ForegroundService)4, // TypePhoneCall
    Exported = false)]
public class DialerForegroundService : Service
{
    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        const string CHANNEL_ID = "dianxiao_dialer_channel";
        const int FOREGROUND_ID = 999;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(CHANNEL_ID,
                "自动拨号服务", NotificationImportance.Low);
            var manager = (NotificationManager)GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(channel);
        }

        var notification = new NotificationCompat.Builder(this, CHANNEL_ID)
            .SetContentTitle("自动拨号运行中")
            .SetContentText("正在监听拨号界面，自动点击呼叫按钮")
            .SetSmallIcon(AndroidResource.SymActionCall)
            .SetPriority(NotificationCompat.PriorityLow)
            .SetOngoing(true)
            .Build();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
        {
            StartForeground(FOREGROUND_ID, notification, (global::Android.Content.PM.ForegroundService)4);
        }
        else
        {
            StartForeground(FOREGROUND_ID, notification);
        }
        return StartCommandResult.Sticky;
    }
}

#endregion

#region 无障碍服务实现

[Service(Name = "com.xinghe.dianxiao.DialerAccessibilityService",
    Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
    Exported = true,
    Enabled = true)]
[IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
[MetaData("android.accessibilityservice", Resource = "@xml/accessibility_service_config")]
public class DialerAccessibilityService : AccessibilityService
{
    public override void OnAccessibilityEvent(AccessibilityEvent? e)
    {
        if (e?.EventType == EventTypes.WindowStateChanged || e?.EventType == EventTypes.WindowContentChanged)
        {
            TryClickCallButton(e);
        }
    }

    public override void OnInterrupt() { }

    protected override void OnServiceConnected()
    {
        base.OnServiceConnected();
    }

    private void TryClickCallButton(AccessibilityEvent? e)
    {
        var root = RootInActiveWindow;
        if (root == null) return;

        var callButtons = FindCallButtons(root);
        foreach (var btn in callButtons)
        {
            btn.PerformAction(global::Android.Views.Accessibility.Action.Click);
            break;
        }
    }

    private List<AccessibilityNodeInfo> FindCallButtons(AccessibilityNodeInfo node)
    {
        var result = new List<AccessibilityNodeInfo>();
        if (node == null) return result;

        var text = node.Text?.ToString() ?? "";
        var className = node.ClassName?.ToString() ?? "";
        var desc = node.ContentDescription?.ToString() ?? "";

        var combined = (text + desc).ToLower();
        if ((combined.Contains("呼叫") || combined.Contains("拨打") || combined.Contains("拨号") || combined.Contains("dial") || combined.Contains("call"))
            && (className.Contains("Button") || className.Contains("ImageButton") || className.Contains("TextView")))
        {
            result.Add(node);
        }

        for (int i = 0; i < node.ChildCount; i++)
        {
            var child = node.GetChild(i);
            if (child != null)
                result.AddRange(FindCallButtons(child));
        }
        return result;
    }
}

#endregion
