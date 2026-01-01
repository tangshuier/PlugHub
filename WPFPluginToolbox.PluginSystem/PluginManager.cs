using System;
using WPFPluginToolbox.Core;

namespace WPFPluginToolbox.PluginSystem
{
    /// <summary>
    /// 插件管理器类，负责管理整个插件系统
    /// </summary>
    public class PluginManager
    {
        private readonly PluginLoader _pluginLoader;
        private static PluginManager? _instance;
        
        /// <summary>
        /// 插件管理器单例实例
        /// </summary>
        public static PluginManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("PluginManager not initialized. Call Initialize first.");
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 插件加载事件
        /// </summary>
        public event EventHandler<PluginEventArgs>? PluginLoaded;
        
        /// <summary>
        /// 插件卸载事件
        /// </summary>
        public event EventHandler<PluginEventArgs>? PluginUnloaded;
        
        /// <summary>
        /// 插件激活事件
        /// </summary>
        public event EventHandler<PluginEventArgs>? PluginActivated;
        
        /// <summary>
        /// 插件停用事件
        /// </summary>
        public event EventHandler<PluginEventArgs>? PluginDeactivated;
        
        /// <summary>
        /// 初始化插件管理器
        /// </summary>
        /// <param name="pluginsDirectory">插件目录路径</param>
        /// <param name="logService">日志服务实例（可选）</param>
        public static void Initialize(string pluginsDirectory, WPFPluginToolbox.Services.LogService? logService = null)
        {
            if (_instance == null)
            {
                _instance = new PluginManager(pluginsDirectory, logService);
            }
        }
        
        /// <summary>
        /// 私有构造函数
        /// </summary>
        /// <param name="pluginsDirectory">插件目录路径</param>
        /// <param name="logService">日志服务实例</param>
        private PluginManager(string pluginsDirectory, WPFPluginToolbox.Services.LogService? logService = null)
        {
            _pluginLoader = new PluginLoader(pluginsDirectory, logService);
        }
        
        /// <summary>
        /// 加载所有插件
        /// </summary>
        public void LoadAllPlugins()
        {
            _pluginLoader.LoadAllPlugins();
            
            // 触发所有已加载插件的加载事件
            foreach (var plugin in _pluginLoader.GetLoadedPlugins())
            {
                PluginLoaded?.Invoke(this, new PluginEventArgs { Plugin = plugin });
            }
        }
        
        /// <summary>
        /// 卸载所有插件
        /// </summary>
        public void UnloadAllPlugins()
        {
            // 触发所有已加载插件的卸载事件
            foreach (var plugin in _pluginLoader.GetLoadedPlugins())
            {
                PluginUnloaded?.Invoke(this, new PluginEventArgs { Plugin = plugin });
            }
            
            _pluginLoader.UnloadAllPlugins();
        }
        
        /// <summary>
        /// 重新加载所有插件
        /// </summary>
        public void ReloadPlugins()
        {
            UnloadAllPlugins();
            LoadAllPlugins();
        }
        
        /// <summary>
        /// 获取所有已加载的插件
        /// </summary>
        /// <returns>已加载插件列表</returns>
        public List<IPlugin> GetLoadedPlugins()
        {
            return _pluginLoader.GetLoadedPlugins();
        }
        
        /// <summary>
        /// 获取所有已加载的依赖
        /// </summary>
        /// <returns>已加载依赖列表</returns>
        public List<IDependency> GetLoadedDependencies()
        {
            return _pluginLoader.GetLoadedDependencies();
        }
        
        /// <summary>
        /// 激活插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        public void ActivatePlugin(string pluginId)
        {
            var plugin = GetPluginById(pluginId);
            if (plugin != null)
            {
                plugin.Activate();
                PluginActivated?.Invoke(this, new PluginEventArgs { Plugin = plugin });
            }
        }
        
        /// <summary>
        /// 停用插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        public void DeactivatePlugin(string pluginId)
        {
            var plugin = GetPluginById(pluginId);
            if (plugin != null)
            {
                plugin.Deactivate();
                PluginDeactivated?.Invoke(this, new PluginEventArgs { Plugin = plugin });
            }
        }
        
        /// <summary>
        /// 根据ID获取插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>插件实例，不存在则返回null</returns>
        public IPlugin? GetPluginById(string pluginId)
        {
            return _pluginLoader.GetLoadedPlugins().Find(p => p.Id == pluginId);
        }
        
        /// <summary>
        /// 根据ID获取依赖
        /// </summary>
        /// <param name="dependencyId">依赖ID</param>
        /// <returns>依赖实例，不存在则返回null</returns>
        public IDependency? GetDependencyById(string dependencyId)
        {
            return _pluginLoader.GetLoadedDependencies().Find(d => d.Id == dependencyId);
        }
        
        /// <summary>
        /// 卸载指定插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>是否成功卸载</returns>
        public bool UnloadPlugin(string pluginId)
        {
            var plugin = GetPluginById(pluginId);
            if (plugin != null)
            {
                bool success = _pluginLoader.UnloadPlugin(pluginId);
                if (success)
                {
                    PluginUnloaded?.Invoke(this, new PluginEventArgs { Plugin = plugin });
                }
                return success;
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
            return _pluginLoader.ReloadPlugin(pluginId);
        }
        
        /// <summary>
        /// 导出指定插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <param name="exportPath">导出路径</param>
        /// <returns>是否成功导出</returns>
        public bool ExportPlugin(string pluginId, string exportPath)
        {
            return _pluginLoader.ExportPlugin(pluginId, exportPath);
        }
        
        /// <summary>
        /// 删除指定插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>是否成功删除</returns>
        public bool DeletePlugin(string pluginId)
        {
            var plugin = GetPluginById(pluginId);
            if (plugin != null)
            {
                bool success = _pluginLoader.DeletePlugin(pluginId);
                return success;
            }
            
            // 检查是否为已卸载的插件
            var unloadedPlugins = _pluginLoader.GetUnloadedPlugins();
            var unloadedPlugin = unloadedPlugins.Find(p => p.Id == pluginId);
            if (unloadedPlugin != null)
            {
                return _pluginLoader.DeletePlugin(pluginId);
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取所有已卸载的插件信息
        /// </summary>
        /// <returns>已卸载插件信息列表</returns>
        public List<PluginMetadata> GetUnloadedPlugins()
        {
            return _pluginLoader.GetUnloadedPlugins();
        }
        
        /// <summary>
        /// 获取所有插件信息（包括已加载和已卸载）
        /// </summary>
        /// <returns>所有插件信息列表</returns>
        public List<PluginMetadata> GetAllPlugins()
        {
            return _pluginLoader.GetAllPlugins();
        }
        
        /// <summary>
        /// 根据插件ID获取插件元数据
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>插件元数据，不存在则返回null</returns>
        public PluginMetadata? GetPluginMetadataById(string pluginId)
        {
            return GetAllPlugins().FirstOrDefault(p => p.Id == pluginId);
        }
    }
    
    /// <summary>
    /// 插件事件参数
    /// </summary>
    public class PluginEventArgs : EventArgs
    {
        /// <summary>
        /// 插件实例
        /// </summary>
        public IPlugin? Plugin { get; set; }
    }
}