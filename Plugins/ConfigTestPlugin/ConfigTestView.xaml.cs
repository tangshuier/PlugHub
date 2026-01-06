using System;
using System.Windows;
using System.Windows.Controls;

namespace ConfigTestPlugin
{
    /// <summary>
    /// ConfigTestView.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigTestView : UserControl
    {
        private readonly ConfigTestPlugin _plugin;

        public ConfigTestView(ConfigTestPlugin plugin)
        {
            InitializeComponent();
            _plugin = plugin;
            
            // 加载初始配置
            LoadConfig();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfig()
        {
            var config = _plugin.GetConfig();
            
            // 设置选中的选项
            if (config.SelectedOptionIndex >= 0 && config.SelectedOptionIndex < OptionComboBox.Items.Count)
            {
                OptionComboBox.SelectedIndex = config.SelectedOptionIndex;
            }
            
            // 更新配置信息显示
            UpdateConfigInfo(config);
        }

        /// <summary>
        /// 更新配置信息显示
        /// </summary>
        /// <param name="config">配置对象</param>
        private void UpdateConfigInfo(Config config)
        {
            ConfigInfo.Text = $"选中的选项索引: {config.SelectedOptionIndex}\n" +
                             $"示例字符串: {config.ExampleString}\n" +
                             $"示例数值: {config.ExampleNumber}";
        }

        /// <summary>
        /// 选项选择变化事件
        /// </summary>
        private void OptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OptionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var config = _plugin.GetConfig();
                config.SelectedOptionIndex = OptionComboBox.SelectedIndex;
                UpdateConfigInfo(config);
            }
        }

        /// <summary>
        /// 保存配置按钮点击事件
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _plugin.SaveConfig();
            
            // 显示保存成功消息
            MessageBox.Show("配置已成功保存！下次打开插件时会记住当前选择。", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}