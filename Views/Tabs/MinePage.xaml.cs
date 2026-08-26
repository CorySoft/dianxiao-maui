namespace DianxiaoMaui.Views.Tabs;

public partial class MinePage : ContentPage
{
    public MinePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.MineViewModel vm)
            await vm.OnAppearingAsync();
    }
}