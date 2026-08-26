using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace DianxiaoMaui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // 确保无障碍服务权限检查
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            // 可在此检查并引导用户开启无障碍服务
        }
    }
}