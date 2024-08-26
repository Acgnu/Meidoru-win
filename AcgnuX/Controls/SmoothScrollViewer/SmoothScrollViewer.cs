using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AcgnuX.Controls.SmoothScrollViewer
{
    /// <summary>
    /// 此组件可以在xaml中通过定义本地组件方式使用, 配对的Adapter和Behavior会被自动调用
    /// 此组件需要设置 CanContentScroll = False, 并且设置 VirtualizingPanel.ScrollUnit = Pixel
    /// 此组件源码来自于网上 https://www.wpf-controls.com/wpf-smooth-scroll-viewer/
    /// 此组件在内容过多时会产生性能问题以及UI线程等待问题, 因此仅作为学习代码留下, 不直接使用
    /// </summary>
    public class SmoothScrollViewer : System.Windows.Controls.ScrollViewer
    {
        public SmoothScrollViewer()
        {
            Loaded += ScrollViewer_Loaded;
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollInfo = new ScrollInfoAdapter(ScrollInfo);
        }
    }
}
