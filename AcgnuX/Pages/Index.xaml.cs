using AcgnuX.Controls;
using AcgnuX.Source.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Index.xaml 的交互逻辑
    /// </summary>
    public partial class Index
    {
        public bool Bv { get; set; } = true;
        public Index()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoadData(object sender, RoutedEventArgs e)
        {
        }

        public void Button_Click_1(object sender, RoutedEventArgs e)
        {
        }
    }
}
