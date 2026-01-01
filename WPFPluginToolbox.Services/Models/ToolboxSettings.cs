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
        
        /// <summary>
        /// 主窗口宽度
        /// </summary>
        public double MainWindowWidth { get; set; } = 1200;
        
        /// <summary>
        /// 主窗口高度
        /// </summary>
        public double MainWindowHeight { get; set; } = 700;
        
        /// <summary>
        /// 底部调试信息窗口是否显示
        /// </summary>
        public bool IsDebugPanelVisible { get; set; } = true;
        
        /// <summary>
        /// 左侧插件栏是否显示
        /// </summary>
        public bool IsPluginPanelVisible { get; set; } = true;
    }
}