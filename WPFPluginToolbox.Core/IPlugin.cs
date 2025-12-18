using System.Windows.Controls;

namespace WPFPluginToolbox.Core
{
    /// <summary>
    /// 插件接口，代表可加载的插件
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 插件ID
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// 插件名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 插件描述
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// 插件版本
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// 插件类型
        /// </summary>
        PluginType Type { get; }
        
        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="pluginApi">插件API实例</param>
        void Initialize(IPluginAPI pluginApi);
        
        /// <summary>
        /// 激活插件
        /// </summary>
        void Activate();
        
        /// <summary>
        /// 停用插件
        /// </summary>
        void Deactivate();
        
        /// <summary>
        /// 获取插件主视图
        /// </summary>
        /// <returns>插件主视图</returns>
        UserControl GetMainView();
        
        /// <summary>
        /// 清理插件资源
        /// </summary>
        void Dispose();
    }
}
