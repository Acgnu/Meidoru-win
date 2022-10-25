using AcgnuX.Controls;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Index.xaml 的交互逻辑
    /// </summary>
    public partial class Index : BasePage
    {
        public Index()
        {
            InitializeComponent();
        }

        private void LoadData(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //BusyIndicator mask = new BusyIndicator();
            //MainGrid.Children.Add(mask);//将Load添加到页面，会显示在最顶层
            //mask.IsBusy = true;
            //var _me = this;
            ////在线程中调用，否则会造成UI线程阻塞。
            //new System.Threading.Thread(() =>
            //{
            //    System.Threading.Thread.Sleep(2000);
            //    _me.Dispatcher.BeginInvoke((Action) delegate()
            //    {
            //        mask.IsBusy = false;
            //    });
            //}).Start();
        }
    }
}
