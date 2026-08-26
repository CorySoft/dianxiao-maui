using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DianxiaoMaui.Models;
using DianxiaoMaui.Services;

namespace DianxiaoMaui.ViewModels;

public partial class CallHistoryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CallLog> _logs = new();

    [ObservableProperty]
    private int _selectedFilter = 0;

    public ICommand FilterCommand { get; }

    public CallHistoryViewModel()
    {
        FilterCommand = new AsyncRelayCommand<int>(LoadAsync);
    }

    public async Task LoadAsync(int filter = 0)
    {
        SelectedFilter = filter;
        var list = await DatabaseService.Instance.GetCallLogsAsync(filter);
        Logs.Clear();
        foreach (var l in list) Logs.Add(l);
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(SelectedFilter))
        {
            _ = LoadAsync(SelectedFilter);
        }
    }
}