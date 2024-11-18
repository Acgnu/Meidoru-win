using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AcgnuX.Controls
{
    /// <summary>
    /// FileItemListControl.xaml 的交互逻辑
    /// </summary>
    public partial class FileItemListControl : UserControl
    {
        public FileItemListControl()
        {
            InitializeComponent();
        }

        private void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (!e.Handled)
            //{
            //    e.Handled = true;
            //    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            //    eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            //    eventArg.Source = sender;
            //    var parent = ((Control)sender).Parent as UIElement;
            //    parent.RaiseEvent(eventArg);
            //}
        }

        private void SubItemOnKeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show("abc");
        }

        /// <summary>
        /// 容器Border鼠标按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var container = sender as ContentControl;
            var bd = VisualTreeHelper.GetChild(container, 0) as Border;
            //设置为焦点, 方便其他地方跟踪按键输入事件
            bd.Focus();
            //e.Handled = true;  //不需要阻止, 会影响文件点击命令
        }
    }
}
