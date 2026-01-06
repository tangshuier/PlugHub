using System.Diagnostics;using System.Windows;using System.Windows.Controls;using WPFPluginToolbox.Core;

namespace PerformanceTestPlugin
{
    public partial class PerformanceTestView : UserControl
    {
        private readonly IPluginAPI _pluginApi;
        private bool _isTiming = false;

        public PerformanceTestView(IPluginAPI pluginApi)
        {
            InitializeComponent();
            _pluginApi = pluginApi;
            Log("性能监控测试插件视图初始化完成");
        }

        private void Log(string message)
        {
            ResultTextBlock.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            _pluginApi.Debug(message);
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            var operationName = OperationNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(operationName))
            {
                Log("操作名称不能为空！");
                return;
            }

            if (!int.TryParse(TestCountTextBox.Text, out int testCount))
            {
                Log("测试次数必须是数字！");
                return;
            }

            if (testCount <= 0)
            {
                Log("测试次数必须大于0！");
                return;
            }

            try
            {
                Log($"开始执行耗时操作 - 名称: {operationName}, 次数: {testCount}");

                // 创建Stopwatch实例
                var stopwatch = Stopwatch.StartNew();

                // 使用PerformanceMonitor监控整个操作
                _pluginApi.StartOperationTimer("ExecuteHeavyOperation");

                // 执行耗时操作
                long totalResult = 0;
                for (int i = 0; i < testCount; i++)
                {
                    // 执行一些耗时的计算
                    totalResult += Fibonacci(20);
                }

                // 停止计时
                _pluginApi.StopOperationTimer("ExecuteHeavyOperation");
                stopwatch.Stop();

                Log($"耗时操作执行完成 - 结果: {totalResult}, 耗时: {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                Log($"执行耗时操作失败 - 错误: {ex.Message}");
                _pluginApi.StopOperationTimer("ExecuteHeavyOperation");
            }
        }

        // 计算斐波那契数列，用于模拟耗时操作
        private long Fibonacci(int n)
        {
            if (n <= 1)
                return n;
            return Fibonacci(n - 1) + Fibonacci(n - 2);
        }

        private void StartTimerButton_Click(object sender, RoutedEventArgs e)
        {
            var operationName = OperationNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(operationName))
            {
                Log("操作名称不能为空！");
                return;
            }

            if (_isTiming)
            {
                Log("已经在计时中！");
                return;
            }

            try
            {
                _pluginApi.StartOperationTimer(operationName);
                _isTiming = true;
                Log($"开始计时 - 操作: {operationName}");
            }
            catch (Exception ex)
            {
                Log($"开始计时失败 - 错误: {ex.Message}");
            }
        }

        private void StopTimerButton_Click(object sender, RoutedEventArgs e)
        {
            var operationName = OperationNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(operationName))
            {
                Log("操作名称不能为空！");
                return;
            }

            if (!_isTiming)
            {
                Log("还没有开始计时！");
                return;
            }

            try
            {
                _pluginApi.StopOperationTimer(operationName);
                _isTiming = false;
                Log($"停止计时 - 操作: {operationName}");
            }
            catch (Exception ex)
            {
                Log($"停止计时失败 - 错误: {ex.Message}");
            }
        }
    }
}