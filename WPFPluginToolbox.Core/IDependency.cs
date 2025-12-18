namespace WPFPluginToolbox.Core
{
    /// <summary>
    /// 依赖接口，代表插件可以使用的依赖
    /// </summary>
    public interface IDependency
    {
        /// <summary>
        /// 依赖ID
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// 依赖名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 依赖描述
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// 依赖版本
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// 初始化依赖
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 清理依赖资源
        /// </summary>
        void Dispose();
    }
}
