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
            
            try
            {
                // 订阅主题变更事件
                _pluginApi.ThemeChanged += OnThemeChanged;
                
                // 初始化主题显示
                UpdateTheme();
            }
            catch (Exception ex)
            {
                _pluginApi.Debug($"初始化主题事件订阅失败: {ex.Message}");
            }
        }

        private void OnThemeChanged(object sender, ToolboxTheme theme)
        {
            _pluginApi.Debug($"收到主题变更事件: {theme}");
            UpdateTheme();
        }

        private void UpdateTheme()
        {
            try
            {
                // 获取主题信息
                var savedTheme = _pluginApi.SavedTheme;
                var currentTheme = _pluginApi.CurrentTheme;
                
                // 更新主题文本
                UpdateThemeText(savedTheme, currentTheme);
                
                // 使用API提供的方法自动应用主题到整个控件
                _pluginApi.ApplyThemeToElement(this);
                
                // 获取主题颜色（用于预览显示）
                var mainBgBrush = _pluginApi.CurrentBackgroundBrush;
                var mainFgBrush = _pluginApi.CurrentForegroundBrush;
                var panelBgBrush = _pluginApi.PluginPanelBackgroundBrush;
                var workspaceBgBrush = _pluginApi.PluginWorkspaceBackgroundBrush;
                var borderBrush = _pluginApi.BorderBrush;
                
                // 更新主题色预览
                UpdateColorPreviews(mainBgBrush, mainFgBrush, panelBgBrush, workspaceBgBrush, borderBrush);
                
                // 更新代码示例部分
                UpdateCodeExampleTheme();
                
                _pluginApi.Debug($"主题更新完成: 已保存={savedTheme}, 当前={currentTheme}");
            }
            catch (Exception ex)
            {
                _pluginApi.Debug($"更新主题失败: {ex.Message}");
            }
        }

        private void UpdateThemeText(ToolboxTheme savedTheme, ToolboxTheme currentTheme)
        {
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
        }
        
        private void UpdateColorPreviews(Brush mainBgBrush, Brush mainFgBrush, Brush panelBgBrush, Brush workspaceBgBrush, Brush borderBrush)
        {
            // 更新主背景色预览
            MainBgColorBorder.Background = mainBgBrush;
            MainBgColorBorder.BorderBrush = borderBrush;
            
            // 更新主前景色预览
            MainFgColorBorder.Background = mainFgBrush;
            MainFgColorBorder.BorderBrush = borderBrush;
            
            // 更新面板背景色预览
            PanelBgColorBorder.Background = panelBgBrush;
            PanelBgColorBorder.BorderBrush = borderBrush;
            
            // 更新工作区背景色预览
            WorkspaceBgColorBorder.Background = workspaceBgBrush;
            WorkspaceBgColorBorder.BorderBrush = borderBrush;
        }

        private void UpdateCodeExampleTheme()
        {
            // 代码示例部分的主题已经通过 ApplyThemeToElement 自动应用
            // 这里可以添加其他需要特殊处理的逻辑
        }

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