

namespace WPFPluginToolbox.Core
{
    /// <summary>
    /// 插件配置接口
    /// </summary>
    public interface IPluginConfig
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// 插件版本
        /// </summary>
        string Version { get; set; }
        
        /// <summary>
        /// 插件描述
        /// </summary>
        string Description { get; set; }
        
        /// <summary>
        /// 插件作者
        /// </summary>
        string Author { get; set; }
        
        /// <summary>
        /// 插件类型
        /// </summary>
        PluginType Type { get; set; }
        
        /// <summary>
        /// 插件依赖列表
        /// </summary>
        List<PluginDependency> Dependencies { get; set; }
    }
    
    /// <summary>
    /// 插件依赖项
    /// </summary>
    public class PluginDependency
    {
        /// <summary>
        /// 依赖的插件ID
        /// </summary>
        public string? PluginId { get; set; }
        
        /// <summary>
        /// 依赖类型
        /// </summary>
        public DependencyType Type { get; set; }
    }
}
