using System;

namespace ConfigTestPlugin
{
    /// <summary>
    /// 插件配置类
    /// </summary>
    public class Config
    {
        /// <summary>
        /// 选中的选项索引
        /// </summary>
        public int SelectedOptionIndex { get; set; } = 0;
        
        /// <summary>
        /// 其他配置项示例
        /// </summary>
        public string ExampleString { get; set; } = "默认值";
        
        /// <summary>
        /// 数值配置示例
        /// </summary>
        public int ExampleNumber { get; set; } = 42;
    }
}