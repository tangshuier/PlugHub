using System.Windows.Media;
using WPFPluginToolbox.Core;
using WPFPluginToolbox.Services.Models;

namespace WPFPluginToolbox.Services
{
    /// <summary>
    /// 主题服务，实现主题相关的功能
    /// </summary>
    public class ThemeService : WPFPluginToolbox.Core.IThemeService
    {
        private readonly SettingsService _settingsService;
        private ToolboxTheme _currentTheme;
        
        /// <summary>
        /// 主题变更事件
        /// </summary>
        public event EventHandler<ToolboxTheme>? ThemeChanged;
        
        /// <summary>
        /// 获取当前主题
        /// </summary>
        public ToolboxTheme CurrentTheme => _currentTheme;
        
        /// <summary>
        /// 获取当前主题的主背景色
        /// </summary>
        public Brush MainBackgroundBrush => GetMainBackgroundBrush(_currentTheme);
        
        /// <summary>
        /// 获取当前主题的主前景色
        /// </summary>
        public Brush MainForegroundBrush => GetMainForegroundBrush(_currentTheme);
        
        /// <summary>
        /// 获取当前主题的插件面板背景色
        /// </summary>
        public Brush PluginPanelBackgroundBrush => GetPluginPanelBackgroundBrush(_currentTheme);
        
        /// <summary>
        /// 获取当前主题的插件工作区背景色
        /// </summary>
        public Brush PluginWorkspaceBackgroundBrush => GetPluginWorkspaceBackgroundBrush(_currentTheme);
        
        /// <summary>
        /// 获取当前主题的调试面板背景色
        /// </summary>
        public Brush DebugPanelBackgroundBrush => GetDebugPanelBackgroundBrush(_currentTheme);
        
        /// <summary>
        /// 获取当前主题的工具栏背景色
        /// </summary>
        public Brush ToolBarBackgroundBrush => GetToolBarBackgroundBrush(_currentTheme);
        
        /// <summary>
        /// 获取当前主题的边框颜色
        /// </summary>
        public Brush BorderBrush => GetBorderBrush(_currentTheme);
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="settingsService">设置服务</param>
        public ThemeService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _currentTheme = _settingsService.GetSettings().Theme;
        }
        
        /// <summary>
        /// 设置主题
        /// </summary>
        /// <param name="theme">主题枚举</param>
        public void SetTheme(ToolboxTheme theme)
        {
            if (_currentTheme != theme)
            {
                _currentTheme = theme;
                
                // 保存主题到设置
                var settings = _settingsService.GetSettings();
                settings.Theme = theme;
                _settingsService.SaveSettings(settings);
                
                // 触发主题变更事件
                ThemeChanged?.Invoke(this, theme);
            }
        }
        
        /// <summary>
        /// 获取主题对应的主背景色
        /// </summary>
        /// <param name="theme">主题枚举</param>
        /// <returns>主背景色</returns>
        private Brush GetMainBackgroundBrush(ToolboxTheme theme)
        {
            switch (theme)
            {
                case ToolboxTheme.Black:
                    return new SolidColorBrush(Color.FromRgb(30, 30, 30));
                case ToolboxTheme.White:
                    return new SolidColorBrush(Color.FromRgb(245, 245, 245));
                case ToolboxTheme.LightBlack:
                    return new SolidColorBrush(Color.FromRgb(50, 50, 50));
                case ToolboxTheme.Gray:
                    return new SolidColorBrush(Color.FromRgb(190, 190, 190));
                default:
                    return new SolidColorBrush(Color.FromRgb(30, 30, 30));
            }
        }
        
        /// <summary>
        /// 获取主题对应的主前景色
        /// </summary>
        /// <param name="theme">主题枚举</param>
        /// <returns>主前景色</returns>
        private Brush GetMainForegroundBrush(ToolboxTheme theme)
        {
            switch (theme)
            {
                case ToolboxTheme.Black:
                case ToolboxTheme.LightBlack:
                    return Brushes.White;
                case ToolboxTheme.White:
                case ToolboxTheme.Gray:
                    return Brushes.Black;
                default:
                    return Brushes.White;
            }
        }
        
        /// <summary>
        /// 获取主题对应的插件面板背景色
        /// </summary>
        /// <param name="theme">主题枚举</param>
        /// <returns>插件面板背景色</returns>
        private Brush GetPluginPanelBackgroundBrush(ToolboxTheme theme)
        {
            switch (theme)
            {
                case ToolboxTheme.Black:
                    return new SolidColorBrush(Color.FromRgb(40, 40, 40));
                case ToolboxTheme.White:
                    return new SolidColorBrush(Color.FromRgb(235, 235, 235));
                case ToolboxTheme.LightBlack:
                    return new SolidColorBrush(Color.FromRgb(60, 60, 60));
                case ToolboxTheme.Gray:
                    return new SolidColorBrush(Color.FromRgb(200, 200, 200));
                default:
                    return new SolidColorBrush(Color.FromRgb(40, 40, 40));
            }
        }
        
        /// <summary>
        /// 获取主题对应的插件工作区背景色
        /// </summary>
        /// <param name="theme">主题枚举</param>
        /// <returns>插件工作区背景色</returns>
        private Brush GetPluginWorkspaceBackgroundBrush(ToolboxTheme theme)
        {
            switch (theme)
            {
                case ToolboxTheme.Black:
                    return new SolidColorBrush(Color.FromRgb(45, 45, 45));
                case ToolboxTheme.White:
                    return new SolidColorBrush(Color.FromRgb(250, 250, 250));
                case ToolboxTheme.LightBlack:
                    return new SolidColorBrush(Color.FromRgb(65, 65, 65));
                case ToolboxTheme.Gray:
                    return new SolidColorBrush(Color.FromRgb(210, 210, 210));
                default:
                    return new SolidColorBrush(Color.FromRgb(45, 45, 45));
            }
        }
        
        /// <summary>
        /// 获取主题对应的调试面板背景色
        /// </summary>
        /// <param name="theme">主题枚举</param>
        /// <returns>调试面板背景色</returns>
        private Brush GetDebugPanelBackgroundBrush(ToolboxTheme theme)
        {
            switch (theme)
            {
                case ToolboxTheme.Black:
                    return new SolidColorBrush(Color.FromRgb(40, 40, 40));
                case ToolboxTheme.White:
                    return new SolidColorBrush(Color.FromRgb(235, 235, 235));
                case ToolboxTheme.LightBlack:
                    return new SolidColorBrush(Color.FromRgb(60, 60, 60));
                case ToolboxTheme.Gray:
                    return new SolidColorBrush(Color.FromRgb(200, 200, 200));
                default:
                    return new SolidColorBrush(Color.FromRgb(40, 40, 40));
            }
        }
        
        /// <summary>
        /// 获取主题对应的工具栏背景色
        /// </summary>
        /// <param name="theme">主题枚举</param>
        /// <returns>工具栏背景色</returns>
        private Brush GetToolBarBackgroundBrush(ToolboxTheme theme)
        {
            switch (theme)
            {
                case ToolboxTheme.Black:
                    return new SolidColorBrush(Color.FromRgb(50, 50, 50));
                case ToolboxTheme.White:
                    return new SolidColorBrush(Color.FromRgb(225, 225, 225));
                case ToolboxTheme.LightBlack:
                    return new SolidColorBrush(Color.FromRgb(70, 70, 70));
                case ToolboxTheme.Gray:
                    return new SolidColorBrush(Color.FromRgb(210, 210, 210));
                default:
                    return new SolidColorBrush(Color.FromRgb(50, 50, 50));
            }
        }
        
        /// <summary>
        /// 获取主题对应的边框颜色
        /// </summary>
        /// <param name="theme">主题枚举</param>
        /// <returns>边框颜色</returns>
        private Brush GetBorderBrush(ToolboxTheme theme)
        {
            switch (theme)
            {
                case ToolboxTheme.Black:
                    return new SolidColorBrush(Color.FromRgb(70, 70, 70));
                case ToolboxTheme.White:
                    return new SolidColorBrush(Color.FromRgb(200, 200, 200));
                case ToolboxTheme.LightBlack:
                    return new SolidColorBrush(Color.FromRgb(80, 80, 80));
                case ToolboxTheme.Gray:
                    return new SolidColorBrush(Color.FromRgb(180, 180, 180));
                default:
                    return new SolidColorBrush(Color.FromRgb(70, 70, 70));
            }
        }
    }
}
