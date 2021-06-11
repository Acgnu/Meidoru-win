using AcgnuX.Source.Model;
using AcgnuX.WindowX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AcgnuX.Resource.Styles
{
    partial class ListViewStyle : ResourceDictionary
    {
        public ListViewStyle()
        {
            InitializeComponent();
        }

        private void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void ImgItemRightClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("hahaha ");
        }
    }
}
