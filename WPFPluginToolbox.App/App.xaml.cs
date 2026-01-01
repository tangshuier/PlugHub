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
        
        try
        {
            // 添加全局异常处理
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            // 显示UI项目中的主窗口
            var mainWindow = new MainWindow();
            // 设置窗口为可见，确保窗口能够显示
            mainWindow.Visibility = Visibility.Visible;
            mainWindow.Show();
            mainWindow.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"应用程序启动失败: {ex.Message}\n\n堆栈跟踪: {ex.StackTrace}", "启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown();
        }
    }
    
    /// <summary>
    /// 处理UI线程未捕获异常
    /// </summary>
    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"UI线程异常: {e.Exception.Message}\n\n堆栈跟踪: {e.Exception.StackTrace}", "应用程序错误", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
    
    /// <summary>
    /// 处理非UI线程未捕获异常
    /// </summary>
    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show($"非UI线程异常: {ex.Message}\n\n堆栈跟踪: {ex.StackTrace}", "应用程序错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        this.Shutdown();
    }
}

