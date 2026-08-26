using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DianxiaoMaui.Models;
using DianxiaoMaui.Services;

namespace DianxiaoMaui.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public CustomersViewModel()
    {
        SearchCommand = new AsyncRelayCommand(LoadAsync);
        AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
        CustomerSelectedCommand = new AsyncRelayCommand<Customer?>(OpenCustomerAsync);
    }

    public ICommand SearchCommand { get; }
    public ICommand AddCustomerCommand { get; }
    public ICommand CustomerSelectedCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var list = string.IsNullOrWhiteSpace(SearchText)
                ? await DatabaseService.Instance.GetAllCustomersAsync()
                : await DatabaseService.Instance.SearchCustomersAsync(SearchText);
            Customers.Clear();
            foreach (var c in list)
                Customers.Add(c);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("错误", ex.Message, "确定");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddCustomerAsync()
    {
        await Shell.Current.GoToAsync(nameof(ContactDetailPage),
            new Dictionary<string, object> { ["customerId"] = 0 });
    }

    private async Task OpenCustomerAsync(Customer? customer)
    {
        if (customer is null) return;
        await Shell.Current.GoToAsync(nameof(ContactDetailPage),
            new Dictionary<string, object> { ["customerId"] = customer.Id });
    }
}