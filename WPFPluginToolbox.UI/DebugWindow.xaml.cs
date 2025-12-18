using System;
using System.Windows;
using System.Windows.Controls;
using WPFPluginToolbox.Core;
using WPFPluginToolbox.Services;

namespace WPFPluginToolbox.UI;

/// <summary>
/// DebugWindow.xaml 的交互逻辑
/// </summary>
public partial class DebugWindow : Window
{
    private readonly LogService _logService;
    private List<LogEntry> _allLogs;
    
    public DebugWindow(LogService logService)
    {
        InitializeComponent();
        
        _logService = logService;
        _logService.LogRecorded += LogService_LogRecorded;
        
        // 初始化日志列表
        _allLogs = new List<LogEntry>(_logService.LogEntries);
        LogsDataGrid.ItemsSource = _allLogs;
    }
    
    /// <summary>
    /// 日志记录事件处理
    /// </summary>
    private void LogService_LogRecorded(object? sender, LogEntry e)
    {
        Dispatcher.Invoke(() =>
        {
            _allLogs.Add(e);
            LogsDataGrid.Items.Refresh();
            LogsDataGrid.ScrollIntoView(e);
        });
    }
    
    /// <summary>
    /// 清空日志按钮点击事件
    /// </summary>
    private void ClearLogs_Click(object sender, RoutedEventArgs e)
    {
        _logService.ClearLogs();
        _allLogs.Clear();
        LogsDataGrid.Items.Refresh();
    }
    
    /// <summary>
    /// 筛选日志按钮点击事件
    /// </summary>
    private void FilterLogs_Click(object sender, RoutedEventArgs e)
    {
        if (LogLevelComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            string filterTag = selectedItem.Tag?.ToString() ?? string.Empty;
            
            if (filterTag == "All")
            {
                // 显示所有日志
                LogsDataGrid.ItemsSource = _allLogs;
            }
            else
            {
                // 根据级别筛选日志
                if (Enum.TryParse(filterTag, out DebugLevel level))
                {
                    var filteredLogs = _logService.GetLogsByLevel(level);
                    LogsDataGrid.ItemsSource = filteredLogs;
                }
            }
        }
    }
    
    /// <summary>
    /// 窗口关闭事件处理
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        // 移除事件监听
        _logService.LogRecorded -= LogService_LogRecorded;
    }
}