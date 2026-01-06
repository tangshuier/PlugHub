using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using WPFPluginToolbox.Core;
using WPFPluginToolbox.PluginSystem;
using WPFPluginToolbox.Services;
using WPFPluginToolbox.Services.Models;
namespace WPFPluginToolbox.UI;

/// <summary>
/// 转换器：根据滚动条方向返回IsDirectionReversed值
/// </summary>
public class OrientationToDirectionReversedConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // 对于垂直滚动条，返回true，确保滚动方向正确
        // 对于水平滚动条，返回false，保持默认行为
        return value is Orientation orientation && orientation == Orientation.Vertical;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

/// <summary>
/// 插件标签页项，用于管理多标签页中的插件实例
/// </summary>
public class PluginTabItem
{
    public required string Title { get; set; }
    public required object Content { get; set; }
    public required string PluginId { get; set; }
    public PluginMetadata? Metadata { get; set; }
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly LogService _logService;
        private readonly SettingsService _settingsService;
        private readonly ThemeService _themeService;
        private DebugWindow? _debugWindow;
        private GridLength _lastDebugHeight = new(200);
        private bool _isPluginPanelVisible = true;
        
        /// <summary>
        /// 属性变化事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// 触发属性变化事件
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    
        public MainWindow()
        {
            InitializeComponent();
            
            // 添加窗口状态改变事件处理程序
            this.StateChanged += MainWindow_StateChanged;
            // 添加窗口关闭事件处理程序，用于保存窗口大小
            this.Closing += MainWindow_Closing;
            
            // 初始化日志服务
            _logService = new LogService();
            _logService.LogRecorded += LogService_LogRecorded;
            
            // 初始化设置服务
            _settingsService = new SettingsService();
            
            // 初始化主题服务
            _themeService = new ThemeService(_settingsService);
            _themeService.ThemeChanged += OnThemeChanged;
            
            // 初始化插件管理器
            string pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            
            // 创建插件目录如果不存在
            if (!Directory.Exists(pluginsDirectory))
            {
                Directory.CreateDirectory(pluginsDirectory);
                _logService.Info($"创建了插件目录: {pluginsDirectory}");
            }
            
            PluginManager.Initialize(pluginsDirectory, _logService);
            
            // 监听插件加载事件
            PluginManager.Instance.PluginLoaded += Instance_PluginLoaded;
            PluginManager.Instance.PluginUnloaded += Instance_PluginUnloaded;
            
            // 自动加载插件
            try
            {
                PluginManager.Instance.LoadAllPlugins();
                UpdatePluginsList();
                _logService.Info("已自动加载所有插件");
            }
            catch (Exception ex)
            {
                _logService.Error($"自动加载插件失败: {ex.Message}");
            }
            
            // 加载设置并应用主题
            LoadSettings();
            
            // 初始化最大化按钮图标
            UpdateMaximizeButtonIcon();
            
            // 显示初始信息
            _logService.Info("WPF插件工具箱已启动");
            _logService.Info("当前插件目录: " + pluginsDirectory);
        }
        
        /// <summary>
        /// 窗口状态改变事件处理程序
        /// </summary>
        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            UpdateMaximizeButtonIcon();
        }
        
        /// <summary>
        /// 更新最大化按钮图标
        /// </summary>
        private void UpdateMaximizeButtonIcon()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Content = "▢";
            }
            else
            {
                MaximizeButton.Content = "□";
            }
        }
    
    /// <summary>
    /// 加载设置
    /// </summary>
    private void LoadSettings()
    {
        ToolboxSettings settings = _settingsService.GetSettings();
        
        // 应用主题
        ApplyTheme(settings.Theme);
        
        // 应用窗口大小
        if (settings.MainWindowWidth > 0 && settings.MainWindowHeight > 0)
        {
            this.Width = settings.MainWindowWidth;
            this.Height = settings.MainWindowHeight;
        }
        
        // 应用调试信息面板状态
        if (!settings.IsDebugPanelVisible)
        {
            // 如果设置为隐藏，调用ToggleDebugPanel方法隐藏面板
            ToggleDebugPanel();
        }
        
        // 应用插件栏状态
        if (!settings.IsPluginPanelVisible)
        {
            // 如果设置为隐藏，调用TogglePluginPanel方法隐藏插件栏
            // 直接模拟点击切换按钮的效果
            TogglePluginPanel();
        }
        
        // 如果设置了调试窗口默认打开，则打开调试窗口
        if (settings.IsDebugWindowDefaultOpen)
        {
            OpenDebugWindow_Click(null, null);
        }
    }
    
    /// <summary>
    /// 窗口关闭事件处理程序，用于保存窗口大小、调试信息面板状态和插件栏状态
    /// </summary>
    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // 获取当前设置
        ToolboxSettings settings = _settingsService.GetSettings();
        
        // 只在窗口处于正常状态时保存大小，避免保存最大化/最小化状态下的大小
        if (this.WindowState == WindowState.Normal)
        {
            // 更新窗口大小
            settings.MainWindowWidth = this.ActualWidth;
            settings.MainWindowHeight = this.ActualHeight;
            
            _logService.Info($"保存窗口大小: {settings.MainWindowWidth}x{settings.MainWindowHeight}");
        }
        
        // 保存调试信息面板状态
        settings.IsDebugPanelVisible = DebugRow.Height.Value > 0;
        _logService.Info($"保存调试信息面板状态: {settings.IsDebugPanelVisible}");
        
        // 保存插件栏状态
        settings.IsPluginPanelVisible = PluginContent.Visibility == Visibility.Visible;
        _logService.Info($"保存插件栏状态: {settings.IsPluginPanelVisible}");
        
        // 保存设置
        _settingsService.SaveSettings(settings);
    }
        
        /// <summary>
        /// 应用主题
        /// </summary>
        /// <param name="theme">主题枚举</param>
        /// <param name="isPreview">是否为预览模式，预览模式下不保存主题到设置文件</param>
        public void ApplyTheme(ToolboxTheme theme, bool isPreview = false)
        {
            // 使用主题服务设置主题，预览模式下不保存到设置文件
            _themeService.SetTheme(theme, !isPreview);
            
            // 应用主题到主窗口
            this.Background = _themeService.MainBackgroundBrush;
            this.Foreground = _themeService.MainForegroundBrush;
            
            // 应用主题到自定义标题栏
            TitleBar.Background = _themeService.ToolBarBackgroundBrush;
            
            // 更新标题栏中的TextBlock颜色
            foreach (var child in TitleBar.Children)
            {
                if (child is Border border)
                {
                    // Border只有一个Child属性，没有Children集合
                    if (border.Child is TextBlock textBlock)
                    {
                        textBlock.Foreground = _themeService.MainForegroundBrush;
                    }
                }
                else if (child is StackPanel stackPanel)
                {
                    // 更新标题栏按钮颜色
                    foreach (var button in stackPanel.Children)
                    {
                        if (button is Button btn)
                        {
                            btn.Foreground = _themeService.MainForegroundBrush;
                        }
                    }
                }
            }
            
            // 应用主题到主要网格
            if (this.Content is Grid mainGrid)
            {
                mainGrid.Background = _themeService.MainBackgroundBrush;
                
                // 更新所有分隔线颜色和子元素样式
                foreach (var child in mainGrid.Children)
                {
                    if (child is GridSplitter gridSplitter)
                    {
                        gridSplitter.Background = _themeService.BorderBrush;
                    }
                    // 更新工具栏样式
                    else if (child is ToolBar toolBar)
                    {
                        ApplyThemeToToolBar(toolBar);
                    }
                    // 更新状态栏样式
                    else if (child is StatusBar statusBar)
                    {
                        statusBar.Background = _themeService.ToolBarBackgroundBrush;
                        statusBar.Foreground = _themeService.MainForegroundBrush;
                    }
                    // 更新主内容区域网格中的分隔线
                    else if (child is Grid contentGrid)
                    {
                        foreach (var contentChild in contentGrid.Children)
                        {
                            if (contentChild is GridSplitter contentGridSplitter)
                            {
                                contentGridSplitter.Background = _themeService.BorderBrush;
                            }
                        }
                    }
                }
            }
            
            // 应用主题到插件栏容器
            PluginPanel.Background = _themeService.PluginPanelBackgroundBrush;
            
            // 应用主题到插件内容区域
            PluginContent.Background = _themeService.PluginPanelBackgroundBrush;
            
            // 应用主题到插件列表的DockPanel容器
            if (PluginContent.Children.Count > 0 && PluginContent.Children[0] is DockPanel dockPanel)
            {
                dockPanel.Background = _themeService.PluginPanelBackgroundBrush;
                
                // 更新"已加载插件"文字颜色
                foreach (var child in dockPanel.Children)
                {
                    if (child is Label label)
                    {
                        label.Foreground = Brushes.White;
                    }
                }
            }
            
            // 应用主题到插件列表
            PluginsListBox.Background = _themeService.PluginPanelBackgroundBrush;
            PluginsListBox.Foreground = _themeService.MainForegroundBrush;
            PluginsListBox.BorderBrush = _themeService.BorderBrush;
            
            // 更新插件列表项的样式
            foreach (var item in PluginsListBox.Items)
            {
                if (PluginsListBox.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem listBoxItem)
                {
                    listBoxItem.Background = _themeService.PluginPanelBackgroundBrush;
                    listBoxItem.Foreground = _themeService.MainForegroundBrush;
                }
            }
            
            // 应用主题到插件工作区网格
            PluginWorkspaceGrid.Background = _themeService.PluginWorkspaceBackgroundBrush;
            
            // 应用主题到插件标签页控件
            PluginWorkspaceTabs.Background = _themeService.PluginWorkspaceBackgroundBrush;
            PluginWorkspaceTabs.Foreground = _themeService.MainForegroundBrush;
            PluginWorkspaceTabs.BorderBrush = _themeService.BorderBrush;
            
            // 更新所有标签页的样式
            for (int i = 0; i < PluginWorkspaceTabs.Items.Count; i++)
            {
                var tabItem = PluginWorkspaceTabs.ItemContainerGenerator.ContainerFromIndex(i) as TabItem;
                if (tabItem != null)
                {
                    // 设置标签页背景和前景色
                    tabItem.Background = _themeService.PluginWorkspaceBackgroundBrush;
                    tabItem.Foreground = _themeService.MainForegroundBrush;
                    tabItem.BorderBrush = _themeService.BorderBrush;
                    
                    // 更新标签页标题中的文本和按钮颜色
                    if (tabItem.Header is DockPanel tabDockPanel)
                    {
                        foreach (var child in tabDockPanel.Children)
                        {
                            if (child is TextBlock textBlock)
                            {
                                textBlock.Foreground = _themeService.MainForegroundBrush;
                            }
                            else if (child is Button button)
                            {
                                button.Foreground = _themeService.MainForegroundBrush;
                            }
                        }
                    }
                }
            }
            
            // 更新插件列表的右键菜单样式
            // 注意：ContextMenu只有在显示时才会创建，所以这里无法直接获取
            // 我们需要确保样式绑定正确，或者在ContextMenu显示时更新
            
            // 应用主题到调试面板
            DebugPanel.Background = _themeService.DebugPanelBackgroundBrush;
            
            // 应用主题到调试信息文本框
            DebugInfoTextBox.Background = _themeService.DebugPanelBackgroundBrush;
            DebugInfoTextBox.Foreground = _themeService.MainForegroundBrush;
            
            // 更新文本颜色
            NoPluginSelectedText.Foreground = _themeService.MainForegroundBrush;
            
            // 更新插件栏按钮样式
            ClosePluginPanelButton.Background = _themeService.ToolBarBackgroundBrush;
            ClosePluginPanelButton.Foreground = _themeService.MainForegroundBrush;
            ClosePluginPanelButton.BorderBrush = _themeService.BorderBrush;
            
            // 更新显示插件栏按钮样式
            ShowPluginPanelButton.Background = _themeService.MainBackgroundBrush;
            ShowPluginPanelButton.Foreground = _themeService.MainForegroundBrush;
            ShowPluginPanelButton.BorderBrush = _themeService.BorderBrush;
        }
        
        /// <summary>
        /// 应用主题到工具栏
        /// </summary>
        /// <param name="toolBar">工具栏实例</param>
        private void ApplyThemeToToolBar(ToolBar toolBar)
        {
            toolBar.Background = _themeService.ToolBarBackgroundBrush;
            toolBar.Foreground = _themeService.MainForegroundBrush;
            
            // 更新工具栏中的按钮样式
            foreach (var item in toolBar.Items)
            {
                if (item is Button button)
                {
                    button.Background = _themeService.ToolBarBackgroundBrush;
                    button.Foreground = _themeService.MainForegroundBrush;
                }
            }
        }
        

        
        /// <summary>
        /// 主题变更事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="theme">新主题</param>
        private void OnThemeChanged(object? sender, ToolboxTheme theme)
        {
            // 应用主题到整个窗口
            ApplyTheme(theme);
            
            // 通知UI主题属性已变更
            OnPropertyChanged(nameof(MainBackgroundBrush));
            OnPropertyChanged(nameof(MainForegroundBrush));
            OnPropertyChanged(nameof(PluginWorkspaceBackgroundBrush));
            OnPropertyChanged(nameof(PluginPanelBackgroundBrush));
            OnPropertyChanged(nameof(ContextMenuBackgroundBrush));
            OnPropertyChanged(nameof(BorderBrush));
            
            // 通知所有插件主题变更
            // 这里可以添加逻辑来通知所有插件主题已变更
            // 例如，遍历所有插件并调用其API的主题变更事件
        }
    
    /// <summary>
    /// 打开设置界面
    /// </summary>
    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        if (CheckSettingsChanges())
        {
            // 检查设置窗口是否已在标签页中打开
            var existingSettingsTab = PluginWorkspaceTabs.Items.Cast<PluginTabItem>()
                .FirstOrDefault(tab => tab.Content is SettingsWindow);
            
            if (existingSettingsTab != null)
            {
                // 如果已打开，切换到该标签页
                PluginWorkspaceTabs.SelectedItem = existingSettingsTab;
            }
            else
            {
                // 创建新的设置窗口
                SettingsWindow settingsView = new();
                
                // 创建设置标签页
                var settingsTab = new PluginTabItem
                {
                    Title = "设置",
                    Content = settingsView,
                    PluginId = "settings",
                    Metadata = null
                };
                
                // 添加到标签页控件
                PluginWorkspaceTabs.Items.Add(settingsTab);
                // 选中新标签页
                PluginWorkspaceTabs.SelectedItem = settingsTab;
            }
            
            // 清除插件列表的选中状态，以便再次点击同一插件时能触发SelectionChanged事件
            PluginsListBox.SelectedItem = null;
        }
    }
    
    /// <summary>
    /// 插件加载事件处理
    /// </summary>
    private void Instance_PluginLoaded(object? sender, PluginEventArgs e)
    {
        if (e.Plugin != null)
        {
            _logService.Info($"插件已加载: {e.Plugin.Name} ({e.Plugin.Version})");
        }
    }
    
    /// <summary>
    /// 插件卸载事件处理
    /// </summary>
    private void Instance_PluginUnloaded(object? sender, PluginEventArgs e)
    {
        if (e.Plugin != null)
        {
            _logService.Info($"插件已卸载: {e.Plugin.Name} ({e.Plugin.Version})");
        }
    }
    
    /// <summary>
    /// 日志记录事件处理
    /// </summary>
    private void LogService_LogRecorded(object? sender, LogEntry e)
    {
        try
        {
            // 只检查Dispatcher状态，不访问UI属性，避免跨线程访问异常
            if (!this.Dispatcher.HasShutdownStarted && !this.Dispatcher.HasShutdownFinished)
            {
                // 使用BeginInvoke异步更新UI，避免阻塞日志线程
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // 在UI线程中再次检查DebugInfoTextBox是否可用
                        if (DebugInfoTextBox != null && this.IsLoaded)
                        {
                            DebugInfoTextBox.AppendText(e.ToString() + Environment.NewLine);
                            DebugInfoTextBox.ScrollToEnd();
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // 忽略UI元素不可用异常
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
        catch (TaskCanceledException)
        {
            // 忽略任务取消异常，这是正常的程序关闭行为
        }
        catch (InvalidOperationException)
        {
            // 忽略调度器已关闭异常
        }
        catch (Exception)
        {
            // 忽略其他所有异常，确保程序能正常关闭
        }
    }
    
    /// <summary>
    /// 加载插件按钮点击事件
    /// </summary>
    private void LoadPlugins_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PluginManager.Instance.LoadAllPlugins();
            UpdatePluginsList();
            _logService.Info("已加载所有插件");
        }
        catch (Exception ex)
        {
            _logService.Error($"加载插件失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 重新加载插件按钮点击事件
    /// </summary>
    private void ReloadPlugins_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PluginManager.Instance.ReloadPlugins();
            UpdatePluginsList();
            ClearPluginWorkspace();
            _logService.Info("已重新加载所有插件");
        }
        catch (Exception ex)
        {
            _logService.Error($"重新加载插件失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 卸载插件按钮点击事件
    /// </summary>
    private void UnloadPlugins_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PluginManager.Instance.UnloadAllPlugins();
            UpdatePluginsList();
            ClearPluginWorkspace();
            _logService.Info("已卸载所有插件");
        }
        catch (Exception ex)
        {
            _logService.Error($"卸载插件失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 打开调试窗口按钮点击事件
    /// </summary>
    private void OpenDebugWindow_Click(object? sender, RoutedEventArgs? e)
    {
        if (_debugWindow == null || !_debugWindow.IsLoaded)
        {
            _debugWindow = new DebugWindow(_logService);
            _debugWindow.Show();
        }
        else
        {
            _debugWindow.Activate();
        }
    }
    
    /// <summary>
    /// 清空调试信息按钮点击事件
    /// </summary>
    private void ClearDebugInfo_Click(object sender, RoutedEventArgs e)
    {
        DebugInfoTextBox.Clear();
        _logService.ClearLogs();
    }
    
    /// <summary>
        /// 插件列表选择变更事件
        /// </summary>
        private void PluginsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PluginsListBox.SelectedItem is PluginMetadata selectedPluginMetadata)
            {
                if (selectedPluginMetadata.IsLoaded && !string.IsNullOrEmpty(selectedPluginMetadata.Id))
                {
                    // 获取已加载的插件实例
                    var loadedPlugin = PluginManager.Instance.GetPluginById(selectedPluginMetadata.Id);
                    if (loadedPlugin != null)
                    {
                        ShowPluginUI(loadedPlugin);
                        return;
                    }
                }
                // 如果是已卸载插件或获取失败，清空工作区
                ClearPluginWorkspace();
            }
            else
            {
                ClearPluginWorkspace();
            }
        }
    
    /// <summary>
    /// 显示插件UI
    /// </summary>
    /// <param name="plugin">插件实例</param>
    private void ShowPluginUI(IPlugin plugin)
    {
        try
        {
            if (CheckSettingsChanges())
            {
                var pluginView = plugin.GetMainView();
                if (pluginView != null)
                {
                    // 获取插件元数据
                    var metadata = PluginManager.Instance.GetPluginMetadataById(plugin.Id);
                    if (metadata != null)
                    {
                        // 检查该插件是否已在标签页中打开
                    var existingTab = PluginWorkspaceTabs.Items.Cast<PluginTabItem>()
                        .FirstOrDefault(tab => tab.PluginId == plugin.Id);
                        
                        if (existingTab != null)
                        {
                            // 如果已打开，切换到该标签页
                            PluginWorkspaceTabs.SelectedItem = existingTab;
                        }
                        else
                        {
                            // 创建新标签页
                    var newTab = new PluginTabItem
                    {
                        Title = metadata.Name,
                        Content = pluginView,
                        PluginId = plugin.Id,
                        Metadata = metadata
                    };
                            
                            // 添加到标签页控件
                            PluginWorkspaceTabs.Items.Add(newTab);
                            // 选中新标签页
                            PluginWorkspaceTabs.SelectedItem = newTab;
                        }
                        
                        NoPluginSelectedText.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    _logService.Error($"插件 {plugin.Id} 未提供主视图");
                }
            }
        }
        catch (Exception ex)
        {
            _logService.Error($"显示插件UI失败: {ex.Message}");
        }
    }
    
    /// <summary>
        /// 清空插件工作区
        /// </summary>
        private void ClearPluginWorkspace()
        {
            // 检查当前选中的标签页是否是设置页面
            if (PluginWorkspaceTabs.SelectedItem is PluginTabItem selectedTab && selectedTab.Content is SettingsWindow)
            {
                return;
            }
            
            if (CheckSettingsChanges())
            {
                // 如果没有标签页，显示提示文本
                if (PluginWorkspaceTabs.Items.Count == 0)
                {
                    NoPluginSelectedText.Visibility = Visibility.Visible;
                }
            }
        }
        
        /// <summary>
        /// 插件列表鼠标右键点击事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void PluginsListBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null) return;
            
            var hitTestResult = VisualTreeHelper.HitTest(listBox, e.GetPosition(listBox));
            
            // 检查点击的是否是空白区域
            if (hitTestResult == null || !(hitTestResult.VisualHit is ListBoxItem))
            {
                // 点击的是空白区域，取消选择并显示空白区域的上下文菜单
                listBox.SelectedItem = null;
                var contextMenu = listBox.FindResource("EmptySpaceContextMenu") as ContextMenu;
                if (contextMenu != null)
                {
                    contextMenu.PlacementTarget = listBox;
                    contextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            else
            {
                // 点击的是插件项，显示插件项的上下文菜单
                // 这里不需要处理，因为ListBoxItem会自动处理ContextMenu
            }
        }
        
        /// <summary>
        /// 重载所有插件菜单项点击事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void ReloadAllPluginsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logService.Info("正在重载所有插件");
                PluginManager.Instance.ReloadPlugins();
                UpdatePluginsList();
                ClearPluginWorkspace();
                _logService.Info("已重新加载所有插件");
            }
            catch (Exception ex)
            {
                _logService.Error($"重新加载插件失败: {ex.Message}");
            }
        }
    
    /// <summary>
        /// 卸载插件菜单项点击事件
        /// </summary>
        private void UnloadPluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PluginsListBox.SelectedItem is PluginMetadata selectedPluginMetadata)
            {
                try
                {
                    if (selectedPluginMetadata.IsLoaded && !string.IsNullOrEmpty(selectedPluginMetadata.Id))
                    {
                        bool success = PluginManager.Instance.UnloadPlugin(selectedPluginMetadata.Id);
                        if (success)
                        {
                            _logService.Info($"成功卸载插件: {selectedPluginMetadata.Name}");
                            UpdatePluginsList();
                            ClearPluginWorkspace();
                        }
                        else
                        {
                            _logService.Error($"卸载插件失败: {selectedPluginMetadata.Name}");
                        }
                    }
                    else
                    {
                        _logService.Warning($"插件 '{selectedPluginMetadata.Name}' 已处于卸载状态");
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error($"卸载插件时发生异常: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 重载插件菜单项点击事件
        /// </summary>
        private void ReloadPluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PluginsListBox.SelectedItem is PluginMetadata selectedPluginMetadata)
            {
                try
                {
                    _logService.Info($"正在重载插件: {selectedPluginMetadata.Name}");
                    if (!string.IsNullOrEmpty(selectedPluginMetadata.Id))
                    {
                        bool success = PluginManager.Instance.ReloadPlugin(selectedPluginMetadata.Id);
                        if (success)
                        {
                            _logService.Info($"成功重载插件: {selectedPluginMetadata.Name}");
                            UpdatePluginsList();
                            // 重新显示插件UI
                            var reloadedPlugin = PluginManager.Instance.GetPluginById(selectedPluginMetadata.Id);
                            if (reloadedPlugin != null)
                            {
                                ShowPluginUI(reloadedPlugin);
                            }
                            else
                            {
                                ClearPluginWorkspace();
                            }
                        }
                        else
                        {
                            _logService.Error($"重载插件失败: {selectedPluginMetadata.Name}");
                        }
                    }
                    else
                    {
                        _logService.Error($"重载插件失败: 插件ID为空");
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error($"重载插件时发生异常: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 导出插件菜单项点击事件
        /// </summary>
        private void ExportPluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PluginsListBox.SelectedItem is PluginMetadata selectedPluginMetadata)
            {
                try
                {
                    // 使用SaveFileDialog让用户选择导出路径
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new()
                    {
                        FileName = $"{selectedPluginMetadata.Name}_{selectedPluginMetadata.Version}",
                        DefaultExt = ".dll",
                        Filter = "插件文件 (*.dll)|*.dll|所有文件 (*.*)|*.*"
                    };
                    
                    bool? result = saveFileDialog.ShowDialog();
                    if (result == true)
                    {
                        string exportPath = saveFileDialog.FileName;
                        if (!string.IsNullOrEmpty(selectedPluginMetadata.Id))
                        {
                            bool success = PluginManager.Instance.ExportPlugin(selectedPluginMetadata.Id, exportPath);
                            if (success)
                            {
                                _logService.Info($"成功导出插件: {selectedPluginMetadata.Name} 到 {exportPath}");
                            }
                            else
                            {
                                _logService.Error($"导出插件失败: {selectedPluginMetadata.Name}");
                            }
                        }
                        else
                        {
                            _logService.Error($"导出插件失败: 插件ID为空");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error($"导出插件时发生异常: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 删除插件菜单项点击事件
        /// </summary>
        private void DeletePluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PluginsListBox.SelectedItem is PluginMetadata selectedPluginMetadata)
            {
                _logService.Info($"=== 删除插件流程开始 ===");
                _logService.Info($"选中的插件: {selectedPluginMetadata.Name} (ID: {selectedPluginMetadata.Id})");
                _logService.Info($"插件路径: {selectedPluginMetadata.PluginPath}");
                _logService.Info($"插件是否已加载: {selectedPluginMetadata.IsLoaded}");
                
                // 显示确认对话框
                MessageBoxResult result = MessageBox.Show(
                    $"确定要删除插件 '{selectedPluginMetadata.Name}' 吗？此操作无法撤销。",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _logService.Info($"用户确认删除，调用DeletePlugin方法");
                        if (!string.IsNullOrEmpty(selectedPluginMetadata.Id))
                        {
                            bool success = PluginManager.Instance.DeletePlugin(selectedPluginMetadata.Id);
                            if (success)
                            {
                                _logService.Info($"=== 删除插件成功 ===");
                                UpdatePluginsList();
                                
                                // 关闭所有与被删除插件相关的标签页
                                var pluginId = selectedPluginMetadata.Id;
                                var tabsToRemove = PluginWorkspaceTabs.Items.Cast<PluginTabItem>()
                                    .Where(tab => tab.PluginId == pluginId)
                                    .ToList();
                                
                                foreach (var tab in tabsToRemove)
                                {
                                    PluginWorkspaceTabs.Items.Remove(tab);
                                }
                                
                                // 如果没有标签页了，显示提示文本
                                if (PluginWorkspaceTabs.Items.Count == 0)
                                {
                                    NoPluginSelectedText.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                _logService.Error($"=== 删除插件失败 ===");
                            }
                        }
                        else
                        {
                            _logService.Error($"=== 删除插件失败：插件ID为空 ===");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Error($"=== 删除插件时发生异常 ===");
                        _logService.Error($"异常信息: {ex.Message}");
                        _logService.Error($"异常堆栈: {ex.StackTrace}");
                    }
                }
                else
                {
                    _logService.Info($"用户取消删除操作");
                }
                _logService.Info($"=== 删除插件流程结束 ===");
            }
            else
            {
                _logService.Warning("未选中任何插件进行删除");
            }
        }
        
        /// <summary>
        /// 配置插件菜单项点击事件
        /// </summary>
        private void ConfigurePluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PluginsListBox.SelectedItem is PluginMetadata selectedPluginMetadata)
            {
                try
                {
                    _logService.Info($"=== 配置插件开始 ===");
                    _logService.Info($"选中的插件: {selectedPluginMetadata.Name} (ID: {selectedPluginMetadata.Id})");
                    
                    if (!string.IsNullOrEmpty(selectedPluginMetadata.Id) && !string.IsNullOrEmpty(selectedPluginMetadata.PluginPath))
                    {
                        // 构造配置文件路径
                        string pluginsDirectory = Path.GetDirectoryName(selectedPluginMetadata.PluginPath) ?? string.Empty;
                        string configDir = Path.Combine(pluginsDirectory, selectedPluginMetadata.Id);
                        string configPath = Path.Combine(configDir, "config.json");
                        
                        _logService.Info($"配置文件路径: {configPath}");
                        
                        // 确保配置目录存在
                        if (!Directory.Exists(configDir))
                        {
                            Directory.CreateDirectory(configDir);
                            _logService.Info($"创建了配置目录: {configDir}");
                        }
                        
                        // 如果配置文件不存在，创建一个空的JSON文件
                        if (!File.Exists(configPath))
                        {
                            File.WriteAllText(configPath, "{\n  // 插件配置文件\n  // 在JSON格式中配置插件参数\n}", System.Text.Encoding.UTF8);
                            _logService.Info($"创建了新的配置文件: {configPath}");
                        }
                        
                        // 使用默认应用程序打开配置文件
                        _logService.Info($"打开配置文件: {configPath}");
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(configPath) { UseShellExecute = true });
                    }
                    else
                    {
                        _logService.Error($"配置插件失败：插件ID或路径为空");
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error($"配置插件时发生异常");
                    _logService.Error($"异常信息: {ex.Message}");
                    _logService.Error($"异常堆栈: {ex.StackTrace}");
                    MessageBox.Show($"打开配置文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _logService.Warning("未选中任何插件进行配置");
            }
        }
        
        /// <summary>
        /// 导入插件按钮点击事件
        /// </summary>
        private void ImportPlugins_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 使用OpenFileDialog让用户选择要导入的插件文件
                Microsoft.Win32.OpenFileDialog openFileDialog = new()
                {
                    Multiselect = true,
                    DefaultExt = ".dll",
                    Filter = "插件文件 (*.dll)|*.dll|所有文件 (*.*)|*.*",
                    Title = "选择要导入的插件文件"
                };
                
                bool? result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    // 使用与初始化时相同的插件目录路径
                    string pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                    int importedCount = 0;
                    
                    foreach (string selectedFile in openFileDialog.FileNames)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(selectedFile);
                            string destinationPath = Path.Combine(pluginsDirectory, fileName);
                            
                            // 检查目标文件是否已存在
                            if (File.Exists(destinationPath))
                            {
                                // 显示覆盖确认对话框
                                MessageBoxResult overwriteResult = MessageBox.Show(
                                    $"插件 '{fileName}' 已存在，是否覆盖？",
                                    "确认覆盖",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);
                                
                                if (overwriteResult != MessageBoxResult.Yes)
                                {
                                    _logService.Info($"跳过导入: {fileName} (已存在，用户选择不覆盖)");
                                    continue;
                                }
                            }
                            
                            // 复制文件到插件目录
                            File.Copy(selectedFile, destinationPath, true);
                            importedCount++;
                            _logService.Info($"成功导入插件: {fileName} 到 {pluginsDirectory}");
                        }
                        catch (Exception ex)
                        {
                            _logService.Error($"导入插件失败: {selectedFile} - {ex.Message}");
                        }
                    }
                    
                    if (importedCount > 0)
                    {
                        // 重新加载所有插件，以加载新导入的插件
                        PluginManager.Instance.ReloadPlugins();
                        UpdatePluginsList();
                        ClearPluginWorkspace();
                        
                        _logService.Info($"成功导入 {importedCount} 个插件");
                        MessageBox.Show(
                            $"成功导入 {importedCount} 个插件",
                            "导入成功",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        _logService.Info("未导入任何插件");
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error($"导入插件过程中发生异常: {ex.Message}");
                MessageBox.Show(
                    $"导入插件失败: {ex.Message}",
                    "导入失败",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 更新插件列表，显示所有插件（包括已加载和已卸载的）
        /// </summary>
        private void UpdatePluginsList()
        {
            // 获取所有插件（包括已加载和已卸载的）
            var allPlugins = PluginManager.Instance.GetAllPlugins();
            
            // 如果插件列表为空，尝试重新加载所有插件
            if (allPlugins.Count == 0)
            {
                _logService.Info($"=== UpdatePluginsList: Plugin list is empty, trying to reload all plugins");
                try
                {
                    PluginManager.Instance.LoadAllPlugins();
                    allPlugins = PluginManager.Instance.GetAllPlugins();
                    _logService.Info($"=== UpdatePluginsList: Reloaded {allPlugins.Count} plugins");
                }
                catch (Exception ex)
                {
                    _logService.Error($"=== UpdatePluginsList: Error reloading plugins: {ex.Message}");
                }
            }
            
            PluginsListBox.ItemsSource = allPlugins;
            _logService.Info($"=== UpdatePluginsList: Updated plugin list with {allPlugins.Count} plugins");
        }
        
        /// <summary>
        /// 切换插件栏显示/隐藏状态
        /// </summary>
        private void TogglePluginPanel_Click(object sender, RoutedEventArgs e)
        {
            TogglePluginPanel();
        }
        
        /// <summary>
        /// 切换插件栏显示/隐藏状态的核心逻辑
        /// </summary>
        private void TogglePluginPanel()
        {
            try
            {
                // 直接切换状态标志
                _isPluginPanelVisible = !_isPluginPanelVisible;
                
                if (_isPluginPanelVisible)
                {
                    // 显示插件栏
                    PluginPanel.Visibility = Visibility.Visible;
                    PluginSplitter.Visibility = Visibility.Visible;
                    ShowPluginPanelButton.Visibility = Visibility.Collapsed;
                    ClosePluginPanelButton.Content = "«";
                    ClosePluginPanelButton.ToolTip = "隐藏插件栏";
                    // 将插件栏宽度恢复到默认值
                    PluginColumn.Width = new GridLength(250);
                }
                else
                {
                    // 隐藏插件栏
                    PluginPanel.Visibility = Visibility.Collapsed;
                    PluginSplitter.Visibility = Visibility.Collapsed;
                    ShowPluginPanelButton.Visibility = Visibility.Collapsed;
                    ClosePluginPanelButton.Content = "»";
                    ClosePluginPanelButton.ToolTip = "显示插件栏";
                    // 将插件栏宽度设置为0
                    PluginColumn.Width = new GridLength(0);
                }
                
                // 强制刷新布局
                MainContentGrid.UpdateLayout();
            }
            catch (Exception ex)
            {
                _logService.Error($"切换插件栏状态失败: {ex.Message}");
                // 确保插件栏可以重新打开
                _isPluginPanelVisible = true;
                PluginPanel.Visibility = Visibility.Visible;
                PluginSplitter.Visibility = Visibility.Visible;
                ClosePluginPanelButton.Content = "«";
                ClosePluginPanelButton.ToolTip = "隐藏插件栏";
                // 重置插件栏宽度
                PluginColumn.Width = new GridLength(250);
                // 强制刷新布局
                MainContentGrid.UpdateLayout();
            }
        }
        
        /// <summary>
        /// 切换调试信息面板显示状态
        /// </summary>
        private void CollapseDebugPanel_Click(object sender, RoutedEventArgs e)
        {
            // 切换调试信息面板显示状态
            ToggleDebugPanel();
        }
        
        /// <summary>
        /// 切换调试信息面板显示状态
        /// </summary>
        private void ShowDebugPanel_Click(object sender, RoutedEventArgs e)
        {
            // 切换调试信息面板显示状态
            ToggleDebugPanel();
        }
        
        /// <summary>
        /// 切换调试信息面板显示状态的核心逻辑
        /// </summary>
        private void ToggleDebugPanel()
        {
            if (DebugRow.Height.Value > 0)
            {
                // 当前是打开状态，保存当前高度后关闭
                _lastDebugHeight = DebugRow.Height;
                DebugRow.Height = new GridLength(0);
            }
            else
            {
                // 当前是关闭状态，恢复上一次的高度
                DebugRow.Height = _lastDebugHeight;
            }
        }
        
        /// <summary>
        /// 检查设置变更并提示保存
        /// </summary>
        /// <returns>是否允许继续操作</returns>
        private bool CheckSettingsChanges()
        {
            // 检查当前选中的标签页内容是否是设置页面，如果是则检查是否有变更
            if (PluginWorkspaceTabs.SelectedItem is PluginTabItem selectedTab && 
                selectedTab.Content is SettingsWindow settingsView && 
                settingsView.HasChanges)
            {
                // 显示保存提示
                MessageBoxResult result = MessageBox.Show(
                    "设置已变更，是否保存？",
                    "保存设置",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // 保存设置
                        settingsView.SaveSettings();
                        return true;
                    case MessageBoxResult.No:
                        // 不保存，恢复原始设置
                        settingsView.RevertSettings();
                        return true;
                    case MessageBoxResult.Cancel:
                        // 取消操作
                        return false;
                    default:
                        return true;
                }
            }
            return true;
        }
        
        #region 标题栏事件处理
        
        /// <summary>
        /// 标题栏拖拽事件
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    // 如果是最大化状态，先恢复到正常状态，然后再拖拽
                    this.WindowState = WindowState.Normal;
                    // 计算鼠标位置，以便拖拽时窗口能正确定位
                    Point mousePosition = e.GetPosition(this);
                    this.Left = mousePosition.X - (this.ActualWidth / 2);
                    this.Top = mousePosition.Y;
                }
                this.DragMove();
            }
        }
        
        /// <summary>
        /// 最小化按钮点击事件
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        
        /// <summary>
        /// 最大化/还原按钮点击事件
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaximizeButton.Content = "□";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaximizeButton.Content = "▢";
            }
        }
        
        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        #endregion
        
        /// <summary>
        /// 标签页选择变更事件处理
        /// </summary>
        private void PluginWorkspaceTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 根据标签页是否存在，控制提示文本的显示
            if (PluginWorkspaceTabs.Items.Count == 0)
            {
                NoPluginSelectedText.Visibility = Visibility.Visible;
            }
            else
            {
                NoPluginSelectedText.Visibility = Visibility.Collapsed;
            }
        }
        
        /// <summary>
        /// 关闭插件标签页事件处理
        /// </summary>
        private void ClosePluginTab_Click(object sender, RoutedEventArgs e)
        {
            // 获取点击的按钮
            var button = sender as Button;
            if (button == null) return;
            
            // 获取对应的TabItem
            var tabItem = button.DataContext as PluginTabItem;
            if (tabItem == null) return;
            
            // 检查是否可以关闭（主要是检查设置页面的变更）
            if (CheckSettingsChanges())
            {
                // 移除标签页
                PluginWorkspaceTabs.Items.Remove(tabItem);
            }
        }
        
        /// <summary>
        /// 获取主题服务实例，供设置窗口使用
        /// </summary>
        /// <returns>主题服务实例</returns>
        public ThemeService GetThemeService()
        {
            return _themeService;
        }
        
        // 主题属性，供XAML绑定使用
        public Brush MainBackgroundBrush => _themeService.MainBackgroundBrush;
        public Brush MainForegroundBrush => _themeService.MainForegroundBrush;
        public Brush PluginWorkspaceBackgroundBrush => _themeService.PluginWorkspaceBackgroundBrush;
        public Brush PluginPanelBackgroundBrush => _themeService.PluginPanelBackgroundBrush;
        public Brush ContextMenuBackgroundBrush => _themeService.ContextMenuBackgroundBrush;
        public new Brush BorderBrush => _themeService.BorderBrush;
    }