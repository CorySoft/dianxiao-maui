using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DianxiaoMaui.Services;

namespace DianxiaoMaui.ViewModels;

public partial class PrefixSettingsViewModel : ObservableObject
{
    private readonly PreferencesService _prefs = PreferencesService.Instance;

    [ObservableProperty]
    private ObservableCollection<string> _sim1Prefixes = new();

    [ObservableProperty]
    private ObservableCollection<string> _sim2Prefixes = new();

    [ObservableProperty]
    private string _newSim1Prefix = string.Empty;

    [ObservableProperty]
    private string _newSim2Prefix = string.Empty;

    public ICommand AddSim1PrefixCommand { get; }
    public ICommand RemoveSim1PrefixCommand { get; }
    public ICommand AddSim2PrefixCommand { get; }
    public ICommand RemoveSim2PrefixCommand { get; }

    public PrefixSettingsViewModel()
    {
        AddSim1PrefixCommand = new AsyncRelayCommand(AddSim1PrefixAsync);
        RemoveSim1PrefixCommand = new AsyncRelayCommand<string>(RemoveSim1PrefixAsync);
        AddSim2PrefixCommand = new AsyncRelayCommand(AddSim2PrefixAsync);
        RemoveSim2PrefixCommand = new AsyncRelayCommand<string>(RemoveSim2PrefixAsync);

        LoadAsync();
    }

    private async void LoadAsync()
    {
        var p1 = await _prefs.GetSim1PrefixesAsync();
        var p2 = await _prefs.GetSim2PrefixesAsync();
        Sim1Prefixes.Clear();
        Sim2Prefixes.Clear();
        foreach (var p in p1) Sim1Prefixes.Add(p);
        foreach (var p in p2) Sim2Prefixes.Add(p);
    }

    private async Task AddSim1PrefixAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSim1Prefix)) return;
        if (Sim1Prefixes.Contains(NewSim1Prefix)) return;
        Sim1Prefixes.Add(NewSim1Prefix);
        await _prefs.SetSim1PrefixesAsync(Sim1Prefixes.ToList());
        NewSim1Prefix = string.Empty;
    }

    private async Task RemoveSim1PrefixAsync(string? prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return;
        Sim1Prefixes.Remove(prefix);
        await _prefs.SetSim1PrefixesAsync(Sim1Prefixes.ToList());
    }

    private async Task AddSim2PrefixAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSim2Prefix)) return;
        if (Sim2Prefixes.Contains(NewSim2Prefix)) return;
        Sim2Prefixes.Add(NewSim2Prefix);
        await _prefs.SetSim2PrefixesAsync(Sim2Prefixes.ToList());
        NewSim2Prefix = string.Empty;
    }

    private async Task RemoveSim2PrefixAsync(string? prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return;
        Sim2Prefixes.Remove(prefix);
        await _prefs.SetSim2PrefixesAsync(Sim2Prefixes.ToList());
    }
}