namespace WPFPluginToolbox.Core
{
    /// <summary>
    /// 依赖类型枚举
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// 必要依赖，缺少则插件无法加载
        /// </summary>
        Required,
        /// <summary>
        /// 非必要依赖，缺少时插件仍可运行，但功能可能受限
        /// </summary>
        Optional
    }
}
