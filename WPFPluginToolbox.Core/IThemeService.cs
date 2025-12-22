using System.Windows.Media;

namespace WPFPluginToolbox.Core
{
    /// <summary>
    /// 主题服务接口，提供主题相关的功能
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// 获取当前主题
        /// </summary>
        ToolboxTheme CurrentTheme { get; }
        
        /// <summary>
        /// 获取当前主题的主背景色
        /// </summary>
        Brush MainBackgroundBrush { get; }
        
        /// <summary>
        /// 获取当前主题的主前景色
        /// </summary>
        Brush MainForegroundBrush { get; }
        
        /// <summary>
        /// 获取当前主题的插件面板背景色
        /// </summary>
        Brush PluginPanelBackgroundBrush { get; }
        
        /// <summary>
        /// 获取当前主题的插件工作区背景色
        /// </summary>
        Brush PluginWorkspaceBackgroundBrush { get; }
        
        /// <summary>
        /// 获取当前主题的调试面板背景色
        /// </summary>
        Brush DebugPanelBackgroundBrush { get; }
        
        /// <summary>
        /// 获取当前主题的工具栏背景色
        /// </summary>
        Brush ToolBarBackgroundBrush { get; }
        
        /// <summary>
        /// 获取当前主题的边框颜色
        /// </summary>
        Brush BorderBrush { get; }
        
        /// <summary>
        /// 设置主题
        /// </summary>
        /// <param name="theme">主题枚举</param>
        void SetTheme(ToolboxTheme theme);
        
        /// <summary>
        /// 主题变更事件
        /// </summary>
        event EventHandler<ToolboxTheme>? ThemeChanged;
    }
}