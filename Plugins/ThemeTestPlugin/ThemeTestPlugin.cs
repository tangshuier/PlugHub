using System.Windows.Controls;
using WPFPluginToolbox.Core;

namespace ThemeTestPlugin
{
    public class ThemeTestPlugin : IPlugin
    {
        private IPluginAPI? _pluginApi;
        private ThemeTestView? _view;

        public string Id => "ThemeTestPlugin";
        public string Name => "主题色测试插件";
        public string Description => "测试工具箱主题色功能，用于插件样式开发";
        public string Version => "1.0.0";
        public PluginType Type => PluginType.Plugin;

        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            _view = new ThemeTestView(_pluginApi);
            
            _pluginApi.Debug($"{Name} 初始化完成");
        }

        public void Activate()
        {
            _pluginApi?.Info($"{Name} 激活");
        }

        public void Deactivate()
        {
            _pluginApi?.Info($"{Name} 停用");
        }

        public UserControl GetMainView()
        {
            return _view ?? new UserControl();
        }

        public void Dispose()
        {
            _pluginApi?.Info($"{Name} 释放资源");
        }
    }
}