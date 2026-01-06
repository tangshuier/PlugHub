using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFPluginToolbox.Core;

namespace ChineseFontMatrixPlugin
{
    public partial class ChineseFontMatrixPluginView : System.Windows.Controls.UserControl, IDisposable
    {
        // 使用System.Windows.Forms.FolderBrowserDialog替代Windows API调用

        private readonly IPluginAPI _pluginApi;
        private PluginConfig _config;
        private HashSet<char> _chineseChars = new HashSet<char>();
        private Dictionary<char, byte[]> _charMatrixData = new Dictionary<char, byte[]>();

        // 配置类
        public class PluginConfig
        {
            public string FontName { get; set; } = "SimHei";
            public int FontSize { get; set; } = 16;
            public string Mode { get; set; } = "行列式";
            public string CodeType { get; set; } = "阳码";
            public string BitOrder { get; set; } = "低位在前";
            public string LastProjectPath { get; set; } = ".";
            public bool ClearExistingFonts { get; set; } = false;
            public string DuplicateHandling { get; set; } = "ask";
            public bool RememberChoice { get; set; } = false;
        }

        public ChineseFontMatrixPluginView(IPluginAPI pluginApi)
        {
            InitializeComponent();
            _pluginApi = pluginApi;
            
            // 重新启用主题同步
            _pluginApi.SyncToolboxTheme = true;
            
            // 订阅主题变更事件
            _pluginApi.ThemeChanged += OnThemeChanged;
            
            // 初始化配置
            _config = new PluginConfig();
            
            // 延迟初始化，确保控件完全加载
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _config = LoadConfig();
                InitializeUI();
                // 强制更新主题颜色
                UpdateThemeColors();
                // 再次强制更新，确保颜色设置生效
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateThemeColors();
                }), System.Windows.Threading.DispatcherPriority.Render);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        
        // 主题变更事件处理
        private void OnThemeChanged(object? sender, ToolboxTheme theme)
        {
            // 延迟更新，确保工具箱的主题同步完成后再更新
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateThemeColors(theme);
                // 再次强制更新，确保颜色设置生效
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateThemeColors(theme);
                }), System.Windows.Threading.DispatcherPriority.Render);
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
        
        // ComboBox加载完成事件处理
        private void OnComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Controls.ComboBox comboBox)
                {
                    // 强制更新主题颜色
                    UpdateThemeColors();
                    // 专门更新这个ComboBox的颜色
                    var currentTheme = _pluginApi.CurrentTheme;
                    var backgroundBrush = _pluginApi.CurrentBackgroundBrush;
                    var foregroundBrush = _pluginApi.CurrentForegroundBrush;
                    bool isDarkTheme = IsDarkBackground(backgroundBrush);
                    
                    System.Windows.Media.Brush controlBackgroundBrush;
                    System.Windows.Media.Brush borderBrush;
                    
                    if (isDarkTheme)
                    {
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
                    }
                    else
                    {
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 195, 199));
                    }
                    
                    // 直接更新这个ComboBox
                    UpdateComboBoxColors(comboBox, controlBackgroundBrush, foregroundBrush, borderBrush);
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新ComboBox加载颜色失败", ex);
            }
        }
        
        // 生成方式变更事件处理
        private void OnGenerateModeChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // 检查控件是否已初始化
                if (ManualRadio == null || BothRadio == null || ManualInputGrid == null)
                {
                    return;
                }
                
                // 根据选择的生成方式控制手动输入区域的可见性
                if (ManualRadio.IsChecked == true || BothRadio.IsChecked == true)
                {
                    ManualInputGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    ManualInputGrid.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新生成方式失败", ex);
            }
        }
        
        // 更新主题颜色
        private void UpdateThemeColors()
        {
            try
            {
                // 获取主题相关的颜色
                var backgroundBrush = _pluginApi.CurrentBackgroundBrush;
                var foregroundBrush = _pluginApi.CurrentForegroundBrush;
                
                // 直接更新控件颜色，跳过资源字典更新，确保立即生效
                bool isDarkTheme = IsDarkBackground(backgroundBrush);
                
                // 基于背景色推断实际主题
                var inferredTheme = InferThemeFromBackground(backgroundBrush);
                
                // 确保使用正确的前景色
                if (inferredTheme == ToolboxTheme.White || inferredTheme == ToolboxTheme.Gray)
                {
                    // 浅色主题使用深色前景色
                    foregroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 49, 64));
                }
                else
                {
                    // 深色主题使用浅色前景色
                    foregroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                }
                
                UpdateControlColors(inferredTheme, isDarkTheme, backgroundBrush, foregroundBrush);
                
                // 强制更新所有控件的样式
                UpdateAllControls();
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新主题颜色失败", ex);
            }
        }
        
        // 更新主题颜色（带主题参数）
        private void UpdateThemeColors(ToolboxTheme theme)
        {
            try
            {
                // 获取主题相关的颜色
                var backgroundBrush = _pluginApi.CurrentBackgroundBrush;
                var foregroundBrush = _pluginApi.CurrentForegroundBrush;
                
                // 直接更新控件颜色，跳过资源字典更新，确保立即生效
                bool isDarkTheme = IsDarkBackground(backgroundBrush);
                
                // 基于背景色推断实际主题
                var inferredTheme = InferThemeFromBackground(backgroundBrush);
                
                // 确保使用正确的前景色
                if (inferredTheme == ToolboxTheme.White || inferredTheme == ToolboxTheme.Gray)
                {
                    // 浅色主题使用深色前景色
                    foregroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 49, 64));
                }
                else
                {
                    // 深色主题使用浅色前景色
                    foregroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                }
                
                UpdateControlColors(inferredTheme, isDarkTheme, backgroundBrush, foregroundBrush);
                
                // 强制更新所有控件的样式
                UpdateAllControls();
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新主题颜色失败", ex);
            }
        }
        
        // 强制更新所有控件的样式
        private void UpdateAllControls()
        {
            try
            {
                // 不重新应用样式，因为我们已经通过直接设置属性来更新颜色
                // 这样可以确保我们的颜色设置不会被样式覆盖
                
                // 但是我们可以确保所有控件都已加载并显示正确的颜色
                // 例如，确保手动输入区域的可见性设置正确
                OnGenerateModeChanged(null, null);
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新控件样式失败", ex);
            }
        }
        
        // 应用样式到控件
        private void ApplyStyleToControl(FrameworkElement control, Style style)
        {
            if (control != null && style != null)
            {
                control.Style = null;
                control.Style = style;
            }
        }
        
        // 更新资源字典
        private void UpdateResourceDictionary(ToolboxTheme theme, System.Windows.Media.Brush backgroundBrush, System.Windows.Media.Brush foregroundBrush)
        {
            try
            {
                // 计算是否为深色主题
                bool isDarkTheme = IsDarkBackground(backgroundBrush);
                
                // 直接更新控件的背景色和前景色，而不是通过资源字典
                // 这样可以确保控件立即反映主题变更
                UpdateControlColors(theme, isDarkTheme, backgroundBrush, foregroundBrush);
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新资源字典失败", ex);
            }
        }
        
        // 直接更新控件颜色
        private void UpdateControlColors(ToolboxTheme theme, bool isDarkTheme, System.Windows.Media.Brush backgroundBrush, System.Windows.Media.Brush foregroundBrush)
        {
            try
            {
                // 根据主题类型设置更精确的颜色
                System.Windows.Media.Brush controlBackgroundBrush;
                System.Windows.Media.Brush borderBrush;
                System.Windows.Media.Brush headerBackgroundBrush;
                System.Windows.Media.Brush secondaryBackgroundBrush;
                System.Windows.Media.Brush primaryColor;
                System.Windows.Media.Brush primaryDarkColor;
                System.Windows.Media.Brush primaryDarkerColor;
                System.Windows.Media.Brush textSecondaryColor;
                System.Windows.Media.Brush textTertiaryColor;
                
                // 使用传入的主题类型
                var currentTheme = theme;
                
                // 根据不同主题设置不同的颜色方案
                switch (currentTheme)
                {
                    case ToolboxTheme.White:
                        // 白色主题
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 195, 199));
                        headerBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 249, 250));
                        secondaryBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 249, 250));
                        primaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                        primaryDarkColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 128, 185));
                        primaryDarkerColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(31, 97, 141));
                        textSecondaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 49, 64));
                        textTertiaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(85, 85, 85));
                        break;
                    case ToolboxTheme.Black:
                        // 黑色主题
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 20));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40));
                        headerBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
                        secondaryBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
                        primaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 128, 185));
                        primaryDarkColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(31, 97, 141));
                        primaryDarkerColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 82, 118));
                        textSecondaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                        textTertiaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 150, 150));
                        break;
                    case ToolboxTheme.LightBlack:
                        // 浅黑色主题
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
                        headerBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
                        secondaryBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
                        primaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                        primaryDarkColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 128, 185));
                        primaryDarkerColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(31, 97, 141));
                        textSecondaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                        textTertiaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 150, 150));
                        break;
                    case ToolboxTheme.Gray:
                        // 灰色主题
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 190, 190));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(170, 170, 170));
                        headerBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 190, 190));
                        secondaryBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 190, 190));
                        primaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                        primaryDarkColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 128, 185));
                        primaryDarkerColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(31, 97, 141));
                        textSecondaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 50, 50));
                        textTertiaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100));
                        break;
                    default:
                        // 默认主题
                        if (isDarkTheme)
                        {
                            controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
                            borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
                            headerBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
                            secondaryBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
                            primaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                            primaryDarkColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 128, 185));
                            primaryDarkerColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(31, 97, 141));
                            textSecondaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                            textTertiaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 150, 150));
                        }
                        else
                        {
                            controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                            borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 195, 199));
                            headerBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 249, 250));
                            secondaryBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 249, 250));
                            primaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                            primaryDarkColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 128, 185));
                            primaryDarkerColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(31, 97, 141));
                            textSecondaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 49, 64));
                            textTertiaryColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(85, 85, 85));
                        }
                        break;
                }
                
                // 根据当前主题设置正确的背景色
                System.Windows.Media.Brush actualBackgroundBrush;
                if (theme == ToolboxTheme.White)
                {
                    actualBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                }
                else if (theme == ToolboxTheme.Gray)
                {
                    actualBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 190, 190));
                }
                else if (theme == ToolboxTheme.Black)
                {
                    actualBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
                }
                else // LightBlack
                {
                    actualBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
                }
                
                // 更新容器控件的背景色
                this.Background = actualBackgroundBrush;
                
                // 更新主Grid背景色
                var mainGrid = FindVisualChild<System.Windows.Controls.Grid>(this);
                if (mainGrid != null)
                {
                    mainGrid.Background = actualBackgroundBrush;
                }
                
                // 更新TabControl背景色
                var tabControl = FindVisualChild<System.Windows.Controls.TabControl>(this);
                if (tabControl != null)
                {
                    tabControl.Background = actualBackgroundBrush;
                }
                
                // 直接更新每个控件的颜色
                if (ProjectPathTextBox != null)
                {
                    ProjectPathTextBox.Background = controlBackgroundBrush;
                    ProjectPathTextBox.Foreground = foregroundBrush;
                    ProjectPathTextBox.BorderBrush = borderBrush;
                }
                
                if (FontNameTextBox != null)
                {
                    FontNameTextBox.Background = controlBackgroundBrush;
                    FontNameTextBox.Foreground = foregroundBrush;
                    FontNameTextBox.BorderBrush = borderBrush;
                }
                
                if (ManualCharsTextBox != null)
                {
                    ManualCharsTextBox.Background = controlBackgroundBrush;
                    ManualCharsTextBox.Foreground = foregroundBrush;
                    ManualCharsTextBox.BorderBrush = borderBrush;
                }
                
                // 更新ComboBox控件的颜色
                UpdateComboBoxColors(FontSizeComboBox, controlBackgroundBrush, foregroundBrush, borderBrush);
                UpdateComboBoxColors(ModeComboBox, controlBackgroundBrush, foregroundBrush, borderBrush);
                UpdateComboBoxColors(CodeTypeComboBox, controlBackgroundBrush, foregroundBrush, borderBrush);
                UpdateComboBoxColors(BitOrderComboBox, controlBackgroundBrush, foregroundBrush, borderBrush);
                UpdateComboBoxColors(DuplicateHandlingComboBox, controlBackgroundBrush, foregroundBrush, borderBrush);
                
                if (GeneratedCharsListView != null)
                {
                    GeneratedCharsListView.Background = controlBackgroundBrush;
                    GeneratedCharsListView.Foreground = foregroundBrush;
                    GeneratedCharsListView.BorderBrush = borderBrush;
                    
                    // 更新ListView的滚动条
                    UpdateScrollBarColors(GeneratedCharsListView, controlBackgroundBrush, borderBrush, foregroundBrush);
                }
                
                // 更新标题栏颜色
                var headerBorder = this.FindName("HeaderBorder") as Border;
                if (headerBorder != null)
                {
                    headerBorder.Background = headerBackgroundBrush;
                    headerBorder.BorderBrush = borderBrush;
                }
                
                // 更新RadioButton控件
                UpdateRadioButtonColors(SearchRadio, foregroundBrush, primaryColor);
                UpdateRadioButtonColors(ManualRadio, foregroundBrush, primaryColor);
                UpdateRadioButtonColors(BothRadio, foregroundBrush, primaryColor);
                
                // 更新CheckBox控件
                UpdateCheckBoxColors(ClearExistingCheckBox, foregroundBrush, primaryColor);
                UpdateCheckBoxColors(RememberChoiceCheckBox, foregroundBrush, primaryColor);
                
                // 更新TabControl和TabItem
                UpdateTabControlColorsWithBackground(actualBackgroundBrush, controlBackgroundBrush, foregroundBrush, borderBrush);
                
                // 更新Button控件
                UpdateButtonColors(foregroundBrush, primaryColor, primaryDarkColor, primaryDarkerColor);
                
                // 更新所有TextBlock控件
                UpdateTextBlocks(this, foregroundBrush, textSecondaryColor, textTertiaryColor);
                
                // 同时更新资源字典，以便新创建的控件也能使用正确的颜色
                this.Resources["ControlBackgroundColor"] = controlBackgroundBrush;
                this.Resources["BorderColor"] = borderBrush;
                this.Resources["BackgroundColor"] = actualBackgroundBrush;
                this.Resources["BackgroundSecondaryColor"] = secondaryBackgroundBrush;
                this.Resources["TextColor"] = foregroundBrush;
                this.Resources["TextSecondaryColor"] = textSecondaryColor;
                this.Resources["TextTertiaryColor"] = textTertiaryColor;
                this.Resources["PrimaryColor"] = primaryColor;
                this.Resources["PrimaryDarkColor"] = primaryDarkColor;
                this.Resources["PrimaryDarkerColor"] = primaryDarkerColor;
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新控件颜色失败", ex);
            }
        }
        
        // 专门更新ComboBox颜色的方法
        private void UpdateComboBoxColors(System.Windows.Controls.ComboBox comboBox, System.Windows.Media.Brush backgroundBrush, System.Windows.Media.Brush foregroundBrush, System.Windows.Media.Brush borderBrush)
        {
            if (comboBox != null)
            {
                // 直接设置ComboBox的基本属性
                comboBox.Background = backgroundBrush;
                comboBox.Foreground = foregroundBrush;
                comboBox.BorderBrush = borderBrush;
                
                // 尝试更新ComboBox的内部视觉元素
                try
                {
                    // 遍历视觉树，更新所有相关元素
                    UpdateVisualTree(comboBox, backgroundBrush, foregroundBrush, borderBrush);
                }
                catch (Exception ex)
                {
                    _pluginApi.Error("更新ComboBox视觉元素失败", ex);
                }
                
                // 强制更新
                comboBox.InvalidateVisual();
                comboBox.UpdateLayout();
            }
        }
        
        // 递归更新视觉树中的元素
        private void UpdateVisualTree(DependencyObject element, System.Windows.Media.Brush backgroundBrush, System.Windows.Media.Brush foregroundBrush, System.Windows.Media.Brush borderBrush)
        {
            if (element == null) return;
            
            // 更新Border元素
            if (element is Border border)
            {
                border.Background = backgroundBrush;
                border.BorderBrush = borderBrush;
            }
            
            // 更新TextBlock元素
            if (element is TextBlock textBlock)
            {
                textBlock.Foreground = foregroundBrush;
            }
            
            // 更新ContentPresenter元素
            if (element is ContentPresenter contentPresenter)
            {
                // ContentPresenter本身没有Foreground属性，但可以尝试更新其内容
                if (contentPresenter.Content is TextBlock contentTextBlock)
                {
                    contentTextBlock.Foreground = foregroundBrush;
                }
            }
            
            // 更新ComboBoxItem元素
            if (element is System.Windows.Controls.ComboBoxItem comboBoxItem)
            {
                comboBoxItem.Background = backgroundBrush;
                comboBoxItem.Foreground = foregroundBrush;
                comboBoxItem.BorderBrush = borderBrush;
            }
            
            // 递归处理子元素
            int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(element, i);
                UpdateVisualTree(child, backgroundBrush, foregroundBrush, borderBrush);
            }
        }
        
        // 更新RadioButton颜色
        private void UpdateRadioButtonColors(System.Windows.Controls.RadioButton radioButton, System.Windows.Media.Brush foregroundBrush, System.Windows.Media.Brush primaryColor)
        {
            if (radioButton != null)
            {
                radioButton.Foreground = foregroundBrush;
                // 尝试更新内部视觉元素
                UpdateVisualTree(radioButton, null, foregroundBrush, primaryColor);
            }
        }
        
        // 更新CheckBox颜色
        private void UpdateCheckBoxColors(System.Windows.Controls.CheckBox checkBox, System.Windows.Media.Brush foregroundBrush, System.Windows.Media.Brush primaryColor)
        {
            if (checkBox != null)
            {
                checkBox.Foreground = foregroundBrush;
                // 尝试更新内部视觉元素
                UpdateVisualTree(checkBox, null, foregroundBrush, primaryColor);
            }
        }
        
        // 更新TabControl颜色（带背景色参数）
        private void UpdateTabControlColorsWithBackground(System.Windows.Media.Brush backgroundBrush, System.Windows.Media.Brush controlBackgroundBrush, System.Windows.Media.Brush foregroundBrush, System.Windows.Media.Brush borderBrush)
        {
            try
            {
                // 查找TabControl（通过遍历视觉树）
                var tabControl = FindVisualChild<System.Windows.Controls.TabControl>(this);
                if (tabControl != null)
                {
                    tabControl.Background = backgroundBrush;
                    tabControl.Foreground = foregroundBrush;
                    tabControl.BorderBrush = borderBrush;
                    
                    // 更新TabItem控件
                    foreach (var tabItem in tabControl.Items)
                    {
                        if (tabItem is System.Windows.Controls.TabItem item)
                        {
                            item.Foreground = foregroundBrush;
                            // 更新TabItem的背景色
                            item.Background = controlBackgroundBrush;
                        }
                    }
                    
                    // 更新TabControl内部的Grid背景色
                    var tabControlGrid = FindVisualChild<System.Windows.Controls.Grid>(tabControl);
                    if (tabControlGrid != null)
                    {
                        tabControlGrid.Background = backgroundBrush;
                    }
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新TabControl颜色失败", ex);
            }
        }
        
        // 更新TabControl颜色（旧方法，保持兼容）
        private void UpdateTabControlColors()
        {
            try
            {
                // 获取当前主题
                var currentTheme = _pluginApi.CurrentTheme;
                var backgroundBrush = _pluginApi.CurrentBackgroundBrush;
                var foregroundBrush = _pluginApi.CurrentForegroundBrush;
                
                // 根据主题类型设置颜色
                System.Windows.Media.Brush controlBackgroundBrush;
                System.Windows.Media.Brush borderBrush;
                
                switch (currentTheme)
                {
                    case ToolboxTheme.White:
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 195, 199));
                        break;
                    case ToolboxTheme.Black:
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 20));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40));
                        break;
                    case ToolboxTheme.LightBlack:
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
                        break;
                    case ToolboxTheme.Gray:
                        controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 230));
                        borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                        break;
                    default:
                        bool isDarkTheme = IsDarkBackground(backgroundBrush);
                        if (isDarkTheme)
                        {
                            controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
                            borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
                        }
                        else
                        {
                            controlBackgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                            borderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 195, 199));
                        }
                        break;
                }
                
                UpdateTabControlColorsWithBackground(backgroundBrush, controlBackgroundBrush, foregroundBrush, borderBrush);
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新TabControl颜色失败", ex);
            }
        }        
        // 更新Button颜色
        private void UpdateButtonColors(System.Windows.Media.Brush foregroundBrush, System.Windows.Media.Brush primaryColor, System.Windows.Media.Brush primaryDarkColor, System.Windows.Media.Brush primaryDarkerColor)
        {
            try
            {
                // 查找所有Button控件
                var buttons = FindVisualChildren<System.Windows.Controls.Button>(this);
                foreach (var button in buttons)
                {
                    button.Foreground = System.Windows.Media.Brushes.White;
                    button.Background = primaryColor;
                    button.BorderBrush = primaryDarkColor;
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新Button颜色失败", ex);
            }
        }
        
        // 更新滚动条颜色
        private void UpdateScrollBarColors(DependencyObject element, System.Windows.Media.Brush backgroundBrush, System.Windows.Media.Brush borderBrush, System.Windows.Media.Brush foregroundBrush)
        {
            try
            {
                var scrollBars = FindVisualChildren<System.Windows.Controls.Primitives.ScrollBar>(element);
                foreach (var scrollBar in scrollBars)
                {
                    scrollBar.Background = backgroundBrush;
                    scrollBar.BorderBrush = borderBrush;
                    scrollBar.Foreground = foregroundBrush;
                    
                    // 更新滚动条的滑块
                    var thumb = FindVisualChild<System.Windows.Controls.Primitives.Thumb>(scrollBar);
                    if (thumb != null)
                    {
                        thumb.Background = foregroundBrush;
                    }
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新滚动条颜色失败", ex);
            }
        }
        
        // 更新TextBlock控件颜色
        private void UpdateTextBlocks(DependencyObject element, System.Windows.Media.Brush textColor, System.Windows.Media.Brush textSecondaryColor, System.Windows.Media.Brush textTertiaryColor)
        {
            try
            {
                var textBlocks = FindVisualChildren<TextBlock>(element);
                foreach (var textBlock in textBlocks)
                {
                    // 根据TextBlock的样式或名称判断使用哪种文本颜色
                    if (textBlock.Name.Contains("Header") || textBlock.FontWeight == FontWeights.Bold)
                    {
                        textBlock.Foreground = textSecondaryColor;
                    }
                    else if (textBlock.Name.Contains("Label"))
                    {
                        textBlock.Foreground = textTertiaryColor;
                    }
                    else
                    {
                        textBlock.Foreground = textColor;
                    }
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新TextBlock颜色失败", ex);
            }
        }
        
        // 查找视觉树中的子元素
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                if (child is T t)
                {
                    yield return t;
                }
                
                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
        
        // 查找视觉树中的第一个子元素
        private static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;
            
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                if (child is T t)
                {
                    return t;
                }
                
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            
            return null;
        }
        
        // 基于背景色推断主题
        private ToolboxTheme InferThemeFromBackground(System.Windows.Media.Brush backgroundBrush)
        {
            try
            {
                // 获取API返回的主题
                var apiTheme = _pluginApi.CurrentTheme;
                
                // 首先检查背景色亮度，这是判断主题的最直接方法
                if (backgroundBrush is System.Windows.Media.SolidColorBrush solidBrush)
                {
                    System.Windows.Media.Color color = solidBrush.Color;
                    
                    // 计算亮度
                    double brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
                    
                    // 计算RGB值的差异，用于判断是否为灰色
                    int colorRDiff = Math.Abs(color.R - color.G);
                    int colorGDiff = Math.Abs(color.G - color.B);
                    int colorBDiff = Math.Abs(color.B - color.R);
                    bool colorIsGrayish = colorRDiff < 20 && colorGDiff < 20 && colorBDiff < 20;
                                        
                    // 特殊处理：如果背景色是 #FF1E1E1E（默认深色），
                    // 这说明 _pluginApi.CurrentBackgroundBrush 返回的是默认值，而不是实际的主题背景色
                    if (color.R == 30 && color.G == 30 && color.B == 30)
                    {

                        
                        // 尝试通过视觉树获取父容器的背景色
                        var parentWindow = Window.GetWindow(this);
                        if (parentWindow != null)
                        {
                            var parentBackground = parentWindow.Background;
                            if (parentBackground is System.Windows.Media.SolidColorBrush parentSolidBrush)
                            {
                                System.Windows.Media.Color parentColor = parentSolidBrush.Color;
                                double parentBrightness = (0.299 * parentColor.R + 0.587 * parentColor.G + 0.114 * parentColor.B) / 255;
                                
                                // 计算RGB值的差异，用于判断是否为灰色
                                int parentRDiff = Math.Abs(parentColor.R - parentColor.G);
                                int parentGDiff = Math.Abs(parentColor.G - parentColor.B);
                                int parentBDiff = Math.Abs(parentColor.B - parentColor.R);
                                bool parentIsGrayish = parentRDiff < 20 && parentGDiff < 20 && parentBDiff < 20;
                                

                                
                                // 根据父窗口背景色推断主题
                                    if (parentIsGrayish)
                                    {
                                        // 灰色调背景，根据亮度区分白色和灰色主题
                                        if (parentBrightness > 0.9)
                                        {

                                            return ToolboxTheme.White;
                                        }
                                        else if (parentBrightness > 0.5)
                                        {

                                            return ToolboxTheme.Gray;
                                        }
                                    }
                                    
                                    // 非灰色调背景，根据亮度区分
                                    if (parentBrightness > 0.7)
                                    {

                                        return ToolboxTheme.White;
                                    }
                                    else if (parentBrightness < 0.15)
                                    {

                                        return ToolboxTheme.Black;
                                    }
                                    else if (parentBrightness < 0.45)
                                    {

                                        return ToolboxTheme.LightBlack;
                                    }
                                    else if (parentBrightness < 0.5)
                                    {

                                        return ToolboxTheme.LightBlack;
                                    }
                                    else
                                    {

                                        return ToolboxTheme.White;
                                    }
                            }
                            else
                            {

                            }
                        }
                        else
                        {
    
                        }
                        
                        // 尝试获取父级容器的背景色
                        var parentControl = VisualTreeHelper.GetParent(this);
                        while (parentControl != null)
                        {
                            if (parentControl is System.Windows.Controls.Control control && control.Background != null)
                            {
                                if (control.Background is System.Windows.Media.SolidColorBrush parentSolidBrush)
                                {
                                    System.Windows.Media.Color parentColor = parentSolidBrush.Color;
                                    double parentBrightness = (0.299 * parentColor.R + 0.587 * parentColor.G + 0.114 * parentColor.B) / 255;
                                    
                                    // 计算RGB值的差异，用于判断是否为灰色
                                    int ctrlRDiff = Math.Abs(parentColor.R - parentColor.G);
                                    int ctrlGDiff = Math.Abs(parentColor.G - parentColor.B);
                                    int ctrlBDiff = Math.Abs(parentColor.B - parentColor.R);
                                    bool ctrlIsGrayish = ctrlRDiff < 20 && ctrlGDiff < 20 && ctrlBDiff < 20;
                                    

                                    
                                    // 根据父容器背景色推断主题
                                    if (ctrlIsGrayish)
                                    {
                                        // 灰色调背景，根据亮度区分白色和灰色主题
                                        if (parentBrightness > 0.9)
                                        {
    
                                            return ToolboxTheme.White;
                                        }
                                        else if (parentBrightness > 0.5)
                                        {
    
                                            return ToolboxTheme.Gray;
                                        }
                                    }
                                    
                                    // 非灰色调背景，根据亮度区分
                                    if (parentBrightness > 0.7)
                                    {

                                        return ToolboxTheme.White;
                                    }
                                    else if (parentBrightness < 0.15)
                                    {

                                        return ToolboxTheme.Black;
                                    }
                                    else if (parentBrightness < 0.45)
                                    {

                                        return ToolboxTheme.LightBlack;
                                    }
                                    else if (parentBrightness < 0.5)
                                    {

                                        return ToolboxTheme.LightBlack;
                                    }
                                    else
                                    {

                                        return ToolboxTheme.White;
                                    }
                                }
                                else
                                {
    
                                }
                            }
                            parentControl = VisualTreeHelper.GetParent(parentControl);
                        }
                        

                    }
                    else
                    {
                        // 根据背景色亮度和RGB值推断主题
                        if (colorIsGrayish)
                        {
                            // 灰色调背景，根据亮度区分白色和灰色主题
                            if (brightness > 0.9)
                            {

                                return ToolboxTheme.White;
                            }
                            else if (brightness > 0.5)
                            {

                                return ToolboxTheme.Gray;
                            }
                        }
                        
                        // 非灰色调背景，根据亮度区分
                        if (brightness > 0.7)
                        {

                            return ToolboxTheme.White;
                        }
                        else if (brightness < 0.15)
                        {

                            return ToolboxTheme.Black;
                        }
                        else if (brightness < 0.45)
                        {

                            return ToolboxTheme.LightBlack;
                        }
                        else if (brightness < 0.5)
                        {

                            return ToolboxTheme.LightBlack;
                        }
                        else
                        {

                            return ToolboxTheme.White;
                        }
                    }
                }
                else
                {
    
                }
                
                // 如果无法获取背景色，返回API主题

                return apiTheme;
            }
            catch (Exception ex)
            {
                _pluginApi.Error("推断主题失败", ex);
                return _pluginApi.CurrentTheme;
            }
        }        
        // 判断背景色是否为深色
        private bool IsDarkBackground(System.Windows.Media.Brush brush)
        {
            try
            {
                if (brush is System.Windows.Media.SolidColorBrush solidBrush)
                {
                    System.Windows.Media.Color color = solidBrush.Color;
                    // 计算亮度
                    double brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
                    return brightness < 0.5;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void InitializeUI()
        {
            try
            {
                ProjectPathTextBox.Text = _config.LastProjectPath;
                FontNameTextBox.Text = _config.FontName;
                
                // 安全设置ComboBox选中值
                foreach (ComboBoxItem item in FontSizeComboBox.Items)
                {
                    if (item.Content.ToString() == _config.FontSize.ToString())
                    {
                        FontSizeComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                foreach (ComboBoxItem item in ModeComboBox.Items)
                {
                    if (item.Content.ToString() == _config.Mode)
                    {
                        ModeComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                foreach (ComboBoxItem item in CodeTypeComboBox.Items)
                {
                    if (item.Content.ToString() == _config.CodeType)
                    {
                        CodeTypeComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                foreach (ComboBoxItem item in BitOrderComboBox.Items)
                {
                    if (item.Content.ToString() == _config.BitOrder)
                    {
                        BitOrderComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                foreach (ComboBoxItem item in DuplicateHandlingComboBox.Items)
                {
                    if (item.Content.ToString() == _config.DuplicateHandling)
                    {
                        DuplicateHandlingComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                ClearExistingCheckBox.IsChecked = _config.ClearExistingFonts;
                RememberChoiceCheckBox.IsChecked = _config.RememberChoice;
                
                // 初始化手动输入区域的可见性
                OnGenerateModeChanged(null, null);
                
                // 确保在UI初始化完成后更新主题颜色
                UpdateThemeColors();
            }
            catch (Exception ex)
            {
                _pluginApi.Error("初始化UI失败", ex);
                System.Windows.MessageBox.Show($"初始化UI失败: {ex.Message}");
            }
        }

        private PluginConfig LoadConfig()
        {
            try
            {
                return _pluginApi.GetConfig(new PluginConfig());
            }
            catch (Exception ex)
            {
                _pluginApi.Error("加载配置失败", ex);
                // 返回默认配置
                return new PluginConfig();
            }
        }

        private async Task SaveConfigAsync()
        {
            try
            {
                await _pluginApi.SaveConfigAsync(_config);
            }
            catch (Exception ex)
            {
                _pluginApi.Error("保存配置异步操作失败", ex);
                throw;
            }
        }

        private void OnBrowseProjectClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // 使用WPF文件夹选择对话框替代Windows API调用
                var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                folderDialog.Description = "选择项目目录";
                folderDialog.ShowNewFolderButton = true;
                
                // 设置默认路径
                if (!string.IsNullOrWhiteSpace(ProjectPathTextBox.Text))
                {
                    folderDialog.SelectedPath = ProjectPathTextBox.Text;
                }
                
                // 显示对话框
                var result = folderDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ProjectPathTextBox.Text = folderDialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("浏览项目路径失败", ex);
                System.Windows.MessageBox.Show($"浏览项目路径失败: {ex.Message}");
            }
        }

        private async void OnSaveConfigClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _config.FontName = FontNameTextBox.Text;
                _config.FontSize = int.Parse(((ComboBoxItem)FontSizeComboBox.SelectedItem).Content.ToString()!);
                _config.Mode = ((ComboBoxItem)ModeComboBox.SelectedItem).Content.ToString()!;
                _config.CodeType = ((ComboBoxItem)CodeTypeComboBox.SelectedItem).Content.ToString()!;
                _config.BitOrder = ((ComboBoxItem)BitOrderComboBox.SelectedItem).Content.ToString()!;
                _config.LastProjectPath = ProjectPathTextBox.Text;
                _config.ClearExistingFonts = ClearExistingCheckBox.IsChecked ?? false;
                _config.DuplicateHandling = ((ComboBoxItem)DuplicateHandlingComboBox.SelectedItem).Content.ToString()!;
                _config.RememberChoice = RememberChoiceCheckBox.IsChecked ?? false;
                
                await SaveConfigAsync();
                System.Windows.MessageBox.Show("配置保存成功");
            }
            catch (Exception ex)
            {
                _pluginApi.Error("保存配置失败", ex);
                System.Windows.MessageBox.Show($"保存配置失败: {ex.Message}");
            }
        }

        private async void OnGenerateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _config.FontName = FontNameTextBox.Text;
                _config.FontSize = int.Parse(((ComboBoxItem)FontSizeComboBox.SelectedItem).Content.ToString()!);
                _config.Mode = ((ComboBoxItem)ModeComboBox.SelectedItem).Content.ToString()!;
                _config.CodeType = ((ComboBoxItem)CodeTypeComboBox.SelectedItem).Content.ToString()!;
                _config.BitOrder = ((ComboBoxItem)BitOrderComboBox.SelectedItem).Content.ToString()!;
                _config.LastProjectPath = ProjectPathTextBox.Text;
                _config.ClearExistingFonts = ClearExistingCheckBox.IsChecked ?? false;
                _config.DuplicateHandling = ((ComboBoxItem)DuplicateHandlingComboBox.SelectedItem).Content.ToString()!;
                _config.RememberChoice = RememberChoiceCheckBox.IsChecked ?? false;
                
                await SaveConfigAsync();
                
                _chineseChars.Clear();
                _charMatrixData.Clear();
                
                string generateMode = SearchRadio.IsChecked == true ? "search" : ManualRadio.IsChecked == true ? "manual" : "both";
                string projectPath = ProjectPathTextBox.Text;
                
                if (string.IsNullOrWhiteSpace(projectPath))
                {
                    System.Windows.MessageBox.Show("请选择项目路径");
                    return;
                }
                
                if (generateMode == "search" || generateMode == "both")
                {
                    var searchDirs = new List<string> { Path.Combine(projectPath, "User"), Path.Combine(projectPath, "HardWare") };
                    SearchChineseInFiles(searchDirs);
                }
                
                if (generateMode == "manual" || generateMode == "both")
                {
                    string manualChars = ManualCharsTextBox.Text;
                    if (!string.IsNullOrWhiteSpace(manualChars))
                    {
                        ExtractChineseCharacters(manualChars);
                    }
                }
                
                if (_chineseChars.Count == 0)
                {
                    System.Windows.MessageBox.Show("未找到任何中文汉字");
                    return;
                }
                
                GenerateCharBitmaps();
                UpdateOledDataFile(projectPath);
                UpdateGeneratedCharsListView();
                
                System.Windows.MessageBox.Show($"字模生成成功，共生成 {_chineseChars.Count} 个汉字的字模数据");
            }
            catch (Exception ex)
            {
                _pluginApi.Error("字模生成失败", ex);
                System.Windows.MessageBox.Show($"字模生成失败: {ex.Message}");
            }
        }

        private void SearchChineseInFiles(List<string> searchDirs)
        {
            try
            {
                foreach (string searchDir in searchDirs)
                {
                    if (Directory.Exists(searchDir))
                    {
                        TraverseDirectory(searchDir, filePath =>
                        {
                            if (filePath.EndsWith(".c") || filePath.EndsWith(".h"))
                            {
                                try
                                {
                                    string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                                    ExtractChineseFromContent(content);
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        string content = File.ReadAllText(filePath, System.Text.Encoding.GetEncoding(936));
                                        ExtractChineseFromContent(content);
                                    }
                                    catch (Exception e)
                                    {
                                        _pluginApi.Error($"读取文件 {filePath} 失败", e);
                                    }
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("搜索中文失败", ex);
            }
        }

        private void TraverseDirectory(string dir, Action<string> callback)
        {
            if (!Directory.Exists(dir)) return;
            
            try
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    callback(file);
                }
                foreach (string subDir in Directory.GetDirectories(dir))
                {
                    TraverseDirectory(subDir, callback);
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error($"读取目录 {dir} 失败", ex);
            }
        }

        private void ExtractChineseFromContent(string content)
        {
            try
            {
                var printfMatches = Regex.Matches(content, @"OLED_Printf\([^;]*?\);");
                foreach (Match match in printfMatches)
                {
                    string printfCall = match.Value;
                    var strMatch = Regex.Match(printfCall, @"""([^""]*)""");
                    if (strMatch.Success)
                    {
                        ExtractChineseCharacters(strMatch.Groups[1].Value);
                    }
                    
                    var conditionMatches = Regex.Matches(printfCall, @"\?""([^""?]*)""\s*:\s*""([^""]*)""");
                    foreach (Match condMatch in conditionMatches)
                    {
                        if (condMatch.Groups.Count >= 3)
                        {
                            ExtractChineseCharacters(condMatch.Groups[1].Value + condMatch.Groups[2].Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("从内容中提取中文失败", ex);
            }
        }

        private void ExtractChineseCharacters(string input)
        {
            try
            {
                var matches = Regex.Matches(input, @"[一-龥℃°℉]");
                foreach (Match match in matches)
                {
                    _chineseChars.Add(match.Value[0]);
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("提取中文字符失败", ex);
            }
        }

        private void GenerateCharBitmaps()
        {
            try
            {
                foreach (char c in _chineseChars)
                {
                    _charMatrixData.Add(c, GenerateCharBitmap(c));
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("生成字模位图失败", ex);
                throw;
            }
        }

        private byte[] GenerateCharBitmap(char c)
        {
            try
            {
                int size = _config.FontSize;
                List<byte> bitmapData = new List<byte>();
                
                // 创建DrawingVisual
                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext ctx = visual.RenderOpen())
                {
                    ctx.DrawRectangle(System.Windows.Media.Brushes.White, null, new Rect(0, 0, size, size));
                    Typeface typeface = new Typeface(new System.Windows.Media.FontFamily(_config.FontName), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                    FormattedText formattedText = new FormattedText(
                        c.ToString(),
                        System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight,
                        typeface,
                        size,
                        System.Windows.Media.Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip
                    );
                    double textX = (size - formattedText.Width) / 2;
                    double textY = (size - formattedText.Height) / 2;
                    ctx.DrawText(formattedText, new System.Windows.Point(textX, textY));
                }
                
                RenderTargetBitmap bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                
                byte[] pixelData = new byte[size * size * 4];
                bitmap.CopyPixels(pixelData, size * 4, 0);
                
                if (_config.Mode == "列行式")
                {
                    for (int col = 0; col < size; col++)
                    {
                        for (int rowBlock = 0; rowBlock < size; rowBlock += 8)
                        {
                            byte b = 0;
                            for (int bit = 0; bit < 8; bit++)
                            {
                                int row = rowBlock + bit;
                                if (row < size)
                                {
                                    int pixelIndex = (row * size + col) * 4;
                                    bool isBlack = pixelData[pixelIndex] < 128;
                                    if (_config.CodeType == "阴码") isBlack = !isBlack;
                                    if (isBlack)
                                    {
                                        b |= (byte)(_config.BitOrder == "低位在前" ? 1 << bit : 1 << (7 - bit));
                                    }
                                }
                            }
                            bitmapData.Add(b);
                        }
                    }
                }
                else
                {
                    for (int row = 0; row < size; row += 8)
                    {
                        for (int col = 0; col < size; col++)
                        {
                            byte b = 0;
                            for (int bit = 0; bit < 8; bit++)
                            {
                                int currentRow = row + bit;
                                if (currentRow < size)
                                {
                                    int pixelIndex = (currentRow * size + col) * 4;
                                    bool isBlack = pixelData[pixelIndex] < 128;
                                    if (_config.CodeType == "阴码") isBlack = !isBlack;
                                    if (isBlack)
                                    {
                                        b |= (byte)(_config.BitOrder == "低位在前" ? 1 << bit : 1 << (7 - bit));
                                    }
                                }
                            }
                            bitmapData.Add(b);
                        }
                    }
                }
                
                return bitmapData.ToArray();
            }
            catch (Exception ex)
            {
                _pluginApi.Error($"生成字符 '{c}' 的位图失败", ex);
                throw;
            }
        }

        private void UpdateOledDataFile(string projectDir)
        {
            try
            {
                string oledDataFile = Path.Combine(projectDir, "HardWare", "OLED_Data.c");
                if (!File.Exists(oledDataFile))
                {
                    throw new FileNotFoundException($"文件不存在: {oledDataFile}");
                }
                
                string content = File.ReadAllText(oledDataFile, System.Text.Encoding.UTF8);
                
                // 查找开始位置，支持多种格式
                string[] patterns = {
                    "const ChineseCell_t OLED_CF16x16[] = {",
                    "const uint8_t OLED_CF16x16[] = {",
                    "uint8_t OLED_CF16x16[] = {",
                    "static const uint8_t OLED_CF16x16[] = {"
                };
                
                int startPos = -1;
                string matchedPattern = string.Empty;
                foreach (string pattern in patterns)
                {
                    startPos = content.IndexOf(pattern);
                    if (startPos != -1)
                    {
                        matchedPattern = pattern;
                        break;
                    }
                }
                
                if (startPos == -1)
                {
                    _pluginApi.Warn("未找到中文字库定义，无法更新");
                    return;
                }
                
                // 查找结束位置
                int endPos = content.IndexOf("};\n", startPos);
                if (endPos == -1)
                {
                    endPos = content.IndexOf("};", startPos);
                    if (endPos == -1)
                    {
                        _pluginApi.Warn("未找到中文字库结束标记，无法更新");
                        return;
                    }
                }
                endPos += 2; // 包含"};"
                
                // 生成新的字模数据
                List<string> fontData = new List<string>();
                foreach (char c in _chineseChars)
                {
                    string charStr = c.ToString();
                    byte[] bitmapData = _charMatrixData[c];
                    string dataStr = string.Join(", ", bitmapData.Select(b => $"0x{b:X2}"));
                    fontData.Add($"    {{\"{charStr}\", {{ {dataStr} }}, 16, 16}},");
                }
                
                // 添加默认图形和结束标志
                fontData.Add("    {\"\\0\", {0xFF,0x01,0x01,0x01,0x31,0x09,0x09,0x09,0x09,0x89,0x71,0x01,0x01,0x01,0x01,0xFF, 0xFF,0x80,0x80,0x80,0x80,0x80,0x80,0x96,0x81,0x80,0x80,0x80,0x80,0x80,0x80,0xFF}, 16, 16},");
                fontData.Add("    {NULL, {0}, 0, 0} // 结束标志");
                fontData.Add("};");
                
                string newFontContent = "\n" + string.Join("\n", fontData) + "\n";
                // 使用实际匹配的模式长度，而不是硬编码的偏移量
                if (matchedPattern != null)
                {
                    string newContent = content.Substring(0, startPos + matchedPattern.Length) + newFontContent + content.Substring(endPos);
                    File.WriteAllText(oledDataFile, newContent, System.Text.Encoding.UTF8);
                }
                else
                {
                    _pluginApi.Error("未找到匹配的模式，无法更新OLED数据文件");
                }
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新OLED数据文件失败", ex);
                throw; // 重新抛出异常，让调用者处理
            }
        }

        private void UpdateGeneratedCharsListView()
        {
            GeneratedCharsListView.Items.Clear();
            foreach (char c in _chineseChars)
            {
                GeneratedCharsListView.Items.Add(new { Char = c, FontName = _config.FontName, FontSize = _config.FontSize });
            }
        }

        public bool UpdateOledImageDataFile(string projectPath, byte[] imageData, string arrayName, int width, int height, bool overwrite = false)
        {
            try
            {
                string[] possiblePaths = {
                    Path.Combine(projectPath, "HardWare", "OLED_Data.c"),
                    Path.Combine(projectPath, "OLED_Data.c"),
                    Path.Combine(projectPath, "hardware", "OLED_Data.c")
                };
                
                string? oledDataFile = possiblePaths.FirstOrDefault(File.Exists);
                if (oledDataFile == null)
                {
                    _pluginApi.Error($"未找到OLED_Data.c文件");
                    return false;
                }
                
                string content = File.ReadAllText(oledDataFile, System.Text.Encoding.UTF8);
                var arrayPattern = new Regex($@"const\s+uint8_t\s+{arrayName}\s*\[\]\s*=\s*\{{", RegexOptions.IgnoreCase);
                bool arrayExists = arrayPattern.IsMatch(content);
                
                if (arrayExists && !overwrite)
                {
                    _pluginApi.Warn($"数组 {arrayName} 已存在");
                    return false;
                }
                
                if (arrayExists && overwrite)
                {
                    var fullArrayPattern = new Regex($@"const\s+uint8_t\s+{arrayName}\s*\[\]\s*=\s*\{{([\s\S]*?)\}};", RegexOptions.IgnoreCase);
                    content = fullArrayPattern.Replace(content, string.Empty);
                }
                
                List<string> imageDataLines = new List<string>();
                imageDataLines.Add($"\n\nconst uint8_t {arrayName}[] = {{");
                
                string line = "\t";
                for (int i = 0; i < imageData.Length; i++)
                {
                    line += $"0X{imageData[i]:X2}";
                    if (i < imageData.Length - 1)
                    {
                        line += ",";
                        if ((i + 1) % 8 == 0)
                        {
                            imageDataLines.Add(line);
                            line = "\t";
                        }
                        else
                        {
                            line += " ";
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(line.Trim()))
                {
                    imageDataLines.Add(line);
                }
                imageDataLines.Add("};");
                
                string newImageContent = string.Join("\n", imageDataLines);
                string newContent = content + newImageContent;
                File.WriteAllText(oledDataFile, newContent, System.Text.Encoding.UTF8);
                
                return true;
            }
            catch (Exception ex)
            {
                _pluginApi.Error("更新图模数据失败", ex);
                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                // 清理资源
                _chineseChars.Clear();
                _charMatrixData.Clear();
                // 可以在这里添加其他需要清理的资源
            }
            catch (Exception ex)
            {
                _pluginApi.Error("释放资源失败", ex);
            }
        }
    }
}