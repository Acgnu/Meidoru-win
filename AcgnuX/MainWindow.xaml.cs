using AcgnuX.Pages;
using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.ViewModel;
using AcgnuX.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Navigation;

namespace AcgnuX
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //标识状态栏是否有任务执行
        private bool mIsProgressRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            //DataContext = new MainWindowViewModel(this);
        }

        /// <summary>
        /// 主窗口加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //如果没有配置数据库, 提示错误
            if(string.IsNullOrEmpty(Settings.Default.DBFilePath))
            {
                SetStatusBarText(AlertLevel.WARN, "没有配置数据库文件路径, 部分功能将无法正常使用");
            }
        }
        /// <summary>
        /// 清空消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearMessageButtonClick(object sender, RoutedEventArgs e)
        {
            //将信息恢复默认
            SetStatusBarText(AlertLevel.INFO, "就绪");

            //隐藏按钮
            var btn = sender as Button;
            btn.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 设置主窗口状态栏文及进度
        /// </summary>
        /// <param name="notify"></param>
        public void SetStatustProgess(MainWindowStatusNotify notify)
        {
            //MainProgressBar.Dispatcher.Invoke(() =>
            //{
            //    SetStatusBarText(notify.alertLevel, notify.message);
            //    SetProgess(notify);
            //});
            //this.Dispatcher.BeginInvoke((Action)delegate ()
            //{
            //    SetStatusBarText(notify.alertLevel, notify.message);
            //    SetProgess(notify);
            //});
        }
        /// <summary>
        /// 根据事件变更状态栏信息
        /// </summary>
        /// <param name="alertLevel">警告级别</param>
        /// <param name="message">提示文字</param>
        private void SetStatusBarText(AlertLevel alertLevel, string message)
        {
            //switch (alertLevel)
            //{
            //    case AlertLevel.INFO: MainProgressBar.Foreground = Application.Current.FindResource("FooterBarColorBrush") as SolidColorBrush; break;
            //    case AlertLevel.RUN: MainProgressBar.Foreground = Brushes.ForestGreen; break;
            //    case AlertLevel.WARN: MainProgressBar.Foreground = Brushes.Orange; break;
            //    case AlertLevel.ERROR: MainProgressBar.Foreground = Brushes.Red; break;
            //}
            //MainStatusBarText.Text = message;

            ////如果没有任务在运行, 则显示清空按钮
            //if (!mIsProgressRunning)
            //{
            //    ClearStatusBarTextButton.Visibility = Visibility.Visible;
            //}
        }

        /// <summary>
        /// 执行进度条动画
        /// </summary>
        private void SetProgess(MainWindowStatusNotify notify)
        {
            //如果状态栏有正在执行的任务, 标识任务正在执行, 且隐藏清除消息按钮
            //if (notify.nowProgress < MainProgressBar.Maximum && !mIsProgressRunning)
            //{
            //    mIsProgressRunning = true;
            //    ClearStatusBarTextButton.Visibility = Visibility.Collapsed;
            //    StopTaskButton.Visibility = Visibility.Visible;
            //}

            ////如果一个任务已经执行完毕, 则显示清除按钮
            //if (notify.nowProgress == MainProgressBar.Maximum && mIsProgressRunning)
            //{
            //    mIsProgressRunning = false;
            //    ClearStatusBarTextButton.Visibility = Visibility.Visible;
            //    StopTaskButton.Visibility = Visibility.Collapsed;
            //}

            ////标识需要动画才会执行进度条动画
            ////if (notify.animateProgress)
            ////{
            ////MainProgressBar.IsIndeterminate = true;
            //Duration duration = new Duration(TimeSpan.FromMilliseconds(notify.progressDuration));
            //DoubleAnimation doubleanimation = new DoubleAnimation(notify.oldProgress, notify.nowProgress, duration);
            //MainProgressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            //}
            //else
            //{
            //    //没有动画则直接赋值
            //    MainProgressBar.Dispatcher.Invoke(new Action<DependencyProperty, object>(MainProgressBar.SetValue), DispatcherPriority.Background, ProgressBar.ValueProperty, notify.nowProgress);
            //}
        }

        /// <summary>
        /// 中断任务点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStopTaskButtonClick(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// 状态栏右键事件, 复制状态栏文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStatusBarTextRightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //var barText = MainStatusBarText.Text;
            //if (string.IsNullOrEmpty(barText)) return;
            //Clipboard.SetDataObject(barText);
        }
    }
}
