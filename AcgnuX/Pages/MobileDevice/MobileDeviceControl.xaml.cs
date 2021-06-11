
using AcgnuX.Pages.MobileSync;
using AcgnuX.Source.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace AcgnuX.Pages
{
    /// <summary>
    /// MobileDeviceControl.xaml 的交互逻辑
    /// </summary>
    public partial class MobileDeviceControl : BasePage
    {
        //菜单集合
        private List<dynamic> mSubPages;
        private int mNextPageIndex = 1;
        public MobileDeviceControl(MainWindow mainWin)
        {
            mMainWindow = mainWin;
            InitializeComponent();
            InitializeSubPages();
         }

        /// <summary>
        /// 切换按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSwitchButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            //切换Frame的Page内容
            ContentFrame.Content = mSubPages[mNextPageIndex];
            mNextPageIndex = mNextPageIndex == 0 ? 1 : 0;
        }

        /// <summary>
        /// 初始化子页面
        /// </summary>
        private void InitializeSubPages()
        {
            mSubPages = new List<dynamic>()
            {
               Activator.CreateInstance(typeof(SyncPictures)),
               Activator.CreateInstance(typeof(SyncMusic))
            };
        }
    }
}
