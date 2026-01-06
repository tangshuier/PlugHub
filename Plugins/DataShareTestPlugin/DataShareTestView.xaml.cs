using System.Windows;using System.Windows.Controls;using WPFPluginToolbox.Core;

namespace DataShareTestPlugin
{
    public partial class DataShareTestView : UserControl
    {
        private readonly IPluginAPI _pluginApi;

        public DataShareTestView(IPluginAPI pluginApi)
        {
            InitializeComponent();
            _pluginApi = pluginApi;
            Log("数据共享测试插件视图初始化完成");
        }

        private void Log(string message)
        {
            LogTextBlock.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            _pluginApi.Debug(message);
        }

        private void StoreButton_Click(object sender, RoutedEventArgs e)
        {
            var key = KeyTextBox.Text.Trim();
            var value = ValueTextBox.Text.Trim();

            if (string.IsNullOrEmpty(key))
            {
                ResultTextBlock.Text = "数据键名不能为空！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            try
            {
                _pluginApi.ShareData(key, value);
                ResultTextBlock.Text = "数据存储成功！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                Log($"存储数据成功 - 键: {key}, 值: {value}");
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = "数据存储失败！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                Log($"存储数据失败 - 错误: {ex.Message}");
            }
        }

        private void RetrieveButton_Click(object sender, RoutedEventArgs e)
        {
            var key = KeyTextBox.Text.Trim();

            if (string.IsNullOrEmpty(key))
            {
                ResultTextBlock.Text = "数据键名不能为空！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            try
            {
                var value = _pluginApi.GetSharedData<string>(key);
                if (value != null)
                {
                    ResultTextBlock.Text = $"获取成功: {value}";
                    ResultTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                    ValueTextBox.Text = value;
                    Log($"获取数据成功 - 键: {key}, 值: {value}");
                }
                else
                {
                    ResultTextBlock.Text = "未找到对应数据！";
                    ResultTextBlock.Foreground = System.Windows.Media.Brushes.Orange;
                    Log($"获取数据失败 - 键: {key} 不存在");
                }
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = "数据获取失败！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                Log($"获取数据失败 - 错误: {ex.Message}");
            }
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            var key = KeyTextBox.Text.Trim();

            if (string.IsNullOrEmpty(key))
            {
                ResultTextBlock.Text = "数据键名不能为空！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            try
            {
                var exists = _pluginApi.HasSharedData(key);
                ResultTextBlock.Text = exists ? "数据存在！" : "数据不存在！";
                ResultTextBlock.Foreground = exists ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Orange;
                Log($"检查数据 - 键: {key}, 结果: {(exists ? "存在" : "不存在")}");
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = "检查数据失败！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                Log($"检查数据失败 - 错误: {ex.Message}");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var key = KeyTextBox.Text.Trim();

            if (string.IsNullOrEmpty(key))
            {
                ResultTextBlock.Text = "数据键名不能为空！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            try
            {
                _pluginApi.RemoveSharedData(key);
                ResultTextBlock.Text = "数据删除成功！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                Log($"删除数据成功 - 键: {key}");
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = "数据删除失败！";
                ResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                Log($"删除数据失败 - 错误: {ex.Message}");
            }
        }
    }
}