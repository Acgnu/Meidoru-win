using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.ViewModel;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.WindowX
{
    /// <summary>
    /// Tan8DownloadRecordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Tan8DownloadRecordWindow : BaseDialog
    {
        /// <summary>
        /// 控件数据源
        /// </summary>
        private readonly PianoScoreDownloadRecordViewModel _ContentDataContext;

        public Tan8DownloadRecordWindow(Action<Tan8SheetCrawlArg> downloadAction)
        {
            InitializeComponent();
            DataContext = this;
            _ContentDataContext = MainContentDock.DataContext as PianoScoreDownloadRecordViewModel;
            _ContentDataContext._DownloadAction = downloadAction;
            //监听单个乐谱下载完成事件
            Messenger.Default.Register<int>(this, ApplicationConstant.TAN8_DOWNLOAD_COMPLETE, (ypid) =>
            {
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    _ContentDataContext.OnSheetItemDownloadComplete(ypid);
                });
            });
        }

        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //加载曲谱下载记录
            _ContentDataContext.Load();
        }

        /// <summary>
        /// 下载记录键盘按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadRecordGridKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Delete == e.Key)
            {
                //BatchDeleteDownloadRecord();
            }
        }

        /// <summary>
        /// 过滤框选中变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFilterBoxCheckedChange(object sender, RoutedEventArgs e)
        {
            _ContentDataContext.Load();
            Console.WriteLine("111");
        }

        /// <summary>
        /// 重写窗口关闭事件
        /// </summary>
        /// <param name="e"></param>
        /**
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;  //标识取消close
            Hide();      // 隐藏, 方便再次调用show()
        }
        **/
    }
}
