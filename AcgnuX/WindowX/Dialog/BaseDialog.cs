using AcgnuX.Pages;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 对话框基类
    /// </summary>
    public class BaseDialog : Window
    {
        //默认的标题栏高度
        public int TitleHeightGridLength { get; set; } = 30;

        //是否展示在状态栏
        public bool ShowInTaskBar { get; set; } = false;

        //状态栏调整大小模式
        public ResizeMode DialogResizeMode { get; set; } = ResizeMode.NoResize;

        //允许最小化
        public Visibility CanMinimize { get; set; } = Visibility.Collapsed;

        //允许最大化
        public Visibility CanMaxmize { get; set; } = Visibility.Collapsed;

        //最小化命令
        public ICommand MinimizeCommand { get; set; }

        //最大化
        public ICommand MaximizeCommand { get; set; }

        //关闭对话框命令
        public ICommand CloseCommand { get; set; }

        //关闭窗口动画计时器
        private DispatcherTimer mDispatcherTimer;

        /// <summary>
        /// 构造函数接收一个窗口作为父窗口
        /// </summary>
        public BaseDialog()
        {
            CloseCommand = new RelayCommand(() => {
                //this.Close();
                mDispatcherTimer.Start();
            });
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;
            mDispatcherTimer = new DispatcherTimer();
            mDispatcherTimer.Tick += new EventHandler(DispatcherTimerHandler);
            mDispatcherTimer.Interval = TimeSpan.FromSeconds(0.2);
        }

        private void DispatcherTimerHandler(object sender, EventArgs e)
        {
            this.mDispatcherTimer.Stop();
            this.Close();
        }
    }
}
