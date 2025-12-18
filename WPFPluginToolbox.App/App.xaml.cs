using System.Configuration;
using System.Data;
using System.Windows;
using WPFPluginToolbox.UI;

namespace WPFPluginToolbox.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // 显示UI项目中的主窗口
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}

