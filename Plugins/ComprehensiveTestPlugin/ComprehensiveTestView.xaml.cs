using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ComprehensiveTestPlugin
{
    /// <summary>
    /// ComprehensiveTestView.xaml 的交互逻辑
    /// </summary>
    public partial class ComprehensiveTestView : UserControl
    {
        public TestViewModel ViewModel { get; private set; }

        /// <summary>
        /// 运行测试事件
        /// </summary>
        public event RoutedEventHandler? RunTests;

        /// <summary>
        /// 获取选中的测试类型列表
        /// </summary>
        public List<string> SelectedTestTypes
        {
            get { return ViewModel.SelectedTestTypes; }
        }

        public ComprehensiveTestView()
        {
            InitializeComponent();
            
            // 初始化ViewModel
            ViewModel = new TestViewModel();
            this.DataContext = ViewModel;
            
            // 绑定事件
            TestButton.Click += TestButton_Click;
            SelectAllButton.Click += SelectAllButton_Click;
            ClearAllButton.Click += ClearAllButton_Click;
        }

        /// <summary>
        /// 运行测试按钮点击事件
        /// </summary>
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            // 添加调试信息，确认方法被调用
            System.Diagnostics.Debug.WriteLine("TestButton_Click 方法被调用");
            System.Console.WriteLine("TestButton_Click 方法被调用");
            
            // 显式检查并触发事件
            if (RunTests != null)
            {
                System.Diagnostics.Debug.WriteLine("RunTests 事件被触发");
                System.Console.WriteLine("RunTests 事件被触发");
                RunTests(this, e);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("RunTests 事件没有订阅者");
                System.Console.WriteLine("RunTests 事件没有订阅者");
            }
        }

        /// <summary>
        /// 全选按钮点击事件
        /// </summary>
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectAll();
        }

        /// <summary>
        /// 全不选按钮点击事件
        /// </summary>
        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearAll();
        }
    }

    /// <summary>
    /// 测试视图模型
    /// </summary>
    public class TestViewModel : INotifyPropertyChanged
    {
        private List<TestItem> _testItems;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 测试项目列表
        /// </summary>
        public List<TestItem> TestItems
        {
            get { return _testItems; }
            set
            {
                _testItems = value;
                OnPropertyChanged(nameof(TestItems));
            }
        }

        /// <summary>
        /// 获取选中的测试类型列表
        /// </summary>
        public List<string> SelectedTestTypes
        {
            get
            {
                List<string> selectedTypes = new List<string>();
                foreach (var item in _testItems)
                {
                    if (item.IsSelected)
                    {
                        selectedTypes.Add(item.TestType);
                    }
                }
                return selectedTypes;
            }
        }

        public TestViewModel()
        {
            // 初始化测试项目
            _testItems = new List<TestItem>
            {
                new TestItem("日志功能测试", "Logging"),
                new TestItem("文件操作测试", "FileOperations"),
                new TestItem("窗口操作测试", "WindowOperations"),
                new TestItem("依赖管理测试", "DependencyManagement"),
                new TestItem("配置功能测试", "Configuration"),
                new TestItem("边界情况测试", "EdgeCases")
            };
        }

        /// <summary>
        /// 全选测试项目
        /// </summary>
        public void SelectAll()
        {
            foreach (var item in _testItems)
            {
                item.IsSelected = true;
            }
        }

        /// <summary>
        /// 全不选测试项目
        /// </summary>
        public void ClearAll()
        {
            foreach (var item in _testItems)
            {
                item.IsSelected = false;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}