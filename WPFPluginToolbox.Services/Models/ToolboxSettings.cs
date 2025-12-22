using System.ComponentModel;
using WPFPluginToolbox.Core;

namespace WPFPluginToolbox.Services.Models
{
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