namespace DianxiaoMaui.Views.Features;

public partial class CallHistoryPage : ContentPage
{
    public CallHistoryPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.CallHistoryViewModel vm)
            await vm.LoadAsync();
    }
}