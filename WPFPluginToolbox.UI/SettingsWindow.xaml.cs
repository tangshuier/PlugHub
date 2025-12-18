using System.Windows;
using System.Windows.Controls;
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
            // 通知主窗口应用主题
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ApplyTheme(theme);
            }
        }
    }
}