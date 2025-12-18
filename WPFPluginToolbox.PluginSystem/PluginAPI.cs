using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPFPluginToolbox.Core;

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
        private readonly PluginLoader? _pluginLoader;
        
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

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public PluginAPI()
        {
            // 初始化默认值
            PluginId = string.Empty;
            PluginName = string.Empty;
            PluginPath = string.Empty;
            _pluginLoader = null;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pluginLoader">插件加载器实例</param>
        public PluginAPI(PluginLoader pluginLoader) : this()
        {
            _pluginLoader = pluginLoader;
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
            }
            catch (Exception ex)
            {
                Error($"保存配置失败: {ConfigFilePath}", ex);
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

        #region 内部方法

        /// <summary>
        /// 记录调试信息到事件
        /// </summary>
        private void LogDebugInfo(string message, DebugLevel level = DebugLevel.Info)
        {
            DebugInfoGenerated?.Invoke(this, new DebugInfoEventArgs { Message = message, Level = level, Timestamp = DateTime.Now });
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