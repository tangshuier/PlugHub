using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using WPFPluginToolbox.Core;

namespace TestPlugin
{
    [Export(typeof(IPlugin))]
    public class TestPlugin : IPlugin
    {
        private IPluginAPI? _pluginApi;
        private TestPluginView? _mainView;

        public string Id => "TestPlugin";
        
        public string Name => "测试插件";
        
        public string Description => "测试工具箱各种功能的插件";
        
        public string Version => "1.0.0";
        
        public PluginType Type => PluginType.Plugin;
        
        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            _pluginApi.Info("测试插件已初始化");
        }
        
        public void Activate()
        {
            _pluginApi?.Info("测试插件已激活");
        }
        
        public void Deactivate()
        {
            _pluginApi?.Info("测试插件已停用");
        }
        
        public UserControl GetMainView()
        {
            if (_mainView == null && _pluginApi != null)
            {
                _mainView = new TestPluginView(_pluginApi);
            }
            return _mainView!;
        }
        
        public void Dispose()
        {
            _pluginApi?.Info("测试插件已释放");
        }
    }
}