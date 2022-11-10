using AcgnuX.Controls;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using GalaSoft.MvvmLight.Messaging;
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
            //BubbleTipViwerViewModel = new BubbleTipViwerViewModel();
            DataContext = this;
        }

        private void LoadData(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send(new BubbleTipViewModel
            {
                AlertLevel = Source.Bussiness.Constants.AlertLevel.RUN,
                Text="Running"
            });
            Messenger.Default.Send(new BubbleTipViewModel
            {
                AlertLevel = Source.Bussiness.Constants.AlertLevel.INFO,
                Text= "Info Message"
            });
            Messenger.Default.Send(new BubbleTipViewModel
            {
                AlertLevel = Source.Bussiness.Constants.AlertLevel.WARN,
                Text= "Warning Message Test"
            });
            Messenger.Default.Send(new BubbleTipViewModel
            {
                AlertLevel = Source.Bussiness.Constants.AlertLevel.ERROR,
                Text= "Error Message show AAAAAAAAAAAAAAAAAAAAAAAA"
            });
        }
    }
}
