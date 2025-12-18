using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using WPFPluginToolbox.Core;

namespace WPFPluginToolbox.PluginSystem
{
    /// <summary>
    /// 插件加载器类，基于MEF实现插件加载
    /// </summary>
    public class PluginLoader
    {
        // Windows API declarations for file operations
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern bool DeleteFile(string lpFileName);
        
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern int GetLastError();
        
        private AssemblyLoadContext? _pluginLoadContext;
        
        // 已加载的插件
        private readonly Dictionary<string, IPlugin> _loadedPlugins = [];
        // 已卸载的插件信息
        private readonly Dictionary<string, PluginMetadata> _unloadedPlugins = [];
        private readonly Dictionary<string, IDependency> _loadedDependencies = [];
        private readonly Dictionary<string, PluginAPI> _pluginApis = [];
        // 记录插件ID对应的.dll文件路径
        private readonly Dictionary<string, string> _pluginFilePaths = [];
        private FileSystemWatcher? _directoryWatcher;
        private readonly WPFPluginToolbox.Services.LogService? _logService;
        
        /// <summary>
        /// 插件目录路径
        /// </summary>
        public string PluginsDirectory { get; set; } = string.Empty;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pluginsDirectory">插件目录路径</param>
        /// <param name="logService">日志服务实例</param>
        public PluginLoader(string pluginsDirectory, WPFPluginToolbox.Services.LogService? logService = null)
        {
            PluginsDirectory = pluginsDirectory;
            _logService = logService;
            
            // 创建目录如果不存在
            if (!Directory.Exists(PluginsDirectory))
            {
                Directory.CreateDirectory(PluginsDirectory);
                LogInfo($"创建了插件目录: {PluginsDirectory}");
    
            }
            
            // 清理旧的临时文件
            CleanupTempFiles();
            
            // 初始化插件加载上下文
            InitializePluginLoadContext();
            
            // 初始化目录监听
            InitializeDirectoryWatcher();
        }
        
        /// <summary>
        /// 清理插件目录中的临时文件和临时目录
        /// </summary>
        private void CleanupTempFiles()
        {
            try
            {
                LogInfo($"=== PluginLoader: Cleaning up temporary files in {PluginsDirectory}");
                
                // 1. 清理插件目录中的.tmp文件
                var tmpFiles = Directory.GetFiles(PluginsDirectory, "*.tmp");
                foreach (var tmpFile in tmpFiles)
                {
                    try
                    {
                        File.Delete(tmpFile);
                        LogInfo($"=== PluginLoader: Cleaned up temp file: {tmpFile}");
                    }
                    catch (Exception ex)
                    {
                        LogWarning($"=== PluginLoader: Failed to cleanup temp file {tmpFile}: {ex.Message}");
                    }
                }
                
                // 2. 清理临时插件目录
                string pluginTempRoot = Path.Combine(Path.GetTempPath(), "PluginLoader");
                if (Directory.Exists(pluginTempRoot))
                {
                    LogInfo($"=== PluginLoader: Cleaning up temp plugin directories");
                    
                    try
                    {
                        // 删除临时插件目录及其所有内容
                        Directory.Delete(pluginTempRoot, true);
                        LogInfo($"=== PluginLoader: Cleaned up temp plugin directory: {pluginTempRoot}");
                    }
                    catch (Exception ex)
                    {
                        LogWarning($"=== PluginLoader: Failed to cleanup temp plugin directory {pluginTempRoot}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"=== PluginLoader: Error cleaning up temp files: {ex.Message}");
            }
        }
        

        
        /// <summary>
        /// 初始化插件加载上下文
        /// </summary>
        private void InitializePluginLoadContext()
        {
            // 创建可卸载的AssemblyLoadContext
            _pluginLoadContext = new AssemblyLoadContext("PluginContext", true);
            _pluginLoadContext.Unloading += PluginLoadContext_Unloading;
            
            LogInfo($"=== PluginLoader: Created AssemblyLoadContext 'PluginContext'");
            
            // 创建目录编目，它将使用我们的自定义AssemblyLoadContext来加载插件
            // 注意：DirectoryCatalog本身不支持自定义AssemblyLoadContext，所以我们需要手动加载程序集
        }
        
        /// <summary>
        /// 插件加载上下文卸载事件
        /// </summary>
        private void PluginLoadContext_Unloading(AssemblyLoadContext obj)
        {
            LogInfo($"=== PluginLoader: AssemblyLoadContext is unloading");
            
            // 确保所有资源被释放
            LogInfo($"=== PluginLoader: AssemblyLoadContext unloaded successfully");
        }
        
        /// <summary>
        /// 日志记录辅助方法
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogInfo(string message)
        {
            _logService?.Info(message);
            Console.WriteLine(message);
        }
        
        /// <summary>
        /// 日志记录辅助方法
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogError(string message)
        {
            _logService?.Error(message);
            Console.WriteLine(message);
        }
        
        /// <summary>
        /// 日志记录辅助方法
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogDebug(string message)
        {
            _logService?.Info(message);
            Console.WriteLine(message);
        }
        
        /// <summary>
        /// 日志记录辅助方法
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogWarning(string message)
        {
            _logService?.Info(message);
            Console.WriteLine($"WARNING: {message}");
        }
        
        /// <summary>
        /// 初始化目录监听
        /// </summary>
        private void InitializeDirectoryWatcher()
        {
            _directoryWatcher = new FileSystemWatcher(PluginsDirectory);
            _directoryWatcher.Filter = "*.dll";
            _directoryWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            _directoryWatcher.Created += OnPluginFileChanged;
            _directoryWatcher.Deleted += OnPluginFileChanged;
            _directoryWatcher.Changed += OnPluginFileChanged;
            _directoryWatcher.EnableRaisingEvents = true;
        }
        
        /// <summary>
        /// 插件文件变化事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void OnPluginFileChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"检测到插件文件变化: {e.ChangeType} - {e.Name}");
            // 延迟重新加载，确保文件操作完成
            System.Threading.Thread.Sleep(500);
            ReloadPlugins();
        }
        
        /// <summary>
        /// 加载所有插件
        /// </summary>
        public void LoadAllPlugins()
        {
            // 先加载所有依赖
            LoadDependencies();
            
            // 再加载所有插件
            LoadPlugins();
        }
        
        
        /// <summary>
        /// 加载所有依赖
        /// </summary>
        private void LoadDependencies()
        {
            try
            {
                // 手动加载所有依赖程序集
                var dllFiles = Directory.GetFiles(PluginsDirectory, "*.dll");
                
                foreach (var dllFile in dllFiles)
                {
                    try
                    {
                        // 使用FileStream加载程序集，设置FileShare.Delete允许文件被删除
                        using (var fileStream = new FileStream(dllFile, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                        {
                            if (_pluginLoadContext != null)
                            {
                                var assembly = _pluginLoadContext.LoadFromStream(fileStream);
                                
                                // 查找IDependency类型
                                var dependencyTypes = assembly.GetTypes()
                                    .Where(type => typeof(IDependency).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
                                
                                foreach (var dependencyType in dependencyTypes)
                                {
                                    // 创建依赖实例
                                    if (Activator.CreateInstance(dependencyType) is IDependency dependency && !_loadedDependencies.ContainsKey(dependency.Id))
                                    {
                                        dependency.Initialize();
                                        _loadedDependencies[dependency.Id] = dependency;
                                        Console.WriteLine($"Loaded dependency: {dependency.Name} ({dependency.Version})");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading dependency from {dllFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dependencies: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 加载所有插件
        /// </summary>
        private void LoadPlugins()
        {
            try
            {
                // 手动加载所有插件程序集
                var dllFiles = Directory.GetFiles(PluginsDirectory, "*.dll");
                
                foreach (var dllFile in dllFiles)
                {
                    try
                    {
                        // 使用FileStream加载程序集，设置FileShare.Delete允许文件被删除
                        using (var fileStream = new FileStream(dllFile, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                        {
                            if (_pluginLoadContext != null)
                            {
                                var assembly = _pluginLoadContext.LoadFromStream(fileStream);
                                
                                // 查找IPlugin类型
                                var pluginTypes = assembly.GetTypes()
                                    .Where(type => typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
                                
                                foreach (var pluginType in pluginTypes)
                                {
                                    // 创建插件实例
                                    if (Activator.CreateInstance(pluginType) is IPlugin plugin && !_loadedPlugins.ContainsKey(plugin.Id))
                                    {
                                        // 检查插件依赖
                                        if (CheckPluginDependencies(plugin))
                                        {
                                            // 创建插件API实例，传入插件加载器引用
                                            var pluginApi = new PluginAPI(this);
                                            
                                            // 设置插件API的基本信息
                                            pluginApi.PluginId = plugin.Id;
                                            pluginApi.PluginName = plugin.Name;
                                            pluginApi.PluginPath = dllFile;
                                            
                                            _pluginApis[plugin.Id] = pluginApi;
                                            
                                            // 初始化并激活插件
                                            plugin.Initialize(pluginApi);
                                            plugin.Activate();
                                            
                                            _loadedPlugins[plugin.Id] = plugin;
                                            
                                            // 记录插件对应的原始.dll文件路径
                                            _pluginFilePaths[plugin.Id] = dllFile;
                                            
                                            Console.WriteLine($"Loaded plugin: {plugin.Name} ({plugin.Version}) from {dllFile}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Failed to load plugin {plugin.Name}: Missing required dependencies");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading plugin from {dllFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading plugins: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 检查插件依赖
        /// </summary>
        /// <param name="plugin">插件实例</param>
        /// <returns>是否满足所有必要依赖</returns>
        private static bool CheckPluginDependencies(IPlugin plugin)
        {
            // 当前实现：总是返回true，让插件在Initialize方法中自行检测依赖
            // 这样可以保持兼容性，同时让插件有更大的灵活性来处理依赖
            return true;
        }
        
        /// <summary>
        /// 卸载所有插件
        /// </summary>
        public void UnloadAllPlugins()
        {
            // 先卸载所有插件
            foreach (var plugin in _loadedPlugins.Values)
            {
                plugin.Deactivate();
                plugin.Dispose();
                Console.WriteLine($"Unloaded plugin: {plugin.Name}");
            }
            _loadedPlugins.Clear();
            _pluginApis.Clear();
            
            // 再卸载所有依赖
            foreach (var dependency in _loadedDependencies.Values)
            {
                dependency.Dispose();
                Console.WriteLine($"Unloaded dependency: {dependency.Name}");
            }
            _loadedDependencies.Clear();
        }
        
        /// <summary>
        /// 重新加载所有插件
        /// </summary>
        public void ReloadPlugins()
        {
            // 卸载所有插件和释放资源
            UnloadAllPlugins();
            
            // 卸载并重新创建插件加载上下文，这将释放所有文件锁
            if (_pluginLoadContext != null)
            {
                LogInfo($"=== PluginLoader: Unloading AssemblyLoadContext");
                _pluginLoadContext.Unload();
                _pluginLoadContext = null;
            }
            
            // 等待垃圾回收释放资源
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            // 重新初始化插件加载上下文
            InitializePluginLoadContext();
            
            // 重新加载所有插件
            LoadAllPlugins();
        }
        
        /// <summary>
        /// 获取所有已加载的插件
        /// </summary>
        /// <returns>已加载插件列表</returns>
        public List<IPlugin> GetLoadedPlugins()
        {
            return _loadedPlugins.Values.ToList();
        }
        
        /// <summary>
        /// 获取所有已卸载的插件信息
        /// </summary>
        /// <returns>已卸载插件信息列表</returns>
        public List<PluginMetadata> GetUnloadedPlugins()
        {
            return _unloadedPlugins.Values.ToList();
        }
        
        /// <summary>
        /// 获取所有插件信息（包括已加载和已卸载）
        /// </summary>
        /// <returns>所有插件信息列表</returns>
        public List<PluginMetadata> GetAllPlugins()
        {
            var allPlugins = new List<PluginMetadata>();
            
            // 先添加已加载的插件，转换为PluginMetadata
            foreach (var plugin in _loadedPlugins.Values)
            {
                allPlugins.Add(new PluginMetadata
                {
                    Id = plugin.Id,
                    Name = plugin.Name,
                    Description = plugin.Description,
                    Version = plugin.Version,
                    Type = plugin.Type,
                    PluginPath = _pluginFilePaths[plugin.Id],
                    IsLoaded = true,
                    IsActive = true,
                    HasError = false
                });
            }
            
            // 再添加已卸载的插件信息，避免重复
            foreach (var pluginMetadata in _unloadedPlugins.Values)
            {
                // 检查是否已经存在
                if (!allPlugins.Any(p => p.Id == pluginMetadata.Id))
                {
                    allPlugins.Add(pluginMetadata);
                }
            }
            
            return allPlugins;
        }
        
        /// <summary>
        /// 获取所有已加载的依赖
        /// </summary>
        /// <returns>已加载依赖列表</returns>
        public List<IDependency> GetLoadedDependencies()
        {
            return _loadedDependencies.Values.ToList();
        }
        
        /// <summary>
        /// 获取指定ID的依赖
        /// </summary>
        /// <param name="dependencyId">依赖ID</param>
        /// <returns>依赖实例，不存在则返回null</returns>
        public IDependency? GetDependency(string dependencyId)
        {
            _loadedDependencies.TryGetValue(dependencyId, out var dependency);
            return dependency;
        }
        
        /// <summary>
        /// 检查指定ID的依赖是否已加载
        /// </summary>
        /// <param name="dependencyId">依赖ID</param>
        /// <returns>是否已加载</returns>
        public bool HasDependency(string dependencyId)
        {
            return _loadedDependencies.ContainsKey(dependencyId);
        }
        
        /// <summary>
        /// 卸载指定插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>是否成功卸载</returns>
        public bool UnloadPlugin(string pluginId)
        {
            if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
            {
                try
                {
                    _pluginFilePaths.TryGetValue(pluginId, out var pluginFilePath);
                    
                    // 停用并释放插件
                    plugin.Deactivate();
                    plugin.Dispose();
                    
                    Console.WriteLine($"Unloaded plugin: {plugin.Name}");
                    
                    // 创建插件元数据并添加到已卸载列表
                    var pluginMetadata = new PluginMetadata
                    {
                        Id = plugin.Id,
                        Name = plugin.Name,
                        Description = plugin.Description,
                        Version = plugin.Version,
                        Type = plugin.Type,
                        PluginPath = pluginFilePath ?? string.Empty,
                        IsLoaded = false,
                        IsActive = false,
                        HasError = false
                    };
                    
                    // 添加到已卸载插件列表
                    _unloadedPlugins[pluginId] = pluginMetadata;
                    
                    // 从已加载插件字典中移除
                    _loadedPlugins.Remove(pluginId);
                    _pluginApis.Remove(pluginId);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error unloading plugin {pluginId}: {ex.Message}");
                    return false;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 重载指定插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>是否成功重载</returns>
        public bool ReloadPlugin(string pluginId)
        {
            try
            {
                // 检查插件是否已加载
                bool wasLoaded = _loadedPlugins.ContainsKey(pluginId);
                
                // 如果已加载，先卸载插件
                if (wasLoaded)
                {
                    if (!UnloadPlugin(pluginId))
                    {
                        Console.WriteLine($"Failed to unload plugin {pluginId} before reloading");
                        return false;
                    }
                }
                
                // 检查插件是否在已卸载列表中
                bool wasUnloaded = _unloadedPlugins.ContainsKey(pluginId);
                if (!wasUnloaded)
                {
                    Console.WriteLine($"Plugin {pluginId} not found in loaded or unloaded plugins");
                    return false;
                }
                
                // 重新加载所有插件
                // 注意：由于MEF的限制，无法只重新加载单个插件
                // 我们需要重新创建整个MEF容器并加载所有插件
                ReloadPlugins();
                
                // 检查插件是否成功重新加载
                bool success = _loadedPlugins.ContainsKey(pluginId);
                if (success)
                {
                    // 从已卸载列表中移除
                    _unloadedPlugins.Remove(pluginId);
                    Console.WriteLine($"Successfully reloaded plugin {pluginId}");
                }
                else
                {
                    Console.WriteLine($"Failed to reload plugin {pluginId}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reloading plugin {pluginId}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 导出指定插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <param name="exportPath">导出路径</param>
        /// <returns>是否成功导出</returns>
        public bool ExportPlugin(string pluginId, string exportPath)
        {
            try
            {
                string? sourcePath = null;
                string? pluginName = null;
                
                // 检查是已加载还是已卸载的插件
                if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
                {
                    _pluginFilePaths.TryGetValue(pluginId, out sourcePath);
                    pluginName = plugin.Name;
                }
                else if (_unloadedPlugins.TryGetValue(pluginId, out var unloadedPlugin))
                {
                    sourcePath = unloadedPlugin.PluginPath ?? string.Empty;
                    pluginName = unloadedPlugin.Name ?? string.Empty;
                }
                else
                {
                    Console.WriteLine($"Plugin {pluginId} not found");
                    return false;
                }
                
                if (!string.IsNullOrEmpty(sourcePath))
                {
                    Console.WriteLine($"Exporting plugin {pluginName} from {sourcePath} to {exportPath}");
                    
                    // 复制文件
                    File.Copy(sourcePath, exportPath, overwrite: true);
                    
                    Console.WriteLine($"Successfully exported plugin {pluginName} to {exportPath}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error exporting plugin {pluginId}: Source path is null or empty");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting plugin {pluginId}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 删除指定插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>是否成功删除</returns>
        public bool DeletePlugin(string pluginId)
        {
            try
            {
                string? pluginFilePath = null;
                string? pluginName = null;
                
                // 1. 获取插件信息和文件路径
                if (_loadedPlugins.TryGetValue(pluginId, out var plugin))
                {
                    pluginName = plugin.Name;
                    if (_pluginFilePaths.TryGetValue(pluginId, out var filePath))
                    {
                        pluginFilePath = filePath;
                    }
                    
                    // 停用并释放插件资源
                    plugin.Deactivate();
                    plugin.Dispose();
                    
                    // 从已加载插件列表中移除
                    _loadedPlugins.Remove(pluginId);
                    _pluginApis.Remove(pluginId);
                    _pluginFilePaths.Remove(pluginId);
                    
                    LogInfo($"=== DeletePlugin: Unloaded and disposed plugin: {pluginName ?? string.Empty}");
                }
                else if (_unloadedPlugins.TryGetValue(pluginId, out var unloadedPlugin))
                {
                    pluginFilePath = unloadedPlugin.PluginPath ?? string.Empty;
                    pluginName = unloadedPlugin.Name ?? string.Empty;
                    
                    // 从已卸载插件列表中移除
                    _unloadedPlugins.Remove(pluginId);
                    
                    LogInfo($"=== DeletePlugin: Removed plugin from unloaded plugins list: {pluginName}");
                }
                else
                {
                    LogError($"=== DeletePlugin: Plugin {pluginId} not found");
                    return false;
                }
                
                LogInfo($"=== DeletePlugin: Attempting to delete plugin: {pluginName ?? string.Empty}");
                LogInfo($"=== DeletePlugin: Plugin file path: {pluginFilePath ?? string.Empty}");
                
                // 2. 检查文件是否存在
                if (!File.Exists(pluginFilePath))
                {
                    LogWarning($"=== DeletePlugin: Plugin file not found, may have already been deleted: {pluginFilePath}");
                    return true; // 文件不存在，视为删除成功
                }
                
                // 3. 尝试直接删除文件，由于我们使用了FileShare.Delete，应该可以直接删除
                bool deletionSuccess = false;
                
                try
                {
                    // 确保文件属性为Normal
                    var fileInfo = new FileInfo(pluginFilePath);
                    if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        fileInfo.Attributes = FileAttributes.Normal;
                        LogInfo($"=== DeletePlugin: Removed ReadOnly attribute from file");
                    }
                    
                    // 直接删除文件
                    File.Delete(pluginFilePath);
                    
                    // 验证删除是否成功
                    if (!File.Exists(pluginFilePath))
                    {
                        LogInfo($"=== DeletePlugin: Successfully deleted plugin file using direct File.Delete");
                        deletionSuccess = true;
                    }
                    else
                    {
                        LogWarning($"=== DeletePlugin: Direct deletion failed, trying fallback method");
                        
                        // 4. 如果直接删除失败，使用Windows API强制删除
                        bool win32Result = DeleteFile(pluginFilePath);
                        if (win32Result || !File.Exists(pluginFilePath))
                        {
                            LogInfo($"=== DeletePlugin: Successfully deleted plugin file using Windows API");
                            deletionSuccess = true;
                        }
                        else
                        {
                            int errorCode = GetLastError();
                            LogError($"=== DeletePlugin: Failed to delete plugin file with Windows API. Error code: {errorCode}");
                            LogError($"=== DeletePlugin: Error message: {new System.ComponentModel.Win32Exception(errorCode).Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError($"=== DeletePlugin: Error during deletion: {ex.Message}");
                }
                
                if (deletionSuccess)
                {
                    LogInfo($"=== DeletePlugin: Successfully deleted plugin: {pluginName}");
                    return true;
                }
                else
                {
                    LogError($"=== DeletePlugin: Failed to delete plugin file after all attempts: {pluginFilePath}");
                    LogError($"=== DeletePlugin: Please try manually deleting the file: {pluginFilePath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"=== DeletePlugin: Unexpected error in DeletePlugin method: {ex.Message}");
                return false;
            }
        }
    }
}