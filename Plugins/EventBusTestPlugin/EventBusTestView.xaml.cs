using System.Windows;using System.Windows.Controls;using WPFPluginToolbox.Core;
using WPFPluginToolbox.Services;

namespace EventBusTestPlugin
{
    // 自定义事件类
    public class CustomEvent
    {
        public string Content { get; set; }
        public string Source { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public partial class EventBusTestView : UserControl
    {
        private readonly IPluginAPI _pluginApi;
        private bool _isSubscribed = false;
        private Action<CustomEvent>? _customEventHandler;
        private Action<PluginErrorEvent>? _errorEventHandler;
        private Action<ConfigChangedEvent>? _configChangedEventHandler;

        public EventBusTestView(IPluginAPI pluginApi)
        {
            InitializeComponent();
            _pluginApi = pluginApi;
            Log("事件总线测试插件视图初始化完成");
        }

        private void Log(string message)
        {
            EventLogTextBlock.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            _pluginApi.Debug(message);
        }

        private void PublishButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = EventTypeComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                Log("请选择事件类型！");
                return;
            }

            var eventType = selectedItem.Tag.ToString();
            var content = EventContentTextBox.Text.Trim();

            try
            {
                switch (eventType)
                {
                    case "CustomEvent":
                        var customEvent = new CustomEvent
                        {
                            Content = content,
                            Source = _pluginApi.PluginName,
                            Timestamp = DateTime.Now
                        };
                        _pluginApi.PublishEvent(customEvent);
                        Log($"发布自定义事件成功 - 内容: {content}");
                        break;

                    case "ErrorEvent":
                        var errorEvent = new PluginErrorEvent(
                            _pluginApi.PluginId,
                            content,
                            new Exception("测试异常")
                        );
                        _pluginApi.PublishEvent(errorEvent);
                        Log($"发布错误事件成功 - 内容: {content}");
                        break;

                    case "ConfigChangedEvent":
                        var configEvent = new ConfigChangedEvent(
                            _pluginApi.PluginId,
                            "TestConfigChange"
                        );
                        _pluginApi.PublishEvent(configEvent);
                        Log($"发布配置变更事件成功 - 类型: TestConfigChange");
                        break;

                    default:
                        Log($"未知事件类型: {eventType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"发布事件失败 - 错误: {ex.Message}");
            }
        }

        private void SubscribeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isSubscribed)
            {
                Log("已经订阅了事件！");
                return;
            }

            try
            {
                // 订阅自定义事件
                _customEventHandler = (e) =>
                {
                    Log($"收到自定义事件 - 来源: {e.Source}, 内容: {e.Content}, 时间: {e.Timestamp}");
                };
                _pluginApi.SubscribeEvent(_customEventHandler);

                // 订阅错误事件
                _errorEventHandler = (e) =>
                {
                    Log($"收到错误事件 - 插件: {e.PluginId}, 内容: {e.Message}");
                };
                _pluginApi.SubscribeEvent(_errorEventHandler);

                // 订阅配置变更事件
                _configChangedEventHandler = (e) =>
                {
                    Log($"收到配置变更事件 - 插件: {e.PluginId}, 类型: {e.ChangeType}");
                };
                _pluginApi.SubscribeEvent(_configChangedEventHandler);

                _isSubscribed = true;
                Log("订阅所有事件成功！");
            }
            catch (Exception ex)
            {
                Log($"订阅事件失败 - 错误: {ex.Message}");
            }
        }

        private void UnsubscribeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSubscribed)
            {
                Log("还没有订阅事件！");
                return;
            }

            try
            {
                // 取消订阅所有事件
                if (_customEventHandler != null)
                {
                    _pluginApi.UnsubscribeEvent(_customEventHandler);
                    _customEventHandler = null;
                }

                if (_errorEventHandler != null)
                {
                    _pluginApi.UnsubscribeEvent(_errorEventHandler);
                    _errorEventHandler = null;
                }

                if (_configChangedEventHandler != null)
                {
                    _pluginApi.UnsubscribeEvent(_configChangedEventHandler);
                    _configChangedEventHandler = null;
                }

                _isSubscribed = false;
                Log("取消订阅所有事件成功！");
            }
            catch (Exception ex)
            {
                Log($"取消订阅事件失败 - 错误: {ex.Message}");
            }
        }
    }
}