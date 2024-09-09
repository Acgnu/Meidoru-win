using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.ViewModel;
using AcgnuX.Properties;
using System;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using AcgnuX.Source.Utils;

namespace AcgnuX
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetService<MainWindowViewModel>();
            DataContext = ViewModel;
        }

        /// <summary>
        /// 主窗口加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //如果没有配置数据库, 提示错误
            if (string.IsNullOrEmpty(Settings.Default.DBFilePath))
            {
                WindowUtil.ShowBubbleWarn("没有配置数据库文件路径, 部分功能将无法正常使用");
            }

            await Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
            {
                ViewModel.InitBackgroundBrush(Width);
            });
        }
    }
}
