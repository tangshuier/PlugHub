using WPFPluginToolbox.Core;

namespace WPFPluginToolbox.PluginSystem
{
    /// <summary>
    /// 插件元数据类，存储插件的基本信息、状态和依赖关系
    /// </summary>
    public class PluginMetadata
    {
        /// <summary>
        /// 插件ID
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// 插件名称
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// 插件描述
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 插件版本
        /// </summary>
        public string? Version { get; set; }
        
        /// <summary>
        /// 插件类型
        /// </summary>
        public PluginType Type { get; set; }
        
        /// <summary>
        /// 插件路径
        /// </summary>
        public string? PluginPath { get; set; }
        
        /// <summary>
        /// 插件是否激活
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// 插件是否已加载
        /// </summary>
        public bool IsLoaded { get; set; }
        
        /// <summary>
        /// 插件是否有错误
        /// </summary>
        public bool HasError { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// 插件依赖列表
        /// </summary>
        public List<PluginDependency> Dependencies { get; set; } = new List<PluginDependency>();
        
        /// <summary>
        /// 已解析的依赖列表
        /// </summary>
        public List<IDependency> ResolvedDependencies { get; set; } = new List<IDependency>();
        
        /// <summary>
        /// 未解析的依赖列表
        /// </summary>
        public List<string> UnresolvedDependencies { get; set; } = new List<string>();
        
        /// <summary>
        /// 插件配置文件路径
        /// </summary>
        public string? ConfigFilePath { get; set; }
    }
}
