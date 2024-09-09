using AcgnuX.Controls;
using AcgnuX.Source.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Windows;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Index.xaml 的交互逻辑
    /// </summary>
    public partial class Index
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

        }
    }
}
