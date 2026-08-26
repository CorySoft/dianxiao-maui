using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DianxiaoMaui.Services;

namespace DianxiaoMaui.ViewModels;

public partial class DialSettingsViewModel : ObservableObject
{
    private readonly PreferencesService _prefs = PreferencesService.Instance;

    [ObservableProperty] private int _intervalSec;
    [ObservableProperty] private string _incallPackage = "com.android.incallui";
    [ObservableProperty] private bool _autoSpeaker;
    [ObservableProperty] private bool _autoRecorder;
    [ObservableProperty] private bool _autoClickCall;
    [ObservableProperty] private bool _autoHangupUnanswered;
    [ObservableProperty] private int _unansweredTimeout;
    [ObservableProperty] private bool _autoHangupAnswered;
    [ObservableProperty] private int _answeredDuration;

    public DialSettingsViewModel()
    {
        LoadAsync();
    }

    private async void LoadAsync()
    {
        IntervalSec = await _prefs.GetIntervalSecAsync();
        IncallPackage = await _prefs.GetIncallPackageAsync();
        AutoSpeaker = await _prefs.GetAutoSpeakerAsync();
        AutoRecorder = await _prefs.GetAutoRecorderAsync();
        AutoClickCall = await _prefs.GetAutoClickCallAsync();
        AutoHangupUnanswered = await _prefs.GetAutoHangupUnansweredAsync();
        UnansweredTimeout = await _prefs.GetUnansweredTimeoutAsync();
        AutoHangupAnswered = await _prefs.GetAutoHangupAnsweredAsync();
        AnsweredDuration = await _prefs.GetAnsweredDurationAsync();
    }

    partial void OnIntervalSecChanged(int value) => _ = _prefs.SetIntervalSecAsync(value);
    partial void OnIncallPackageChanged(string value) => _ = _prefs.SetIncallPackageAsync(value);
    partial void OnAutoSpeakerChanged(bool value) => _ = _prefs.SetAutoSpeakerAsync(value);
    partial void OnAutoRecorderChanged(bool value) => _ = _prefs.SetAutoRecorderAsync(value);
    partial void OnAutoClickCallChanged(bool value) => _ = _prefs.SetAutoClickCallAsync(value);
    partial void OnAutoHangupUnansweredChanged(bool value) => _ = _prefs.SetAutoHangupUnansweredAsync(value);
    partial void OnUnansweredTimeoutChanged(int value) => _ = _prefs.SetUnansweredTimeoutAsync(value);
    partial void OnAutoHangupAnsweredChanged(bool value) => _ = _prefs.SetAutoHangupAnsweredAsync(value);
    partial void OnAnsweredDurationChanged(int value) => _ = _prefs.SetAnsweredDurationAsync(value);
}