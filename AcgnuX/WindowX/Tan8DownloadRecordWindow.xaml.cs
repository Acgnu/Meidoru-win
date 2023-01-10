using AcgnuX.Properties;
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
    public partial class Tan8DownloadManageWindow : BaseDialog
    {
        /// <summary>
        /// 控件数据源
        /// </summary>
        public PianoScoreDownloadRecordViewModel ContentDataContext { get; set; }

        public Tan8DownloadManageWindow()
        {
            InitializeComponent();
            DataContext = this;
            ContentDataContext = MainContent.DataContext as PianoScoreDownloadRecordViewModel;
            //监听单个乐谱下载完成事件
            Messenger.Default.Register<int>(this, ApplicationConstant.TAN8_DOWNLOAD_COMPLETE, (ypid) =>
            {
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    ContentDataContext.OnSheetItemDownloadComplete(ypid, DownloadRecordDataGrid);
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
            ContentDataContext.Load();
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
            ContentDataContext.Load();
        }

        /// <summary>
        /// 新增下载任务点击按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddNewClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.DBFilePath))
            {
                Messenger.Default.Send(new BubbleTipViewModel
                {
                    Text = "答应我, 先去配置数据库",
                    AlertLevel = AlertLevel.ERROR
                });
                return;
            }
            var dialog = new AddSinglePianoScoreDialog
            {
                //绑定窗口点击事件
                ConfirmAction = ContentDataContext.TriggerTan8DownLoadTask
            };
            dialog.ShowDialog();
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
