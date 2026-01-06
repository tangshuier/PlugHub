using System;
using System.Windows.Controls;
using WPFPluginToolbox.Core;

namespace ConfigTestPlugin
{
    /// <summary>
    /// 配置测试插件主类
    /// </summary>
    public class ConfigTestPlugin : IPlugin
    {
        private IPluginAPI? _pluginApi;
        private Config? _config;
        private ConfigTestView? _mainView;

        /// <summary>
        /// 插件ID
        /// </summary>
        public string Id { get; } = "ConfigTestPlugin";

        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; } = "配置测试";

        /// <summary>
        /// 插件描述
        /// </summary>
        public string Description { get; } = "测试插件配置功能";

        /// <summary>
        /// 插件版本
        /// </summary>
        public string Version { get; } = "1.0.0";

        /// <summary>
        /// 插件类型
        /// </summary>
        public PluginType Type { get; } = PluginType.Plugin;

        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="pluginApi">插件API实例</param>
        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            
            // 读取配置
            _config = _pluginApi.GetConfig<Config>(new Config());
            
            _pluginApi.Info($"插件初始化完成，当前选中的选项索引: {_config.SelectedOptionIndex}");
        }

        /// <summary>
        /// 激活插件
        /// </summary>
        public void Activate()
        {
            _pluginApi?.Info("插件已激活");
        }

        /// <summary>
        /// 停用插件
        /// </summary>
        public void Deactivate()
        {
            _pluginApi?.Info("插件已停用");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _pluginApi?.Info("插件已释放");
            // UserControl已经实现了IDisposable，不需要手动调用
        }

        /// <summary>
        /// 获取主视图
        /// </summary>
        /// <returns>主视图控件</returns>
        public UserControl GetMainView()
        {
            if (_mainView == null)
            {
                _mainView = new ConfigTestView(this);
            }
            return _mainView;
        }

        /// <summary>
        /// 获取插件配置
        /// </summary>
        /// <returns>配置对象</returns>
        public Config GetConfig()
        {
            return _config ?? new Config();
        }

        /// <summary>
        /// 保存插件配置
        /// </summary>
        public void SaveConfig()
        {
            if (_config != null)
            {
                _pluginApi?.SaveConfigAsync(_config);
                _pluginApi?.Info($"配置已保存，选中的选项索引: {_config.SelectedOptionIndex}");
            }
        }
    }
}