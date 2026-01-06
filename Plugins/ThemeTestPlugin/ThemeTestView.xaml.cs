using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFPluginToolbox.Core;

namespace ThemeTestPlugin
{
    public partial class ThemeTestView : UserControl
    {
        private readonly IPluginAPI _pluginApi;

        public ThemeTestView(IPluginAPI pluginApi)
        {
            InitializeComponent();
            _pluginApi = pluginApi;
            
            _pluginApi.Debug("主题色测试插件视图初始化完成");
            
            // 先初始化显示，避免初始显示为"-"
            CurrentThemeTextBlock.Text = "正在获取主题信息...";
            
            try
            {
                // 订阅主题变更事件
                _pluginApi.ThemeChanged += OnThemeChanged;
                
                // 初始化主题显示
                UpdateThemeInfo();
            }
            catch (Exception ex)
            {
                _pluginApi.Debug($"初始化主题事件订阅失败: {ex.Message}");
                CurrentThemeTextBlock.Text = "主题初始化失败";
            }
        }

        private void OnThemeChanged(object sender, ToolboxTheme theme)
        {
            UpdateThemeInfo();
        }

        private void UpdateThemeInfo()
        {
            try
            {
                // 直接从PluginAPI获取主题信息，不需要ThemeService
                var savedTheme = _pluginApi.SavedTheme;
                var currentTheme = _pluginApi.CurrentTheme;
                
                // 获取主题的友好名称
                var savedThemeName = GetThemeFriendlyName(savedTheme);
                var currentThemeName = GetThemeFriendlyName(currentTheme);
                
                // 更新主题显示
                SavedThemeTextBlock.Text = savedThemeName;
                CurrentThemeTextBlock.Text = currentThemeName;
                
                // 设置主题状态提示
                if (savedTheme == currentTheme)
                {
                    ThemeStatusTextBlock.Text = "当前预览主题与已保存主题一致";
                    ThemeStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    ThemeStatusTextBlock.Text = "当前预览主题与已保存主题不同，点击保存设置按钮即可保存当前预览主题";
                    ThemeStatusTextBlock.Foreground = new SolidColorBrush(Colors.Orange);
                }
                
                // 更新主题色预览
                MainBgColorBorder.Background = _pluginApi.CurrentBackgroundBrush;
                MainFgColorBorder.Background = _pluginApi.CurrentForegroundBrush;
                PanelBgColorBorder.Background = _pluginApi.PluginPanelBackgroundBrush;
                WorkspaceBgColorBorder.Background = _pluginApi.PluginWorkspaceBackgroundBrush;
                
                // 关键修复：更新整个视图的主题样式
                // 这确保了所有UI元素都能正确应用当前主题
                _pluginApi.ApplyThemeToElement(this);
                
                _pluginApi.Debug($"主题更新: 已保存={savedTheme}, 当前={currentTheme}");
            }
            catch (Exception ex)
            {
                SavedThemeTextBlock.Text = "主题获取失败";
                CurrentThemeTextBlock.Text = "主题获取失败";
                ThemeStatusTextBlock.Text = "主题信息获取失败";
                ThemeStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                
                _pluginApi.Debug($"更新主题信息失败: {ex.Message}");
                
                // 设置默认透明背景，避免视觉异常
                MainBgColorBorder.Background = new SolidColorBrush(Colors.Transparent);
                MainFgColorBorder.Background = new SolidColorBrush(Colors.Transparent);
                PanelBgColorBorder.Background = new SolidColorBrush(Colors.Transparent);
                WorkspaceBgColorBorder.Background = new SolidColorBrush(Colors.Transparent);
            }
        }
        
        /// <summary>
        /// 获取主题的友好名称
        /// </summary>
        private string GetThemeFriendlyName(ToolboxTheme theme)
        {
            return theme switch
            {
                ToolboxTheme.Black => "黑色主题",
                ToolboxTheme.White => "白色主题",
                ToolboxTheme.LightBlack => "浅黑色主题",
                ToolboxTheme.Gray => "灰色主题",
                _ => theme.ToString()
            };
        }
    }
}