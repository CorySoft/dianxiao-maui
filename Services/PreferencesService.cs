using System.Text.Json;

namespace DianxiaoMaui.Services;

/// <summary>偏好设置服务（类似 Android DataStore）</summary>
public sealed class PreferencesService
{
    private static readonly Lazy<PreferencesService> _instance = new(() => new PreferencesService());
    public static PreferencesService Instance => _instance.Value;

    private const string PrefsFile = "dianxiao_prefs.json";
    private readonly Dictionary<string, object> _cache = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _loaded;

    private PreferencesService() { }

    private async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        await _lock.WaitAsync();
        try
        {
            if (_loaded) return;
            var path = Path.Combine(FileSystem.AppDataDirectory, PrefsFile);
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (dict is not null)
                {
                    foreach (var kv in dict)
                    {
                        _cache[kv.Key] = kv.Value.GetRawText();
                    }
                }
            }
            _loaded = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveAsync()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, PrefsFile);
        var dict = _cache.ToDictionary(k => k.Key, v => JsonDocument.Parse(v.Value.ToString()!).RootElement);
        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<T> GetAsync<T>(string key, T defaultValue)
    {
        await EnsureLoadedAsync();
        await _lock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(key, out var val))
            {
                return JsonSerializer.Deserialize<T>(val.ToString()!)!;
            }
            return defaultValue;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        await EnsureLoadedAsync();
        await _lock.WaitAsync();
        try
        {
            _cache[key] = value!;
        }
        finally
        {
            _lock.Release();
        }
        await SaveAsync();
    }

    #region Keys
    // 拨号设置
    public const string INTERVAL_SEC = "interval_sec";            // 两次外呼间隔（秒）
    public const string INCALL_PACKAGE = "incall_package";        // 通话界面包名
    public const string AUTO_SPEAKER = "auto_speaker";            // 自动免提
    public const string AUTO_RECORDER = "auto_recorder";          // 自动录音
    public const string AUTO_HANGUP_UNANSWERED = "auto_hangup_unanswered"; // 未接通自动挂断
    public const string UNANSWERED_TIMEOUT = "unanswered_timeout"; // 未接通挂断超时
    public const string AUTO_HANGUP_ANSWERED = "auto_hangup_answered";   // 接通后自动挂断
    public const string ANSWERED_DURATION = "answered_duration";  // 接通后挂断时长
    public const string AUTO_CLICK_CALL = "auto_click_call";      // 拨号后自动点呼叫

    // 双卡前缀
    public const string SIM1_PREFIXES = "sim1_prefixes";          // 卡1前缀列表
    public const string SIM2_PREFIXES = "sim2_prefixes";          // 卡2前缀列表

    // 无障碍服务包名
    public const string ACCESSIBILITY_SERVICE = "accessibility_service";
    #endregion

    // 便捷属性
    public async Task<int> GetIntervalSecAsync() => await GetAsync(INTERVAL_SEC, 5);
    public async Task SetIntervalSecAsync(int v) => await SetAsync(INTERVAL_SEC, v);

    public async Task<string> GetIncallPackageAsync() => await GetAsync(INCALL_PACKAGE, "com.android.incallui");
    public async Task SetIncallPackageAsync(string v) => await SetAsync(INCALL_PACKAGE, v);

    public async Task<bool> GetAutoSpeakerAsync() => await GetAsync(AUTO_SPEAKER, true);
    public async Task SetAutoSpeakerAsync(bool v) => await SetAsync(AUTO_SPEAKER, v);

    public async Task<bool> GetAutoRecorderAsync() => await GetAsync(AUTO_RECORDER, false);
    public async Task SetAutoRecorderAsync(bool v) => await SetAsync(AUTO_RECORDER, v);

    public async Task<bool> GetAutoClickCallAsync() => await GetAsync(AUTO_CLICK_CALL, true);
    public async Task SetAutoClickCallAsync(bool v) => await SetAsync(AUTO_CLICK_CALL, v);

    public async Task<bool> GetAutoHangupUnansweredAsync() => await GetAsync(AUTO_HANGUP_UNANSWERED, true);
    public async Task SetAutoHangupUnansweredAsync(bool v) => await SetAsync(AUTO_HANGUP_UNANSWERED, v);

    public async Task<int> GetUnansweredTimeoutAsync() => await GetAsync(UNANSWERED_TIMEOUT, 30);
    public async Task SetUnansweredTimeoutAsync(int v) => await SetAsync(UNANSWERED_TIMEOUT, v);

    public async Task<bool> GetAutoHangupAnsweredAsync() => await GetAsync(AUTO_HANGUP_ANSWERED, false);
    public async Task SetAutoHangupAnsweredAsync(bool v) => await SetAsync(AUTO_HANGUP_ANSWERED, v);

    public async Task<int> GetAnsweredDurationAsync() => await GetAsync(ANSWERED_DURATION, 0);
    public async Task SetAnsweredDurationAsync(int v) => await SetAsync(ANSWERED_DURATION, v);

    public async Task<List<string>> GetSim1PrefixesAsync() => await GetAsync(SIM1_PREFIXES, new List<string>());
    public async Task SetSim1PrefixesAsync(List<string> v) => await SetAsync(SIM1_PREFIXES, v);

    public async Task<List<string>> GetSim2PrefixesAsync() => await GetAsync(SIM2_PREFIXES, new List<string>());
    public async Task SetSim2PrefixesAsync(List<string> v) => await SetAsync(SIM2_PREFIXES, v);
}