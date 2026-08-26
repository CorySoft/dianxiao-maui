using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DianxiaoMaui.Models;
using DianxiaoMaui.Services;

namespace DianxiaoMaui.ViewModels;

public partial class BlacklistViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Blacklist> _blacklist = new();

    [ObservableProperty]
    private string _newPhone = string.Empty;

    public ICommand AddCommand { get; }
    public ICommand RemoveCommand { get; }

    public BlacklistViewModel()
    {
        AddCommand = new AsyncRelayCommand(AddAsync);
        RemoveCommand = new AsyncRelayCommand<Blacklist>(RemoveAsync);

        LoadAsync();
    }

    private async Task LoadAsync()
    {
        var list = await DatabaseService.Instance.GetBlacklistAsync();
        Blacklist.Clear();
        foreach (var b in list) Blacklist.Add(b);
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPhone)) return;
        if (Blacklist.Any(b => b.PhoneNumber == NewPhone))
        {
            await Shell.Current.DisplayAlert("提示", "该号码已在黑名单中", "确定");
            return;
        }
        await DatabaseService.Instance.AddBlacklistAsync(NewPhone, "手动添加");
        NewPhone = string.Empty;
        await LoadAsync();
    }

    private async Task RemoveAsync(Blacklist? item)
    {
        if (item is null) return;
        await DatabaseService.Instance.RemoveBlacklistAsync(item.Id);
        Blacklist.Remove(item);
    }
}