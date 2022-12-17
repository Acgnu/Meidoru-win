
using AcgnuX.WindowX.Dialog;
using System;
using System.Windows;

namespace AcgnuX.Pages
{
    /// <summary>
    /// MobileDeviceControl.xaml 的交互逻辑
    /// </summary>
    public partial class MobileDeviceControl : BasePage
    {
        //标识是否已经监听了串口
        // private bool mIsHookedUsb = false;

        public MobileDeviceControl()
        {
            InitializeComponent();
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

        /// <summary>
        /// 监听串口设备
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr DeveiceChanged(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //if (msg == WindowsMessage.WM_DEVICECHANGE)
            //{
            //    switch (wParam.ToInt32())
            //    {
            //        case WindowsMessage.DBT_DEVICEARRIVAL://设备插入  
            //            CheckDevice(true);
            //            break;
            //        case WindowsMessage.DBT_DEVICEREMOVECOMPLETE: //设备卸载
            //            CheckDevice(false);
            //            break;
            //        default:
            //            break;
            //    }
            //}
            return IntPtr.Zero;
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
