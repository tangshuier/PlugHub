using System.Windows.Controls;
using WPFPluginToolbox.Core;

namespace PerformanceTestPlugin
{
    public class PerformanceTestPlugin : IPlugin
    {
        private IPluginAPI _pluginApi;
        private PerformanceTestView _view;

        public string Id => "PerformanceTestPlugin";
        public string Name => "性能监控测试插件";
        public string Description => "测试插件性能监控功能";
        public string Version => "1.0.0";
        public PluginType Type => PluginType.Plugin;

        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            _view = new PerformanceTestView(_pluginApi);
            
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