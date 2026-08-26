using Android.App;
using Android.Content.PM;

// 电话相关权限
[assembly: UsesPermission(Android.Manifest.Permission.CallPhone)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadPhoneState)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadCallLog)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteCallLog)]

// 通话录音权限
[assembly: UsesPermission(Android.Manifest.Permission.RecordAudio)]

// 前台服务权限 (Android 14+)
[assembly: UsesPermission(Android.Manifest.Permission.ForegroundService)]
[assembly: UsesPermission("android.permission.FOREGROUND_SERVICE_PHONE_CALL")]

// 其他权限
[assembly: UsesPermission(Android.Manifest.Permission.ReceiveBootCompleted)]
[assembly: UsesPermission(Android.Manifest.Permission.WakeLock)]
[assembly: UsesPermission(Android.Manifest.Permission.Vibrate)]
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessNetworkState)]

// 无障碍服务权限
[assembly: UsesPermission(Android.Manifest.Permission.BindAccessibilityService)]

// 联系人权限
[assembly: UsesPermission(Android.Manifest.Permission.ReadContacts)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteContacts)]
