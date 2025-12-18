using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using WPFPluginToolbox.Core;

namespace TestDependentPlugin
{
    [Export(typeof(IPlugin))]
    public class TestDependentPlugin : IPlugin
    {
        private IPluginAPI _pluginApi;
        private TestDependentPluginView _mainView;
        private bool _hasRequiredDependency;

        public string Id => "TestDependentPlugin";
        
        public string Name => "测试依赖插件";
        
        public string Description => "需要依赖的测试插件";
        
        public string Version => "1.0.0";
        
        public PluginType Type => PluginType.Plugin;
        
        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            
            // 检测必要依赖
            _hasRequiredDependency = _pluginApi.HasDependency("TestDependency");
            
            if (_hasRequiredDependency)
            {
                _pluginApi.Info("已找到必要依赖: TestDependency");
            }
            else
            {
                _pluginApi.Warn("缺少必要依赖: TestDependency，插件功能将受限");
            }
        }
        
        public void Activate()
        {
            _pluginApi.Info("测试依赖插件已激活");
        }
        
        public void Deactivate()
        {
            _pluginApi.Info("测试依赖插件已停用");
        }
        
        public UserControl GetMainView()
        {
            if (_mainView == null)
            {
                _mainView = new TestDependentPluginView(_pluginApi, _hasRequiredDependency);
            }
            return _mainView;
        }
        
        public void Dispose()
        {
            _pluginApi.Info("测试依赖插件已释放");
        }
    }

    // 插件视图
    public class TestDependentPluginView : UserControl
    {
        private IPluginAPI _pluginApi;
        private bool _hasRequiredDependency;
        private TextBlock _resultText;

        public TestDependentPluginView(IPluginAPI pluginApi, bool hasRequiredDependency)
        {
            _pluginApi = pluginApi;
            _hasRequiredDependency = hasRequiredDependency;
            
            // 创建UI
            StackPanel panel = new StackPanel { Margin = new Thickness(10), Background = System.Windows.Media.Brushes.White };
            
            // 标题
            TextBlock title = new TextBlock
            {
                Text = "测试依赖插件",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(title);
            
            // 依赖状态
            TextBlock statusText = new TextBlock
            {
                Text = _hasRequiredDependency ? "✅ 已找到必要依赖: TestDependency" : "❌ 缺少必要依赖: TestDependency",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = _hasRequiredDependency ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red
            };
            panel.Children.Add(statusText);
            
            // 测试按钮
            Button testButton = new Button
            {
                Content = "测试依赖功能",
                Width = 150,
                Margin = new Thickness(0, 0, 0, 10)
            };
            testButton.Click += OnTestDependencyClick;
            panel.Children.Add(testButton);
            
            // 结果显示
            _resultText = new TextBlock
            {
                Text = "等待测试...",
                Margin = new Thickness(0, 10, 0, 0)
            };
            panel.Children.Add(_resultText);
            
            this.Content = panel;
        }

        private void OnTestDependencyClick(object sender, RoutedEventArgs e)
        {
            if (_hasRequiredDependency)
            {
                try
                {
                    // 尝试获取依赖实例
                    var dependency = _pluginApi.GetDependency("TestDependency");
                    if (dependency != null)
                    {
                        // 调用依赖的方法（这里需要反射，因为IDependency接口没有定义GetTestData方法）
                        var method = dependency.GetType().GetMethod("GetTestData");
                        if (method != null)
                        {
                            var result = method.Invoke(dependency, null) as string;
                            _resultText.Text = $"✅ 成功调用依赖方法: {result}";
                            _resultText.Foreground = System.Windows.Media.Brushes.Green;
                            _pluginApi.Info("成功调用依赖方法");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _resultText.Text = $"❌ 调用依赖方法失败: {ex.Message}";
                    _resultText.Foreground = System.Windows.Media.Brushes.Red;
                    _pluginApi.Error("调用依赖方法失败", ex);
                }
            }
            else
            {
                _resultText.Text = "❌ 缺少必要依赖，无法使用此功能";
                _resultText.Foreground = System.Windows.Media.Brushes.Red;
                _pluginApi.Warn("用户尝试使用需要依赖的功能，但缺少依赖");
            }
        }
    }
}