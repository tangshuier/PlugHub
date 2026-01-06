using System.Windows.Controls;
using WPFPluginToolbox.Core;

namespace EventBusTestPlugin
{
    public class EventBusTestPlugin : IPlugin
    {
        private IPluginAPI _pluginApi;
        private EventBusTestView _view;

        public string Id => "EventBusTestPlugin";
        public string Name => "事件总线测试插件";
        public string Description => "测试插件间事件通信功能";
        public string Version => "1.0.0";
        public PluginType Type => PluginType.Plugin;

        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            _view = new EventBusTestView(_pluginApi);
            
            _pluginApi.Debug($"{Name} 初始化完成");
        }

        public void Activate()
        {
            _pluginApi.Info($"{Name} 激活");
        }

        public void Deactivate()
        {
            _pluginApi.Info($"{Name} 停用");
        }

        public UserControl GetMainView()
        {
            return _view;
        }

        public void Dispose()
        {
            _pluginApi.Info($"{Name} 释放资源");
        }
    }
}