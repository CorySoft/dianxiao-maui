using DianxiaoMaui.Services;
using DianxiaoMaui.Views.Features;

namespace DianxiaoMaui.Views.Tabs;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnOpenAccessibility(object? sender, EventArgs e)
    {
        await DialerService.Instance.OpenAccessibilitySettingsAsync();
    }

    private async void OnDialSettings(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(DialSettingsPage));
    }

    private async void OnPrefixSettings(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PrefixSettingsPage));
    }

    private async void OnBlacklist(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(BlacklistPage));
    }

    private async void OnHistory(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CallHistoryPage));
    }

    private async void OnReport(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ReportPage));
    }

    private async void OnComingSoon(object? sender, TappedEventArgs e)
    {
        await DisplayAlert("提示", "该功能需要服务端支持，敬请期待", "确定");
    }
}