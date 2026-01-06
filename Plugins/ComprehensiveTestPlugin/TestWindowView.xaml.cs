using System.Windows;
using System.Windows.Controls;

namespace ComprehensiveTestPlugin
{
    /// <summary>
    /// TestWindowView.xaml 的交互逻辑
    /// </summary>
    public partial class TestWindowView : UserControl
    {
        public TestWindowView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取父窗口并关闭
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }
}