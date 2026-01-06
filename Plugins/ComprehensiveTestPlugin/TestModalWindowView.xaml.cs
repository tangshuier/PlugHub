using System.Windows;
using System.Windows.Controls;

namespace ComprehensiveTestPlugin
{
    /// <summary>
    /// TestModalWindowView.xaml 的交互逻辑
    /// </summary>
    public partial class TestModalWindowView : UserControl
    {
        public TestModalWindowView()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取父窗口并关闭
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
    }
}