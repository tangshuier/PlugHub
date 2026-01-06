using System;
using WPFPluginToolbox.Core;

namespace ChineseFontMatrixPlugin
{
    public class ChineseFontMatrixPlugin : IPlugin
    {
        private IPluginAPI? _pluginApi;
        private ChineseFontMatrixPluginView? _mainView;

        public string Id => "ChineseFontMatrixPlugin";
        
        public string Name => "汉字字模生成插件";
        
        public string Description => "自动OLED汉字字模生成工具，检索代码中的中文并生成对应的字模数据";
        
        public string Version => "1.0.0";
        
        public PluginType Type => PluginType.Plugin;
        
        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            _pluginApi.Info($"{Name} 初始化完成");
        }
        
        public void Activate()
        {
            _pluginApi?.Info($"{Name} 激活");
        }
        
        public void Deactivate()
        {
            _pluginApi?.Info($"{Name} 停用");
        }
        
        public System.Windows.Controls.UserControl GetMainView()
        {
            if (_mainView == null && _pluginApi != null)
            {
                _mainView = new ChineseFontMatrixPluginView(_pluginApi);
            }
            return _mainView ?? throw new InvalidOperationException($"{Name} 主视图未初始化");
        }
        
        public void Dispose()
        {
            _pluginApi?.Info($"{Name} 释放资源");
            _mainView?.Dispose();
        }
    }
}