using DianxiaoMaui.Views.Tabs;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace DianxiaoMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if ANDROID
        builder.Services.AddSingleton<DianxiaoMaui.Platforms.Android.Services.AndroidDialerPlatform>();
#endif

        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}