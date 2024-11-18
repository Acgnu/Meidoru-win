
using AcgnuX.Source.ViewModel;
using AcgnuX.WindowX.Dialog;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AcgnuX.Pages
{
    /// <summary>
    /// MobileDeviceControl.xaml 的交互逻辑
    /// </summary>
    public partial class MobileDeviceControl
    {
        //标识是否已经监听了串口
        // private bool mIsHookedUsb = false;

        public MobileDeviceControl()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<DeviceSyncViewModel>();
        }

        /// <summary>
        /// 页面加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnPageLoaded(object sender, System.Windows.RoutedEventArgs eventArgs)
        {
            /**
            if (!mIsHookedUsb)
            {
                //用于监听Windows消息 
                //注意获取窗口句柄一定要写在窗口loaded事件里，才能获取到窗口句柄，否则为空
                HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;//窗口过程
                if (hwndSource != null)
                    hwndSource.AddHook(new HwndSourceHook(DeveiceChanged));  //挂钩
                mIsHookedUsb = true;
            }
            **/
        }

        #region WFP页面元素事件
        //private void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    if (!e.Handled)
        //    {
        //        e.Handled = true;
        //        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
        //        eventArg.RoutedEvent = UIElement.MouseWheelEvent;
        //        eventArg.Source = sender;
        //        var parent = ((Control)sender).Parent as UIElement;
        //        parent.RaiseEvent(eventArg);
        //    }
        //}

        /// <summary>
        /// 打开路径配置界面点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPathConfigSettingClick(object sender, RoutedEventArgs e)
        {
            new DeviceSyncPathConfigDialog().Show();
        }
        #endregion
    }
}
