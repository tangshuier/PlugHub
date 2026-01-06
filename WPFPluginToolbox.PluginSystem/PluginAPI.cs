using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WPFPluginToolbox.Core;
using WPFPluginToolbox.Services;
using WPFPluginToolbox.Services.Models;

namespace WPFPluginToolbox.PluginSystem
{
    /// <summary>
    /// 插件API实现类，为插件提供各种功能接口
    /// </summary>
    public class PluginAPI : IPluginAPI
    {
        #region 基础信息

        /// <summary>
        /// 插件ID
        /// </summary>
        public string PluginId { get; internal set; }

        /// <summary>
        /// 插件名称
        /// </summary>
        public string PluginName { get; internal set; }

        /// <summary>
        /// 插件路径
        /// </summary>
        public string PluginPath { get; internal set; }

        #endregion

        #region 私有字段

        /// <summary>
        /// 插件加载器实例
        /// </summary>
        private readonly PluginLoader _pluginLoader;
        
        /// <summary>
        /// JSON序列化选项，静态实例以重用
        /// </summary>
        private static readonly System.Text.Json.JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true
        };
        
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private string ConfigFilePath
        {
            get
            {
                return Path.Combine(ConfigDirectory, "config.json");
            }
        }
        
        /// <summary>
        /// 插件是否同步工具箱主题
        /// </summary>
        private bool _syncToolboxTheme = true;
        
        /// <summary>
        /// 主题服务实例（来自主窗口，所有插件共享）
        /// </summary>
        private ThemeService? _themeService;
        
        /// <summary>
        /// 设置服务实例
        /// </summary>
        private readonly SettingsService _settingsService;
        
        /// <summary>
        /// 数据共享服务实例
        /// </summary>
        private readonly SimpleDataShareService _dataShareService;
        
        /// <summary>
        /// 事件总线实例
        /// </summary>
        private readonly LightweightEventBus _eventBus;
        
        /// <summary>
        /// 性能监控服务实例
        /// </summary>
        private readonly PerformanceMonitor _performanceMonitor;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pluginLoader">插件加载器实例</param>
        public PluginAPI(PluginLoader pluginLoader)
        {
            // 初始化默认值
            PluginId = string.Empty;
            PluginName = string.Empty;
            PluginPath = string.Empty;
            _pluginLoader = pluginLoader;
            
            // 初始化服务实例
            _settingsService = new SettingsService();
            _dataShareService = new SimpleDataShareService();
            _eventBus = new LightweightEventBus();
            _performanceMonitor = new PerformanceMonitor();
            
            // 开始监控当前插件性能
            _performanceMonitor.StartMonitoring(PluginId, PluginName);
        }
        
        /// <summary>
        /// 设置主题服务实例（从外部注入）
        /// </summary>
        /// <param name="themeService">主题服务实例</param>
        public void SetThemeService(ThemeService themeService)
        {
            // 移除旧的事件订阅
            if (_themeService != null)
            {
                _themeService.ThemeChanged -= OnThemeChanged;
            }
            
            // 设置新的主题服务实例
            _themeService = themeService;
            
            // 订阅新的事件
            if (_themeService != null)
            {
                _themeService.ThemeChanged += OnThemeChanged;
            }
        }

        #endregion

        #region 日志记录

        /// <summary>
        /// 记录调试信息
        /// </summary>
        public void Debug(string message, object? data = null)
        {
            LogDebugInfo($"{message}" + (data != null ? $" - {data}" : string.Empty), DebugLevel.Debug);
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        public void Info(string message, object? data = null)
        {
            LogDebugInfo($"{message}" + (data != null ? $" - {data}" : string.Empty), DebugLevel.Info);
        }

        /// <summary>
        /// 记录警告
        /// </summary>
        public void Warn(string message, object? data = null)
        {
            LogDebugInfo($"{message}" + (data != null ? $" - {data}" : string.Empty), DebugLevel.Warning);
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        public void Error(string message, Exception? exception = null)
        {
            string fullMessage = message;
            if (exception != null)
            {
                fullMessage += $"\n{exception}";
            }
            LogDebugInfo(fullMessage, DebugLevel.Error);
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 异步读取文件内容
        /// </summary>
        public async Task<string> ReadFileAsync(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return await File.ReadAllTextAsync(path);
                }
                else
                {
                    throw new FileNotFoundException($"文件不存在: {path}");
                }
            }
            catch (Exception ex)
            {
                Error($"读取文件失败: {path}", ex);
                throw;
            }
        }

        /// <summary>
        /// 异步写入文件内容
        /// </summary>
        public async Task WriteFileAsync(string path, string content)
        {
            try
            {
                // 确保目录存在
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(path, content);
            }
            catch (Exception ex)
            {
                Error($"写入文件失败: {path}", ex);
                throw;
            }
        }

        /// <summary>
        /// 异步创建文件
        /// </summary>
        public async Task CreateFileAsync(string path)
        {
            try
            {
                // 确保目录存在
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(path))
                {
                    using (await Task.Run(() => File.Create(path)))
                    { }
                }
            }
            catch (Exception ex)
            {
                Error($"创建文件失败: {path}", ex);
                throw;
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// 异步删除文件
        /// </summary>
        public async Task DeleteFileAsync(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    await Task.Run(() => File.Delete(path));
                }
            }
            catch (Exception ex)
            {
                Error($"删除文件失败: {path}", ex);
                throw;
            }
        }

        /// <summary>
        /// 异步创建目录
        /// </summary>
        public async Task CreateDirectoryAsync(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    await Task.Run(() => Directory.CreateDirectory(path));
                }
            }
            catch (Exception ex)
            {
                Error($"创建目录失败: {path}", ex);
                throw;
            }
        }

        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// 检索文件
        /// </summary>
        public IEnumerable<string> SearchFiles(string directory, string pattern, bool recursive = false)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    return Directory.GetFiles(directory, pattern, searchOption);
                }
                else
                {
                    throw new DirectoryNotFoundException($"目录不存在: {directory}");
                }
            }
            catch (Exception ex)
            {
                Error($"检索文件失败: {directory}", ex);
                throw;
            }
        }

        #endregion

        #region 窗口操作

        /// <summary>
        /// 创建窗口
        /// </summary>
        public Window CreateWindow(string title, UserControl content, bool isModal = false)
        {
            try
            {
                var window = new Window
                {
                    Title = title,
                    Content = content,
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                Info($"创建窗口: {title}");
                return window;
            }
            catch (Exception ex)
            {
                Error($"创建窗口失败: {title}", ex);
                throw;
            }
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        public void ShowWindow(Window window)
        {
            try
            {
                if (window != null && !window.IsLoaded)
                {
                    window.Show();
                    Info($"显示窗口: {window.Title}");
                }
            }
            catch (Exception ex)
            {
                Error($"显示窗口失败: {window?.Title}", ex);
                throw;
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void CloseWindow(Window window)
        {
            try
            {
                if (window != null && window.IsLoaded)
                {
                    window.Close();
                    Info($"关闭窗口: {window.Title}");
                }
            }
            catch (Exception ex)
            {
                Error($"关闭窗口失败: {window?.Title}", ex);
                throw;
            }
        }

        #endregion

        #region 插件操作

        /// <summary>
        /// 获取依赖插件
        /// </summary>
        IDependency? IPluginAPI.GetDependency(string pluginId)
        {
            try
            {
                if (_pluginLoader == null)
                {
                    Warn($"无法获取依赖 {pluginId}: 插件加载器未初始化");
                    return null;
                }
                
                IDependency? dependency = _pluginLoader.GetDependency(pluginId);
                if (dependency != null)
                {
                    Info($"成功获取依赖: {pluginId} ({dependency.Name})");
                }
                else
                {
                    Warn($"未找到依赖: {pluginId}");
                }
                return dependency;
            }
            catch (Exception ex)
            {
                Error($"获取依赖失败: {pluginId}", ex);
                throw;
            }
        }

        /// <summary>
        /// 检查是否存在指定依赖
        /// </summary>
        public bool HasDependency(string pluginId)
        {
            try
            {
                if (_pluginLoader == null)
                {
                    Warn($"无法检查依赖 {pluginId}: 插件加载器未初始化");
                    return false;
                }
                
                bool hasDependency = _pluginLoader.HasDependency(pluginId);
                Info($"检查依赖 {pluginId}: {(hasDependency ? "已安装" : "未安装")}");
                return hasDependency;
            }
            catch (Exception ex)
            {
                Error($"检查依赖失败: {pluginId}", ex);
                throw;
            }
        }

        #endregion
        
        #region 配置操作
        
        /// <summary>
        /// 获取配置目录路径
        /// </summary>
        public string ConfigDirectory
        {
            get
            {
                // 获取插件目录
                string pluginsDirectory = Path.GetDirectoryName(PluginPath) ?? string.Empty;
                // 配置目录为 Plugins/[PluginId]
                string configDir = Path.Combine(pluginsDirectory, PluginId);
                // 确保目录存在
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                    Info($"创建了配置目录: {configDir}");
                }
                return configDir;
            }
        }
        
        /// <summary>
        /// 获取插件配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="defaultConfig">默认配置</param>
        /// <returns>配置对象</returns>
        public T GetConfig<T>(T defaultConfig) where T : class
        {
            try
            {
                if (!FileExists(ConfigFilePath))
                {
                    Info($"配置文件不存在，使用默认配置: {ConfigFilePath}");
                    return defaultConfig;
                }
                
                string json = File.ReadAllText(ConfigFilePath);
                T config = System.Text.Json.JsonSerializer.Deserialize<T>(json) ?? defaultConfig;
                Info($"成功读取配置: {ConfigFilePath}");
                return config;
            }
            catch (Exception ex)
            {
                Error($"读取配置失败: {ConfigFilePath}", ex);
                return defaultConfig;
            }
        }
        
        /// <summary>
        /// 保存插件配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="config">配置对象</param>
        /// <returns>任务</returns>
        public async Task SaveConfigAsync<T>(T config) where T : class
        {
            try
            {
                // 确保配置目录存在
                if (!Directory.Exists(ConfigDirectory))
                {
                    Directory.CreateDirectory(ConfigDirectory);
                }
                
                string json = System.Text.Json.JsonSerializer.Serialize(config, _jsonSerializerOptions);
                
                await File.WriteAllTextAsync(ConfigFilePath, json);
                Info($"成功保存配置: {ConfigFilePath}");
                
                // 发布配置变更事件
                _eventBus.Publish(new ConfigChangedEvent(PluginId, "ConfigSaved"));
            }
            catch (Exception ex)
            {
                Error($"保存配置失败: {ConfigFilePath}", ex);
                
                // 发布配置变更失败事件
                _eventBus.Publish(new PluginErrorEvent(PluginId, $"保存配置失败: {ex.Message}", ex));
                throw;
            }
        }
        
        /// <summary>
        /// 检查是否存在配置文件
        /// </summary>
        /// <returns>是否存在配置文件</returns>
        public bool HasConfig()
        {
            return FileExists(ConfigFilePath);
        }
        
        #endregion
        
        #region 数据共享
        
        /// <summary>
        /// 存储共享数据
        /// </summary>
        /// <param name="key">数据键名</param>
        /// <param name="data">要存储的数据</param>
        public void ShareData(string key, object data)
        {
            try
            {
                _dataShareService.SetData(key, data);
                Info($"共享数据: {key}");
            }
            catch (Exception ex)
            {
                Error($"共享数据失败: {key}", ex);
            }
        }
        
        /// <summary>
        /// 获取共享数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键名</param>
        /// <returns>转换后的数据，如果转换失败则返回默认值</returns>
        public T? GetSharedData<T>(string key)
        {
            try
            {
                return _dataShareService.GetData<T>(key);
            }
            catch (Exception ex)
            {
                Error($"获取共享数据失败: {key}", ex);
                return default;
            }
        }
        
        /// <summary>
        /// 检查是否存在指定键的数据
        /// </summary>
        /// <param name="key">数据键名</param>
        /// <returns>是否存在数据</returns>
        public bool HasSharedData(string key)
        {
            try
            {
                return _dataShareService.ContainsData(key);
            }
            catch (Exception ex)
            {
                Error($"检查共享数据失败: {key}", ex);
                return false;
            }
        }
        
        /// <summary>
        /// 删除指定键的共享数据
        /// </summary>
        /// <param name="key">数据键名</param>
        public void RemoveSharedData(string key)
        {
            try
            {
                _dataShareService.RemoveData(key);
                Info($"删除共享数据: {key}");
            }
            catch (Exception ex)
            {
                Error($"删除共享数据失败: {key}", ex);
            }
        }
        
        #endregion
        
        #region 事件总线
        
        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">要发布的事件</param>
        public void PublishEvent<TEvent>(TEvent @event) where TEvent : class
        {
            try
            {
                _eventBus.Publish(@event);
                Info($"发布事件: {typeof(TEvent).Name}");
            }
            catch (Exception ex)
            {
                Error($"发布事件失败: {typeof(TEvent).Name}", ex);
            }
        }
        
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件处理程序</param>
        public void SubscribeEvent<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            try
            {
                _eventBus.Subscribe(handler);
                Info($"订阅事件: {typeof(TEvent).Name}");
            }
            catch (Exception ex)
            {
                Error($"订阅事件失败: {typeof(TEvent).Name}", ex);
            }
        }
        
        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">要取消的事件处理程序</param>
        public void UnsubscribeEvent<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            try
            {
                _eventBus.Unsubscribe(handler);
                Info($"取消订阅事件: {typeof(TEvent).Name}");
            }
            catch (Exception ex)
            {
                Error($"取消订阅事件失败: {typeof(TEvent).Name}", ex);
            }
        }
        
        #endregion
        
        #region 性能监控
        
        /// <summary>
        /// 开始计时操作
        /// </summary>
        /// <param name="operationName">操作名称</param>
        public void StartOperationTimer(string operationName)
        {
            try
            {
                _performanceMonitor.StartOperationTimer($"{PluginId}:{operationName}");
            }
            catch (Exception ex)
            {
                Error($"开始性能计时失败: {operationName}", ex);
            }
        }
        
        /// <summary>
        /// 停止计时并记录操作
        /// </summary>
        /// <param name="operationName">操作名称</param>
        public void StopOperationTimer(string operationName)
        {
            try
            {
                _performanceMonitor.StopOperationTimer($"{PluginId}:{operationName}");
            }
            catch (Exception ex)
            {
                Error($"停止性能计时失败: {operationName}", ex);
            }
        }
        
        #endregion

        #region 内部方法

        /// <summary>
        /// 记录调试信息到事件
        /// </summary>
        private void LogDebugInfo(string message, DebugLevel level = DebugLevel.Info)
        {
            DebugInfoGenerated?.Invoke(this, new DebugInfoEventArgs { Message = message, Level = level, Timestamp = DateTime.Now });
        }

        #endregion
        
        #region 主题相关
        
        /// <summary>
        /// 主题变更事件处理方法
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void OnThemeChanged(object? sender, ToolboxTheme e)
        {
            ThemeChanged?.Invoke(this, e);
        }
        
        /// <summary>
        /// 主题变更事件
        /// </summary>
        public event EventHandler<ToolboxTheme>? ThemeChanged;
        
        /// <summary>
        /// 获取当前主题（包括预览主题）
        /// </summary>
        public ToolboxTheme CurrentTheme
        {
            get => _themeService?.CurrentTheme ?? _settingsService.GetSettings().Theme;
        }
        
        /// <summary>
        /// 获取已保存的主题（从设置文件中获取）
        /// </summary>
        public ToolboxTheme SavedTheme
        {
            get => _settingsService.GetSettings().Theme;
        }
        
        /// <summary>
        /// 获取当前主题的主背景色
        /// </summary>
        public Brush CurrentBackgroundBrush
        {
            get => _themeService?.MainBackgroundBrush ?? new SolidColorBrush(Color.FromRgb(245, 245, 245));
        }
        
        /// <summary>
        /// 获取当前主题的主前景色
        /// </summary>
        public Brush CurrentForegroundBrush
        {
            get => _themeService?.MainForegroundBrush ?? Brushes.Black;
        }
        
        /// <summary>
        /// 获取当前主题的插件面板背景色
        /// </summary>
        public Brush PluginPanelBackgroundBrush
        {
            get => _themeService?.PluginPanelBackgroundBrush ?? new SolidColorBrush(Color.FromRgb(235, 235, 235));
        }
        
        /// <summary>
        /// 获取当前主题的插件工作区背景色
        /// </summary>
        public Brush PluginWorkspaceBackgroundBrush
        {
            get => _themeService?.PluginWorkspaceBackgroundBrush ?? new SolidColorBrush(Color.FromRgb(250, 250, 250));
        }
        
        /// <summary>
        /// 获取当前主题的边框颜色
        /// </summary>
        public Brush BorderBrush
        {
            get => _themeService?.BorderBrush ?? new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }
        
        /// <summary>
        /// 获取当前主题的控件背景色
        /// </summary>
        public Brush ControlBackgroundColor
        {
            get => _themeService?.MainBackgroundBrush ?? new SolidColorBrush(Color.FromRgb(245, 245, 245));
        }
        
        /// <summary>
        /// 获取当前主题的控件前景色
        /// </summary>
        public Brush ControlForegroundColor
        {
            get => _themeService?.MainForegroundBrush ?? Brushes.Black;
        }
        
        /// <summary>
        /// 获取当前主题的强调色
        /// </summary>
        public Brush AccentColor
        {
            get
            {
                // 使用主背景色的对比色作为强调色
                var bg = _themeService.MainBackgroundBrush;
                if (bg is SolidColorBrush solidBrush)
                {
                    var color = solidBrush.Color;
                    // 计算亮度
                    double brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
                    // 如果背景较暗，返回蓝色；否则返回深蓝色
                    return brightness < 0.5 ? Brushes.SteelBlue : Brushes.DarkSlateBlue;
                }
                return Brushes.SteelBlue;
            }
        }
        
        /// <summary>
        /// 指示插件是否同步工具箱主题
        /// </summary>
        public bool SyncToolboxTheme
        {
            get => _syncToolboxTheme;
            set => _syncToolboxTheme = value;
        }
        
        /// <summary>
        /// 将当前主题应用到指定的FrameworkElement及其所有子元素
        /// </summary>
        /// <param name="element">要应用主题的元素</param>
        public void ApplyThemeToElement(FrameworkElement element)
        {
            try
            {
                if (element == null)
                    return;
                
                // 直接从设置中获取最新的主题颜色，不依赖于静态实例
                // 对于插件，使用插件工作区背景色
                var bg = PluginWorkspaceBackgroundBrush;
                var fg = CurrentForegroundBrush;
                var border = BorderBrush;
                var controlBg = CurrentBackgroundBrush;
                
                // 首先将主题应用到根元素
                // 对于所有FrameworkElement，直接调用递归方法，确保所有类型都被处理
                ApplyThemeRecursively(element, bg, fg, border, controlBg);
                
                // 确保根元素本身的属性也被正确设置
                if (element is Panel panel)
                {
                    panel.Background = bg;
                }
                else if (element is UserControl userControl)
                {
                    userControl.Background = bg;
                    userControl.Foreground = fg;
                }
                else if (element is TabControl tabControl)
                {
                    // TabControl作为容器控件使用工作区背景色
                    tabControl.Background = bg;
                    tabControl.Foreground = fg;
                    tabControl.BorderBrush = border;
                }
                else if (element is Control control)
                {
                    // 非容器控件使用控件背景色
                    control.Background = controlBg;
                    control.Foreground = fg;
                    control.BorderBrush = border;
                }
                else if (element is Border borderElement)
                {
                    borderElement.Background = bg;
                    borderElement.BorderBrush = border;
                }
                else if (element is TextBlock textBlock)
                {
                    textBlock.Foreground = fg;
                }
                else if (element is ContentControl contentControl)
                {
                    contentControl.Foreground = fg;
                }
                
                // 强制刷新UI，确保颜色立即生效
                element.InvalidateVisual();
                element.UpdateLayout();
            }
            catch (Exception ex)
            {
                Error("应用主题到元素失败", ex);
            }
        }
        
        /// <summary>
        /// 递归将主题应用到元素及其子元素
        /// </summary>
        /// <param name="element">要应用主题的元素</param>
        /// <param name="bg">背景色</param>
        /// <param name="fg">前景色</param>
        /// <param name="border">边框色</param>
        /// <param name="controlBg">控件背景色</param>
        private void ApplyThemeRecursively(DependencyObject element, Brush bg, Brush fg, Brush border, Brush controlBg)
        {
            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                
                // 处理TabControl类型（特殊容器控件）
                if (child is TabControl tabControl)
                {
                    tabControl.Background = bg;
                    tabControl.Foreground = fg;
                    tabControl.BorderBrush = border;
                }
                // 处理Control类型
                else if (child is Control control)
                {
                    // 非容器控件使用控件背景色
                    control.Background = controlBg;
                    control.Foreground = fg;
                    control.BorderBrush = border;
                }
                // 处理Border类型
                else if (child is Border borderElement)
                {
                    borderElement.Background = bg;
                    borderElement.BorderBrush = border;
                }
                // 处理TextBlock类型
                else if (child is TextBlock textBlock)
                {
                    textBlock.Foreground = fg;
                }
                // 处理ContentControl类型
                else if (child is ContentControl contentControl)
                {
                    contentControl.Foreground = fg;
                }
                // 处理ContentPresenter类型
                else if (child is ContentPresenter contentPresenter)
                {
                    // 设置ContentPresenter的前景色
                    TextElement.SetForeground(contentPresenter, fg);
                }
                // 处理WebBrowser类型
                else if (child is System.Windows.Controls.WebBrowser)
                {
                    // WebBrowser控件不需要设置背景色和前景色
                }
                // 处理Image类型
                else if (child is System.Windows.Controls.Image)
                {
                    // Image控件不需要设置背景色和前景色
                }
                // 处理MediaElement类型
                else if (child is System.Windows.Controls.MediaElement)
                {
                    // MediaElement控件不需要设置背景色和前景色
                }
                // 处理Shape类型
                else if (child is System.Windows.Shapes.Shape shape)
                {
                    shape.Fill = bg;
                    shape.Stroke = border;
                    shape.StrokeThickness = 1;
                }
                // 处理Panel类型（包括Grid、StackPanel等容器控件）
                else if (child is System.Windows.Controls.Panel panel)
                {
                    // 面板控件使用工作区背景色
                    panel.Background = bg;
                }
                
                // 递归处理子元素
                ApplyThemeRecursively(child, bg, fg, border, controlBg);
            }
        }
        
        /// <summary>
        /// 为指定的Brush类型获取当前主题的颜色
        /// </summary>
        /// <param name="brushType">Brush类型名称</param>
        /// <returns>对应的Brush对象</returns>
        public Brush GetThemeBrush(string brushType)
        {
            try
            {
                // 直接从当前主题属性返回对应Brush，不依赖于静态实例
                switch (brushType.ToLower())
                {
                    case "background":
                    case "mainbackground":
                        return CurrentBackgroundBrush;
                    case "foreground":
                    case "mainforeground":
                        return CurrentForegroundBrush;
                    case "pluginpanelbackground":
                        return PluginPanelBackgroundBrush;
                    case "pluginworkspacebackground":
                        return PluginWorkspaceBackgroundBrush;
                    case "border":
                    case "borderbrush":
                        return BorderBrush;
                    case "controlbackground":
                    case "controlbackgroundcolor":
                        return ControlBackgroundColor;
                    case "controlforeground":
                    case "controlforegroundcolor":
                        return ControlForegroundColor;
                    case "accent":
                    case "accentcolor":
                        return AccentColor;
                    default:
                        Warn($"未知的Brush类型: {brushType}");
                        return CurrentBackgroundBrush;
                }
            }
            catch (Exception ex)
            {
                Error($"获取主题Brush失败: {brushType}", ex);
                return CurrentBackgroundBrush;
            }
        }
        

        
        #endregion

        /// <summary>
        /// 调试信息事件，当插件输出调试信息时触发
        /// </summary>
        public event EventHandler<DebugInfoEventArgs>? DebugInfoGenerated;
    }

    /// <summary>
    /// 调试信息事件参数
    /// </summary>
    public class DebugInfoEventArgs : EventArgs
    {
        /// <summary>
        /// 调试信息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 调试级别
        /// </summary>
        public DebugLevel Level { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}