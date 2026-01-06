using System.Windows;
using System.Windows.Controls;
using WPFPluginToolbox.Core;
using WPFPluginToolbox.Services;
using WPFPluginToolbox.Services.Models;

namespace WPFPluginToolbox.UI
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : UserControl
    {
        private ToolboxSettings _settings;
        private ToolboxSettings _originalSettings;
        private readonly SettingsService _settingsService;
        private ThemeService? _themeService;
        
        /// <summary>
        /// 设置是否有变更
        /// </summary>
        public bool HasChanges { get; private set; }
        
        public SettingsWindow()
        {
            InitializeComponent();
            
            // 初始化设置服务
            _settingsService = new SettingsService();
            
            // 初始化设置对象
            _settings = new ToolboxSettings();
            
            // 加载当前设置
            LoadSettings();
            
            // 保存原始设置用于比较
            _originalSettings = _settingsService.GetSettings();
            
            // 添加设置变更事件处理
            AddSettingsChangeHandlers();
            
            // 从主窗口获取主题服务实例
            GetThemeServiceFromMainWindow();
            
            // 应用当前主题到设置界面
            ApplyThemeToSettings(_settings.Theme);
        }
        
        /// <summary>
        /// 从主窗口获取主题服务实例
        /// </summary>
        private void GetThemeServiceFromMainWindow()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // 直接使用主窗口的主题服务，而不是创建新实例
                _themeService = (ThemeService)mainWindow.GetThemeService();
                
                // 订阅主题变化事件
                _themeService.ThemeChanged += OnThemeChanged;
            }
        }
        
        /// <summary>
        /// 主题变化事件处理
        /// </summary>
        private void OnThemeChanged(object? sender, ToolboxTheme theme)
        {
            // 实时更新设置页面主题
            ApplyThemeToSettings(theme);
        }
        
        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            _settings = _settingsService.GetSettings();
            
            // 设置主题选择
            switch (_settings.Theme)
            {
                case ToolboxTheme.Black:
                    BlackThemeRadio.IsChecked = true;
                    break;
                case ToolboxTheme.White:
                    WhiteThemeRadio.IsChecked = true;
                    break;
                case ToolboxTheme.LightBlack:
                    LightBlackThemeRadio.IsChecked = true;
                    break;
                case ToolboxTheme.Gray:
                    GrayThemeRadio.IsChecked = true;
                    break;
            }
            
            // 设置调试窗口默认打开选项
            DebugWindowDefaultOpenCheck.IsChecked = _settings.IsDebugWindowDefaultOpen;
            
            // 重置变更标志
            HasChanges = false;
        }
        
        /// <summary>
        /// 恢复原始设置
        /// </summary>
        public void RevertSettings()
        {
            // 恢复原始主题设置
            _settings = _originalSettings;
            
            // 恢复主题选择
            switch (_settings.Theme)
            {
                case ToolboxTheme.Black:
                    BlackThemeRadio.IsChecked = true;
                    break;
                case ToolboxTheme.White:
                    WhiteThemeRadio.IsChecked = true;
                    break;
                case ToolboxTheme.LightBlack:
                    LightBlackThemeRadio.IsChecked = true;
                    break;
                case ToolboxTheme.Gray:
                    GrayThemeRadio.IsChecked = true;
                    break;
            }
            
            // 恢复调试窗口默认打开选项
            DebugWindowDefaultOpenCheck.IsChecked = _settings.IsDebugWindowDefaultOpen;
            
            // 应用原始主题
            ApplyTheme(_settings.Theme);
            
            // 重置变更标志
            HasChanges = false;
        }
        
        /// <summary>
        /// 添加设置变更事件处理
        /// </summary>
        private void AddSettingsChangeHandlers()
        {
            // 主题选择变更事件
            BlackThemeRadio.Checked += SettingsChanged;
            WhiteThemeRadio.Checked += SettingsChanged;
            LightBlackThemeRadio.Checked += SettingsChanged;
            GrayThemeRadio.Checked += SettingsChanged;
            
            // 调试窗口设置变更事件
            DebugWindowDefaultOpenCheck.Checked += SettingsChanged;
            DebugWindowDefaultOpenCheck.Unchecked += SettingsChanged;
        }
        
        /// <summary>
        /// 设置变更处理
        /// </summary>
        private void SettingsChanged(object sender, RoutedEventArgs e)
        {
            HasChanges = true;
            
            // 如果是主题选择变更，立即应用主题进行预览，但不保存到设置文件
            if (sender is RadioButton)
            {
                // 获取当前选中的主题
                ToolboxTheme selectedTheme = _settings.Theme; // 默认使用当前设置的主题
                
                if (BlackThemeRadio.IsChecked == true)
                    selectedTheme = ToolboxTheme.Black;
                else if (WhiteThemeRadio.IsChecked == true)
                    selectedTheme = ToolboxTheme.White;
                else if (LightBlackThemeRadio.IsChecked == true)
                    selectedTheme = ToolboxTheme.LightBlack;
                else if (GrayThemeRadio.IsChecked == true)
                    selectedTheme = ToolboxTheme.Gray;
                
                // 立即应用主题进行预览
                ApplyTheme(selectedTheme);
            }
        }
        
        /// <summary>
        /// 保存设置
        /// </summary>
        public void SaveSettings()
        {
            // 保存主题设置
            if (BlackThemeRadio.IsChecked == true)
            {
                _settings.Theme = ToolboxTheme.Black;
            }
            else if (WhiteThemeRadio.IsChecked == true)
            {
                _settings.Theme = ToolboxTheme.White;
            }
            else if (LightBlackThemeRadio.IsChecked == true)
            {
                _settings.Theme = ToolboxTheme.LightBlack;
            }
            else if (GrayThemeRadio.IsChecked == true)
            {
                _settings.Theme = ToolboxTheme.Gray;
            }
            
            // 保存调试窗口设置
            _settings.IsDebugWindowDefaultOpen = DebugWindowDefaultOpenCheck.IsChecked == true;
            
            // 保存设置到文件
            _settingsService.SaveSettings(_settings);
            
            // 应用主题
            ApplyTheme(_settings.Theme);
            
            // 重置变更标志
            HasChanges = false;
        }
        
        /// <summary>
        /// 保存按钮点击事件
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
        
        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // 可以添加事件通知父窗口取消设置
        }
        
        /// <summary>
        /// 应用主题
        /// </summary>
        private void ApplyTheme(ToolboxTheme theme)
        {
            // 通知主窗口应用主题，设置为预览模式，不保存主题到设置文件
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ApplyTheme(theme, true);
            }
            
            // 同时应用主题到设置界面
            ApplyThemeToSettings(theme);
        }
        
        /// <summary>
        /// 将主题应用到设置界面
        /// </summary>
        private void ApplyThemeToSettings(ToolboxTheme theme)
        {
            // 检查_themeService是否为null
            if (_themeService == null)
                return;
            
            // 更新设置界面的背景和前景色
            this.Background = _themeService.MainBackgroundBrush;
            this.Foreground = _themeService.MainForegroundBrush;
            
            if (this.Content is Grid mainGrid)
            {
                mainGrid.Background = _themeService.MainBackgroundBrush;
                
                // 遍历主网格的所有子元素
                for (int i = 0; i < mainGrid.Children.Count; i++)
                {
                    var child = mainGrid.Children[i];
                    
                    if (child is Border border)
                    {
                        // 根据Grid.Row值区分不同区域
                        int rowIndex = Grid.GetRow(border);
                        
                        if (rowIndex == 0) // 标题区域边框 (Grid.Row="0")
                        {
                            // 更新标题区域
                            border.Background = _themeService.ToolBarBackgroundBrush;
                            border.BorderBrush = _themeService.BorderBrush;
                            
                            if (border.Child is Grid titleGrid)
                            {
                                UpdateTitleGrid(titleGrid);
                            }
                        }
                        else if (rowIndex == 1) // 内容区域边框 (Grid.Row="1")
                        {
                            // 更新内容区域
                            border.Background = _themeService.MainBackgroundBrush;
                            
                            // 更新边框内的元素
                            UpdateBorderContent(border);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 更新标题区域的网格
        /// </summary>
        private void UpdateTitleGrid(Grid titleGrid)
        {
            foreach (var titleChild in titleGrid.Children)
            {
                if (titleChild is TextBlock textBlock)
                {
                    // 更新标题文本颜色
                    textBlock.Foreground = _themeService.MainForegroundBrush;
                }
                else if (titleChild is Button button)
                {
                    // 更新保存按钮样式
                    button.Background = _themeService.ToolBarBackgroundBrush;
                    button.Foreground = _themeService.MainForegroundBrush;
                    button.BorderBrush = _themeService.BorderBrush;
                }
            }
        }
        
        /// <summary>
        /// 更新边框内的元素主题
        /// </summary>
        private void UpdateBorderContent(Border border)
        {
            if (border.Child is StackPanel stackPanel)
            {
                // 遍历StackPanel中的所有子元素
                for (int i = 0; i < stackPanel.Children.Count; i++)
                {
                    var item = stackPanel.Children[i];
                    
                    if (item is Border cardBorder)
                    {
                        // 更新卡片样式
                        cardBorder.Background = _themeService.PluginPanelBackgroundBrush;
                        cardBorder.BorderBrush = _themeService.BorderBrush;
                        
                        // 更新卡片内的元素
                        if (cardBorder.Child is StackPanel cardStack)
                        {
                            UpdateCardContent(cardStack);
                        }
                    }
                    else if (item is TextBlock textBlock)
                    {
                        // 更新文本颜色
                        textBlock.Foreground = _themeService.MainForegroundBrush;
                    }
                }
            }
        }
        
        /// <summary>
        /// 更新卡片内的元素主题
        /// </summary>
        private void UpdateCardContent(StackPanel cardStack)
        {
            // 遍历StackPanel中的所有子元素
            for (int i = 0; i < cardStack.Children.Count; i++)
            {
                var cardItem = cardStack.Children[i];
                
                if (cardItem is TextBlock textBlock)
                {
                    // 更新卡片标题颜色
                    textBlock.Foreground = _themeService.MainForegroundBrush;
                }
                else if (cardItem is StackPanel optionStack)
                {
                    // 更新选项内容
                    for (int j = 0; j < optionStack.Children.Count; j++)
                    {
                        var option = optionStack.Children[j];
                        
                        if (option is RadioButton radioButton)
                        {
                            // 更新单选按钮颜色
                            radioButton.Foreground = _themeService.MainForegroundBrush;
                        }
                        else if (option is CheckBox checkBox)
                        {
                            // 更新复选框颜色
                            checkBox.Foreground = _themeService.MainForegroundBrush;
                        }
                    }
                }
                // 直接处理RadioButton和CheckBox，不依赖于嵌套的StackPanel
                else if (cardItem is RadioButton radioButton)
                {
                    radioButton.Foreground = _themeService.MainForegroundBrush;
                }
                else if (cardItem is CheckBox checkBox)
                {
                    checkBox.Foreground = _themeService.MainForegroundBrush;
                }
            }
        }
    }
}