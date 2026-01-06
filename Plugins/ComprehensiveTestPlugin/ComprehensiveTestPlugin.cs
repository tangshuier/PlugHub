using System;
using System.Windows;
using System.Windows.Controls;
using WPFPluginToolbox.Core;

namespace ComprehensiveTestPlugin
{
    public class ComprehensiveTestPlugin : IPlugin
    {
        private IPluginAPI? _pluginApi;
        private ComprehensiveTestView? _mainView;
        private TestConfig? _config;
        private System.Windows.Window? _testWindow;
        private bool _isDisposed;

        // 插件基本信息
        public string Id { get; } = "ComprehensiveTestPlugin";
        public string Name { get; } = "全面测试插件";
        public string Description { get; } = "用于全面测试WPF插件工具箱功能的插件";
        public string Version { get; } = "1.0.0";
        public PluginType Type { get; } = PluginType.Plugin;

        // 初始化方法
        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            
            // 测试日志记录是否正常工作
            Console.WriteLine($"{Name} 初始化开始");
            _pluginApi.Debug($"{Name} 初始化开始");
            _pluginApi.Info($"{Name} 初始化开始");
            _pluginApi.Warn($"{Name} 初始化开始");

            // 仅初始化UI，不执行测试
            _mainView = new ComprehensiveTestView();
            
            // 显式绑定事件，确保事件处理程序被添加
            Console.WriteLine("绑定 RunTests 事件处理程序");
            _mainView.RunTests += RunSelectedTests_Handler;

            _pluginApi.Info($"{Name} 初始化完成");
        }

        // 显式的事件处理程序，用于调试
        private void RunSelectedTests_Handler(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("RunSelectedTests_Handler 被调用");
            RunSelectedTests();
        }

        // 激活方法
        public void Activate()
        {
            _pluginApi?.Info($"{Name} 激活");
            // 不自动运行所有测试，避免重复创建窗口
        }

        // 停用方法
        public void Deactivate()
        {
            _pluginApi?.Info($"{Name} 停用");
            
            // 关闭测试窗口
            if (_testWindow != null)
            {
                _pluginApi?.CloseWindow(_testWindow);
                _testWindow = null;
            }
        }

        // 释放资源
        public void Dispose()
        {
            if (_isDisposed) return;
            
            _pluginApi?.Info($"{Name} 释放资源");
            
            // 关闭测试窗口
            if (_testWindow != null)
            {
                _pluginApi?.CloseWindow(_testWindow);
                _testWindow = null;
            }
            
            _isDisposed = true;
        }

        // 返回主视图
        public UserControl GetMainView()
        {
            return _mainView ?? throw new InvalidOperationException("插件主视图未初始化");
        }

        // 运行所有测试
        public async void RunAllTests()
        {
            _pluginApi?.Info("=================== 开始运行所有测试 ===================");
            
            await TestLoggingAsync();
            await TestFileOperationsAsync();
            await TestWindowOperationsAsync();
            await TestDependencyManagementAsync();
            await TestConfigurationAsync();
            await TestEdgeCasesAsync();
            
            _pluginApi?.Info("=================== 所有测试运行完成 ===================");
        }

        // 运行选中的测试
        public async void RunSelectedTests()
        {
            // 添加调试信息，确认方法被调用
            Console.WriteLine("RunSelectedTests 方法被调用");
            
            if (_pluginApi == null)
            {
                Console.WriteLine("_pluginApi 为 null");
                return;
            }
            
            // 获取选中的测试类型
            var selectedTestTypes = _mainView?.SelectedTestTypes ?? new List<string>();
            
            // 如果没有选中测试类型，默认执行所有测试
            if (selectedTestTypes.Count == 0)
            {
                _pluginApi.Info("=================== 开始运行所有测试 ===================");
                await TestLoggingAsync();
                await TestFileOperationsAsync();
                await TestWindowOperationsAsync();
                await TestDependencyManagementAsync();
                await TestConfigurationAsync();
                await TestEdgeCasesAsync();
                _pluginApi.Info("=================== 所有测试运行完成 ===================");
                return;
            }
            
            // 根据选中的测试类型执行相应的测试
            _pluginApi.Info($"=================== 开始运行选中的 {selectedTestTypes.Count} 个测试 ===================");
            
            foreach (var testType in selectedTestTypes)
            {
                _pluginApi.Info($"执行测试: {testType}");
                
                switch (testType)
                {
                    case "Logging":
                        await TestLoggingAsync();
                        break;
                    case "FileOperations":
                        await TestFileOperationsAsync();
                        break;
                    case "WindowOperations":
                        await TestWindowOperationsAsync();
                        break;
                    case "DependencyManagement":
                        await TestDependencyManagementAsync();
                        break;
                    case "Configuration":
                        await TestConfigurationAsync();
                        break;
                    case "EdgeCases":
                        await TestEdgeCasesAsync();
                        break;
                    default:
                        _pluginApi.Warn($"未知的测试类型: {testType}");
                        break;
                }
            }
            
            _pluginApi.Info("=================== 选中的测试运行完成 ===================");
        }

        // 测试日志功能
        private async Task TestLoggingAsync()
        {
            _pluginApi?.Info("=== 测试日志功能 ===");
            
            try
            {
                // 测试不同级别的日志
                _pluginApi?.Debug("这是一条调试日志");
                _pluginApi?.Info("这是一条普通信息日志");
                _pluginApi?.Warn("这是一条警告日志");
                
                // 测试带异常的错误日志
                throw new Exception("测试异常");
            }
            catch (Exception ex)
            {
                _pluginApi?.Error("这是一条带异常的错误日志", ex);
            }
            
            _pluginApi?.Info("=== 日志功能测试完成 ===");
            await Task.CompletedTask;
        }

        // 测试文件操作
        private async Task TestFileOperationsAsync()
        {
            _pluginApi?.Info("=== 测试文件操作 ===");
            
            try
            {
                // 测试创建目录
                string testDir = "ComprehensiveTest";
                if (_pluginApi != null)
                {
                    await _pluginApi.CreateDirectoryAsync(testDir);
                    _pluginApi.Info($"创建目录: {testDir}");
                    
                    // 测试写入文件
                    string testFile = $"{testDir}/test.txt";
                    await _pluginApi.WriteFileAsync(testFile, "这是测试文件的内容");
                    _pluginApi.Info($"写入文件: {testFile}");
                    
                    // 测试文件是否存在
                    bool exists = _pluginApi.FileExists(testFile);
                    _pluginApi.Info($"检查文件是否存在: {exists}");
                    
                    // 测试读取文件
                    string content = await _pluginApi.ReadFileAsync(testFile);
                    _pluginApi.Info($"读取文件内容: {content}");
                    
                    // 测试搜索文件
                    var files = _pluginApi.SearchFiles(testDir, "*.txt", true);
                    if (files != null)
                    {
                        _pluginApi.Info($"搜索文件结果: {string.Join(", ", files)}");
                    }
                    
                    // 测试删除文件
                    await _pluginApi.DeleteFileAsync(testFile);
                    _pluginApi.Info($"删除文件: {testFile}");
                    
                    // 测试删除后的文件是否存在
                    exists = _pluginApi.FileExists(testFile);
                    _pluginApi.Info($"检查删除后的文件是否存在: {exists}");
                }
                
            } catch (Exception ex)
            {
                _pluginApi?.Error("文件操作测试失败", ex);
            }
            
            _pluginApi?.Info("=== 文件操作测试完成 ===");
        }

        // 测试窗口操作
        private async Task TestWindowOperationsAsync()
        {
            _pluginApi?.Info("=== 测试窗口操作 ===");
            
            try
            {
                // 仅创建非模态窗口，不自动显示模态窗口
                // 避免阻塞UI线程和跨线程问题
                _testWindow = _pluginApi?.CreateWindow("测试窗口", new TestWindowView());
                if (_testWindow != null)
                {
                    _pluginApi?.ShowWindow(_testWindow);
                    _pluginApi?.Info("创建并显示非模态窗口");
                }
                
            } catch (Exception ex)
            {
                _pluginApi?.Error("窗口操作测试失败", ex);
            }
            
            _pluginApi?.Info("=== 窗口操作测试完成 ===");
            await Task.CompletedTask;
        }

        // 测试依赖管理
        private async Task TestDependencyManagementAsync()
        {
            _pluginApi?.Info("=== 测试依赖管理 ===");
            
            try
            {
                // 测试检查不存在的依赖
                bool hasDependency = _pluginApi?.HasDependency("NonExistentDependency") ?? false;
                _pluginApi?.Info($"检查不存在的依赖: {hasDependency}");
                
                // 测试获取不存在的依赖
                var dependency = _pluginApi?.GetDependency("NonExistentDependency");
                _pluginApi?.Info($"获取不存在的依赖: {(dependency is null ? "null" : "获取到依赖")}");
                
                // 测试检查TestDependency依赖
                hasDependency = _pluginApi?.HasDependency("TestDependency") ?? false;
                _pluginApi?.Info($"检查TestDependency依赖: {hasDependency}");
                
                // 测试获取TestDependency依赖
                dependency = _pluginApi?.GetDependency("TestDependency");
                _pluginApi?.Info($"获取TestDependency依赖: {(dependency is null ? "null" : dependency.Name)}");
                
            } catch (Exception ex)
            {
                _pluginApi?.Error("依赖管理测试失败", ex);
            }
            
            _pluginApi?.Info("=== 依赖管理测试完成 ===");
            await Task.CompletedTask;
        }

        // 测试配置功能
        private async Task TestConfigurationAsync()
        {
            _pluginApi?.Info("=== 测试配置功能 ===");
            
            try
            {
                if (_pluginApi != null)
                {
                    // 获取或创建配置
                    _config = _pluginApi.GetConfig(new TestConfig());
                    if (_config != null)
                    {
                        _pluginApi.Info($"获取配置: Setting1={_config.Setting1}, Setting2={_config.Setting2}, Setting3={_config.Setting3}");
                        
                        // 修改配置
                        _config.Setting1 = "修改后的字符串";
                        _config.Setting2 = 999;
                        _config.Setting3 = !_config.Setting3;
                        
                        // 保存配置
                        await _pluginApi.SaveConfigAsync(_config);
                        _pluginApi.Info($"保存配置: Setting1={_config.Setting1}, Setting2={_config.Setting2}, Setting3={_config.Setting3}");
                        
                        // 重新获取配置，验证保存是否成功
                        var reloadedConfig = _pluginApi.GetConfig(new TestConfig());
                        if (reloadedConfig != null)
                        {
                            _pluginApi.Info($"重新获取配置: Setting1={reloadedConfig.Setting1}, Setting2={reloadedConfig.Setting2}, Setting3={reloadedConfig.Setting3}");
                        }
                    }
                    
                    // 测试配置文件是否存在
                    bool hasConfig = _pluginApi.HasConfig();
                    _pluginApi.Info($"检查配置文件是否存在: {hasConfig}");
                    
                    // 获取配置目录
                    string configDir = _pluginApi.ConfigDirectory;
                    _pluginApi.Info($"配置目录: {configDir}");
                }
                
            } catch (Exception ex)
            {
                _pluginApi?.Error("配置功能测试失败", ex);
            }
            
            _pluginApi?.Info("=== 配置功能测试完成 ===");
        }

        // 测试边界情况
        private async Task TestEdgeCasesAsync()
        {
            _pluginApi?.Info("=== 测试边界情况 ===");
            
            try
            {
                if (_pluginApi != null)
                {
                    // 测试空字符串
                    await _pluginApi.WriteFileAsync("empty.txt", string.Empty);
                    string content = await _pluginApi.ReadFileAsync("empty.txt");
                    _pluginApi.Info($"写入并读取空字符串: 长度={content.Length}");
                    await _pluginApi.DeleteFileAsync("empty.txt");
                    
                    // 测试长文件名
                    string longFileName = new string('a', 200) + ".txt";
                    await _pluginApi.WriteFileAsync(longFileName, "长文件名测试");
                    bool exists = _pluginApi.FileExists(longFileName);
                    _pluginApi.Info($"创建长文件名文件: {exists}");
                    await _pluginApi.DeleteFileAsync(longFileName);
                    
                    // 测试大量日志
                    for (int i = 0; i < 100; i++)
                    {
                        _pluginApi.Debug($"大量日志测试 - {i}");
                        // 每10条日志添加一个短暂延迟，避免UI阻塞
                        if (i % 10 == 0)
                        {
                            await Task.Delay(1);
                        }
                    }
                    _pluginApi.Info("大量日志测试完成");
                }
                
            } catch (Exception ex)
            {
                _pluginApi?.Error("边界情况测试失败", ex);
            }
            
            _pluginApi?.Info("=== 边界情况测试完成 ===");
        }
    }

    // 测试配置类
    public class TestConfig
    {
        public string Setting1 { get; set; } = "默认字符串";
        public int Setting2 { get; set; } = 42;
        public bool Setting3 { get; set; } = true;
    }
}