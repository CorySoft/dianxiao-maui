namespace DianxiaoMaui.Views.Features;

[QueryProperty(nameof(Title), "title")]
public partial class PlaceholderPage : ContentPage
{
    public new string Title { get; set; } = "敬请期待";

    public PlaceholderPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        TitleLabel.Text = Title;
        Shell.SetTitle(this, Title);
    }
}