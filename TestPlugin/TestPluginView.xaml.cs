using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WPFPluginToolbox.Core;

namespace TestPlugin
{
    public partial class TestPluginView : UserControl
    {
        private readonly IPluginAPI _pluginApi;
        private string _testFilePath = Path.Combine(Environment.CurrentDirectory, "test.txt");
        private string _testDirectoryPath = Path.Combine(Environment.CurrentDirectory, "TestDirectory");

        public TestPluginView(IPluginAPI pluginApi)
        {
            InitializeComponent();
            _pluginApi = pluginApi;
            
            // 初始化插件信息
            PluginIdText.Text = pluginApi.PluginId;
            PluginNameText.Text = pluginApi.PluginName;
            PluginPathText.Text = pluginApi.PluginPath;
        }

        #region 日志测试

        private void OnDebugLogClick(object sender, RoutedEventArgs e)
        {
            _pluginApi.Debug("这是一条调试日志", new { TestData = "调试数据", Timestamp = DateTime.Now });
        }

        private void OnInfoLogClick(object sender, RoutedEventArgs e)
        {
            _pluginApi.Info("这是一条信息日志", new { TestData = "信息数据", Timestamp = DateTime.Now });
        }

        private void OnWarnLogClick(object sender, RoutedEventArgs e)
        {
            _pluginApi.Warn("这是一条警告日志", new { TestData = "警告数据", Timestamp = DateTime.Now });
        }

        private void OnErrorLogClick(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new Exception("测试异常");
            }
            catch (Exception ex)
            {
                _pluginApi.Error("这是一条错误日志", ex);
            }
        }

        #endregion

        #region 文件操作测试

        private async void OnCreateFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await _pluginApi.CreateFileAsync(_testFilePath);
                FileOperationResult.Text = $"文件已创建: {_testFilePath}";
                _pluginApi.Info($"已创建测试文件: {_testFilePath}");
            }
            catch (Exception ex)
            {
                FileOperationResult.Text = $"创建文件失败: {ex.Message}";
                _pluginApi.Error($"创建测试文件失败", ex);
            }
        }

        private async void OnWriteFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string content = $"测试文件内容\n创建时间: {DateTime.Now}\n插件ID: {_pluginApi.PluginId}";
                await _pluginApi.WriteFileAsync(_testFilePath, content);
                FileOperationResult.Text = $"已写入文件: {_testFilePath}";
                _pluginApi.Info($"已写入测试文件: {_testFilePath}");
            }
            catch (Exception ex)
            {
                FileOperationResult.Text = $"写入文件失败: {ex.Message}";
                _pluginApi.Error($"写入测试文件失败", ex);
            }
        }

        private async void OnReadFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string content = await _pluginApi.ReadFileAsync(_testFilePath);
                FileOperationResult.Text = $"文件内容: {content}";
                _pluginApi.Info($"已读取测试文件: {_testFilePath}");
            }
            catch (Exception ex)
            {
                FileOperationResult.Text = $"读取文件失败: {ex.Message}";
                _pluginApi.Error($"读取测试文件失败", ex);
            }
        }

        private void OnCheckFileExistsClick(object sender, RoutedEventArgs e)
        {
            bool exists = _pluginApi.FileExists(_testFilePath);
            FileOperationResult.Text = $"文件{(_testFilePath)} {(exists ? "存在" : "不存在")}";
            _pluginApi.Info($"检查文件存在: {_testFilePath}, 结果: {exists}");
        }

        private async void OnDeleteFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await _pluginApi.DeleteFileAsync(_testFilePath);
                FileOperationResult.Text = $"文件已删除: {_testFilePath}";
                _pluginApi.Info($"已删除测试文件: {_testFilePath}");
            }
            catch (Exception ex)
            {
                FileOperationResult.Text = $"删除文件失败: {ex.Message}";
                _pluginApi.Error($"删除测试文件失败", ex);
            }
        }

        private async void OnCreateDirectoryClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await _pluginApi.CreateDirectoryAsync(_testDirectoryPath);
                FileOperationResult.Text = $"目录已创建: {_testDirectoryPath}";
                _pluginApi.Info($"已创建测试目录: {_testDirectoryPath}");
            }
            catch (Exception ex)
            {
                FileOperationResult.Text = $"创建目录失败: {ex.Message}";
                _pluginApi.Error($"创建测试目录失败", ex);
            }
        }

        private void OnCheckDirectoryExistsClick(object sender, RoutedEventArgs e)
        {
            bool exists = _pluginApi.DirectoryExists(_testDirectoryPath);
            FileOperationResult.Text = $"目录{(_testDirectoryPath)} {(exists ? "存在" : "不存在")}";
            _pluginApi.Info($"检查目录存在: {_testDirectoryPath}, 结果: {exists}");
        }

        private void OnSearchFilesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<string> files = _pluginApi.SearchFiles(Environment.CurrentDirectory, "*.txt", false);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("找到的 .txt 文件:");
                foreach (string file in files)
                {
                    sb.AppendLine($"  - {file}");
                }
                FileOperationResult.Text = sb.ToString();
                _pluginApi.Info($"搜索到 {files}");
            }
            catch (Exception ex)
            {
                FileOperationResult.Text = $"搜索文件失败: {ex.Message}";
                _pluginApi.Error($"搜索文件失败", ex);
            }
        }

        #endregion

        #region 窗口操作测试

        private void OnCreateNonModalWindowClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UserControl content = new TestWindowContent();
                Window window = _pluginApi.CreateWindow("非模态测试窗口", content, false);
                _pluginApi.ShowWindow(window);
                WindowOperationResult.Text = "已创建并显示非模态窗口";
                _pluginApi.Info("已创建非模态测试窗口");
            }
            catch (Exception ex)
            {
                WindowOperationResult.Text = $"创建窗口失败: {ex.Message}";
                _pluginApi.Error($"创建非模态窗口失败", ex);
            }
        }

        private void OnCreateModalWindowClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UserControl content = new TestWindowContent();
                Window window = _pluginApi.CreateWindow("模态测试窗口", content, true);
                _pluginApi.ShowWindow(window);
                WindowOperationResult.Text = "已创建并显示模态窗口";
                _pluginApi.Info("已创建模态测试窗口");
            }
            catch (Exception ex)
            {
                WindowOperationResult.Text = $"创建窗口失败: {ex.Message}";
                _pluginApi.Error($"创建模态窗口失败", ex);
            }
        }

        #endregion
    }

    // 测试窗口内容
    public class TestWindowContent : UserControl
    {
        public TestWindowContent()
        {
            StackPanel panel = new StackPanel { Margin = new Thickness(20) };
            
            TextBlock title = new TextBlock
            {
                Text = "测试窗口",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(title);
            
            TextBlock content = new TextBlock
            {
                Text = "这是一个由插件创建的测试窗口",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(content);
            
            Button closeButton = new Button
            {
                Content = "关闭窗口",
                Width = 100
            };
            closeButton.Click += (sender, e) =>
            {
                Window.GetWindow(this)?.Close();
            };
            panel.Children.Add(closeButton);
            
            this.Content = panel;
        }
    }
}