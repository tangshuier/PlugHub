using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WPFPluginToolbox.Core;
using WPFPluginToolbox.PluginSystem;
using WPFPluginToolbox.Services;

namespace WPFPluginToolbox.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
    {
        private readonly LogService _logService;
        private DebugWindow? _debugWindow;
        private GridLength _lastDebugHeight = new(200);
    
    public MainWindow()
    {
        InitializeComponent();
        
        // 初始化日志服务
        _logService = new LogService();
        _logService.LogRecorded += LogService_LogRecorded;
        
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
        
        // 显示初始信息
        _logService.Info("WPF插件工具箱已启动");
        _logService.Info("当前插件目录: " + pluginsDirectory);
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
        Dispatcher.Invoke(() =>
        {
            DebugInfoTextBox.AppendText(e.ToString() + Environment.NewLine);
            DebugInfoTextBox.ScrollToEnd();
        });
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
    private void OpenDebugWindow_Click(object sender, RoutedEventArgs e)
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
            var pluginView = plugin.GetMainView();
            if (pluginView != null)
            {
                PluginWorkspace.Content = pluginView;
                NoPluginSelectedText.Visibility = Visibility.Collapsed;
            }
            else
            {
                ClearPluginWorkspace();
            }
        }
        catch (Exception ex)
        {
            _logService.Error($"显示插件UI失败: {ex.Message}");
            ClearPluginWorkspace();
        }
    }
    
    /// <summary>
    /// 清空插件工作区
    /// </summary>
    private void ClearPluginWorkspace()
    {
        PluginWorkspace.Content = null;
        NoPluginSelectedText.Visibility = Visibility.Visible;
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
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new();
                    saveFileDialog.FileName = $"{selectedPluginMetadata.Name}_{selectedPluginMetadata.Version}";
                    saveFileDialog.DefaultExt = ".dll";
                    saveFileDialog.Filter = "插件文件 (*.dll)|*.dll|所有文件 (*.*)|*.*";
                    
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
                                ClearPluginWorkspace();
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
        /// 导入插件按钮点击事件
        /// </summary>
        private void ImportPlugins_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 使用OpenFileDialog让用户选择要导入的插件文件
                Microsoft.Win32.OpenFileDialog openFileDialog = new();
                openFileDialog.Multiselect = true;
                openFileDialog.DefaultExt = ".dll";
                openFileDialog.Filter = "插件文件 (*.dll)|*.dll|所有文件 (*.*)|*.*";
                openFileDialog.Title = "选择要导入的插件文件";
                
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
            PluginsListBox.ItemsSource = allPlugins;
        }
        
        /// <summary>
        /// 切换插件栏显示/隐藏状态
        /// </summary>
        private void TogglePluginPanel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button toggleButton)
            {
                // 获取主内容区域的Grid
                Grid? mainGrid = PluginPanel.Parent as Grid;
                if (mainGrid != null && mainGrid.ColumnDefinitions.Count > 0)
                {
                    ColumnDefinition pluginColumn = mainGrid.ColumnDefinitions[0];
                    
                    if (PluginContent.Visibility == Visibility.Visible)
                    {
                        // 隐藏插件内容：只隐藏插件内容，保持切换按钮可见
                        PluginContent.Visibility = Visibility.Collapsed;
                        toggleButton.Content = "←";
                        toggleButton.ToolTip = "显示插件栏";
                        // 将插件栏宽度调整为只显示按钮
                        pluginColumn.Width = new GridLength(20);
                    }
                    else
                    {
                        // 显示插件内容：恢复插件内容显示
                        PluginContent.Visibility = Visibility.Visible;
                        toggleButton.Content = "→";
                        toggleButton.ToolTip = "隐藏插件栏";
                        // 将插件栏宽度恢复到默认值
                        pluginColumn.Width = new GridLength(250);
                    }
                }
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
    }