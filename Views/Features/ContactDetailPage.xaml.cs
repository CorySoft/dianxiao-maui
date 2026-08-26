using DianxiaoMaui.ViewModels;

namespace DianxiaoMaui.Views.Features;

[QueryProperty(nameof(CustomerId), "customerId")]
public partial class ContactDetailPage : ContentPage
{
    public ContactDetailPage()
    {
        InitializeComponent();
    }

    public int CustomerId { get; set; }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is ContactDetailViewModel vm)
            await vm.InitializeAsync(CustomerId);
    }
}