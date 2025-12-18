using System.ComponentModel;

namespace WPFPluginToolbox.Services.Models
{
    /// <summary>
    /// 工具箱主题枚举
    /// </summary>
    public enum ToolboxTheme
    {
        [Description("黑色")]
        Black,
        
        [Description("白色")]
        White,
        
        [Description("浅黑色")]
        LightBlack,
        
        [Description("灰色")]
        Gray
    }
    
    /// <summary>
    /// 工具箱设置类
    /// </summary>
    public class ToolboxSettings
    {
        /// <summary>
        /// 工具箱主题
        /// </summary>
        public ToolboxTheme Theme { get; set; } = ToolboxTheme.Black;
        
        /// <summary>
        /// 调试窗口是否默认打开
        /// </summary>
        public bool IsDebugWindowDefaultOpen { get; set; } = false;
    }
}