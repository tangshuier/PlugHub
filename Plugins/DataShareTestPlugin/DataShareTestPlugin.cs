using System.Windows.Controls;
using WPFPluginToolbox.Core;

namespace DataShareTestPlugin
{
    public class DataShareTestPlugin : IPlugin
    {
        private IPluginAPI _pluginApi;
        private DataShareTestView _view;

        public string Id => "DataShareTestPlugin";
        public string Name => "数据共享测试插件";
        public string Description => "测试插件间数据共享功能";
        public string Version => "1.0.0";
        public PluginType Type => PluginType.Plugin;

        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            _view = new DataShareTestView(_pluginApi);
            
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