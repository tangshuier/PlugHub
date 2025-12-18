namespace WPFPluginToolbox.Core
{
    /// <summary>
    /// 插件类型枚举
    /// </summary>
    public enum PluginType
    {
        /// <summary>
        /// 插件，可以独立运行，拥有独立页面和功能
        /// </summary>
        Plugin,
        /// <summary>
        /// 依赖，提供API功能
        /// </summary>
        Dependency
    }
}
