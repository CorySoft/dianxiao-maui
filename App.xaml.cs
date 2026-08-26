using DianxiaoMaui.Services;
using Microsoft.Maui.Controls;

namespace DianxiaoMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // 初始化数据库
        Task.Run(async () => await DatabaseService.Instance.InitAsync());
    }

    protected override Window CreateWindow(IWindow? window)
    {
        return new Window(new AppShell());
    }
}
