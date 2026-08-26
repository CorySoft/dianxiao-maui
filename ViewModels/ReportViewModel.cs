using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DianxiaoMaui.Services;

namespace DianxiaoMaui.ViewModels;

public partial class ReportViewModel : ObservableObject
{
    [ObservableProperty] private int _total;
    [ObservableProperty] private int _connected;
    [ObservableProperty] private int _unconnected;
    [ObservableProperty] private int _totalDuration;

    public string TotalDurationText => $"{TotalDuration / 60}分{TotalDuration % 60}秒";
    public double ConnectionRate => Total > 0 ? (double)Connected / Total : 0;

    public ReportViewModel()
    {
        LoadAsync();
    }

    private async void LoadAsync()
    {
        var (t, c, u, d) = await DatabaseService.Instance.GetStatsAsync();
        Total = t;
        Connected = c;
        Unconnected = u;
        TotalDuration = d;
    }
}