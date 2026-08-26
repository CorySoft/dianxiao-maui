using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DianxiaoMaui.Models;
using DianxiaoMaui.Services;

namespace DianxiaoMaui.ViewModels;

public partial class ContactDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private Customer _customer = new();

    [ObservableProperty]
    private ObservableCollection<CallLog> _callLogs = new();

    [ObservableProperty]
    private bool _isNew;

    public List<string> IntentLevels { get; } = new() { "未分类", "低意向", "中意向", "高意向" };

    public ICommand SaveCommand { get; }
    public ICommand AddCallLogCommand { get; }

    private int _customerId;

    public ContactDetailViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        AddCallLogCommand = new AsyncRelayCommand(AddCallLogAsync);
    }

    public async Task InitializeAsync(int customerId)
    {
        _customerId = customerId;
        IsNew = customerId == 0;
        if (!IsNew)
        {
            var db = await DatabaseService.Instance.GetCustomerByIdAsync(customerId);
            if (db is not null)
            {
                Customer = db;
                await LoadLogsAsync(db.Phone);
            }
        }
    }

    private async Task LoadLogsAsync(string phone)
    {
        var logs = await DatabaseService.Instance.GetCallLogsByPhoneAsync(phone);
        CallLogs.Clear();
        foreach (var l in logs) CallLogs.Add(l);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Customer.Name))
        {
            await Shell.Current.DisplayAlert("提示", "请输入姓名", "确定");
            return;
        }
        if (string.IsNullOrWhiteSpace(Customer.Phone))
        {
            await Shell.Current.DisplayAlert("提示", "请输入电话", "确定");
            return;
        }

        try
        {
            if (IsNew)
                await DatabaseService.Instance.AddCustomerAsync(Customer);
            else
                await DatabaseService.Instance.UpdateCustomerAsync(Customer);

            await Shell.Current.DisplayAlert("已保存", "客户信息已保存", "确定");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("保存失败", ex.Message, "确定");
        }
    }

    private async Task AddCallLogAsync()
    {
        var phone = Customer.Phone;
        if (string.IsNullOrWhiteSpace(phone))
        {
            await Shell.Current.DisplayAlert("提示", "请先填写电话", "确定");
            return;
        }
        var connected = await Shell.Current.DisplayAlert("新增通话记录",
            "是否接通？", "已接通", "未接通");
        var durationStr = await Shell.Current.DisplayPromptAsync("通话时长", "输入通话时长（秒）", placeholder: "如 60");
        var duration = int.TryParse(durationStr, out var d) ? d : 0;

        var log = new CallLog
        {
            PhoneNumber = phone,
            CustomerName = Customer.Name,
            Connected = connected,
            DurationSec = duration,
            StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - duration * 1000,
            EndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        await DatabaseService.Instance.AddCallLogAsync(log);
        await LoadLogsAsync(phone);
    }
}