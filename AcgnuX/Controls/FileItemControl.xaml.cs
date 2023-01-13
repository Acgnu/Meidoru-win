using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AcgnuX.Controls
{
    /// <summary>
    /// FileItemControl.xaml 的交互逻辑
    /// </summary>
    public partial class FileItemControl : UserControl
    {
        public FileItemControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 容器Border鼠标按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContainerBorderLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            Border bd = sender as Border;
            //设置为焦点, 方便其他地方跟踪按键输入事件
            bd.Focus();
        }
    }
}
