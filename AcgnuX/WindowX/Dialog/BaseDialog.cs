using AcgnuX.Pages;
using AcgnuX.Properties;
using AcgnuX.Source.Utils;
using CommunityToolkit.Mvvm.Input;
using SharedLib.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 对话框基类
    /// </summary>
    public class BaseDialog : Window, INotifyPropertyChanged
    {
        //默认的标题栏高度
        public int TitleHeightGridLength { get; } = 30;

        //是否展示在状态栏
        public bool ShowInTaskBar { get; } = false;

        //状态栏调整大小模式
        public ResizeMode DialogResizeMode { get; } = ResizeMode.NoResize;

        //允许最小化
        public Visibility CanMinimize { get; } = Visibility.Collapsed;

        //允许最大化
        public Visibility CanMaxmize { get; } = Visibility.Collapsed;

        //最小化命令
        public ICommand MinimizeCommand { get; }

        //最大化
        public ICommand MaximizeCommand { get; }

        //关闭对话框命令
        public ICommand CloseCommand { get; }

        //弹窗背景图片
        public System.Windows.Media.Brush _dialogWindowBackgroundBrush;
        public System.Windows.Media.Brush DialogWindowBackgroundBrush
        {
            get => _dialogWindowBackgroundBrush;
            set 
            { 
                if (value != null)
                {
                    _dialogWindowBackgroundBrush = value; NotifyPropertyChanged();
                }
            }
        }

        //关闭窗口动画计时器
        //private DispatcherTimer mDispatcherTimer;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 构造函数接收一个窗口作为父窗口
        /// </summary>
        public BaseDialog()
        {
            CloseCommand = new RelayCommand(() => AnimateClose());
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = App.Current.MainWindow;
            DialogWindowBackgroundBrush = XamlUtil.GetMainWindowBackgroundBrush();
            //mDispatcherTimer = new DispatcherTimer();
            //mDispatcherTimer.Tick += new EventHandler(DispatcherTimerHandler);
            //mDispatcherTimer.Interval = TimeSpan.FromSeconds(0.2);
            //this.Loaded += new RoutedEventHandler(OnBaseDialogLoaded);
            //this.Closing += new CancelEventHandler(OnBaseDialogClosing);
        }

        //private void DispatcherTimerHandler(object sender, EventArgs e)
        //{
        //    this.mDispatcherTimer.Stop();
        //    this.Close();
        //}

        /// <summary>
        /// 窗口Load事件, 读取背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnBaseDialogLoaded(object sender, RoutedEventArgs e)
        {
            //var skinFile = FileUtil.GetRandomSkinFile(Settings.Default.SkinFolderPath);
            //DialogWindowBackgroundBrush = ImageUtil.LoadImageAsBrush(skinFile, 0.85, 0, (int)Width);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 动画形式执行关闭对话框
        /// </summary>
        /// <param name="callback">动画执行完之后的回调</param>
        protected void AnimateClose(EventHandler callback = null)
        {
            var storyBoardResource = FindResource("WindowFadeOutStoryboard") as Storyboard;
            var storyBoard = storyBoardResource.Clone();
            storyBoard.Completed += callback ?? new EventHandler((s, a) => Close());
            storyBoard.Begin(this);
        }
    }
}
