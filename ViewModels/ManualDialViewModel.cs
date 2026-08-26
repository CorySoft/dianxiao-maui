using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DianxiaoMaui.Services;

namespace DianxiaoMaui.ViewModels;

public partial class ManualDialViewModel : ObservableObject
{
    [ObservableProperty]
    private string _number = string.Empty;

    public ICommand KeyCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CallCommand { get; }

    public ManualDialViewModel()
    {
        KeyCommand = new Command<string>(Key);
        DeleteCommand = new Command(Delete);
        CallCommand = new AsyncRelayCommand(CallAsync);
    }

    private void Key(string? key)
    {
        if (key is null) return;
        Number += key;
    }

    private void Delete()
    {
        if (Number.Length > 0)
            Number = Number[..^1];
    }

    private async Task CallAsync()
    {
        if (string.IsNullOrWhiteSpace(Number))
        {
            await Shell.Current.DisplayAlert("提示", "请先输入号码", "确定");
            return;
        }

        // 应用双卡前缀（自动拨号设置）
        var prefixed = await ApplySimPrefixAsync(Number);

        // 手动拨号默认自动点呼叫
        await PlatformDialer.DialAsync(prefixed);

        await Shell.Current.DisplayAlert("已拨号", $"正在拨打 {prefixed}", "确定");
    }

    private async Task<string> ApplySimPrefixAsync(string raw)
    {
        // 若号码已含前缀则跳过；否则按设置加上卡1/卡2前缀（此处默认卡1）
        var prefs = PreferencesService.Instance;
        var sim1 = await prefs.GetSim1PrefixesAsync();
        if (sim1.Count > 0 && !sim1.Any(p => raw.StartsWith(p)))
            return sim1[0] + raw;
        return raw;
    }
}