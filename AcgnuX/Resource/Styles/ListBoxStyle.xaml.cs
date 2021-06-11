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
    partial class ListBoxStyle : ResourceDictionary
    {
        //弹琴吧播放器实例
        private Tan8PlayerWindow mTan8Player;
        //ListBox实例
        private ListBox pianoScoreListBox;

        public ListBoxStyle()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 琴谱页播放按钮点击事件
        /// 打开弹琴吧播放器并播放选中的曲谱
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnTan8PlayButtonClick(Object sender, RoutedEventArgs e)
        {
            //事件按钮对象
            var eventButton = (Button)sender;

            //如果ListBox没有初始化, 则根据触发节点寻找
            if(null == pianoScoreListBox)
            {
                DependencyObject dep = (DependencyObject)e.OriginalSource;
                while ((dep != null) && !(dep is ListBox))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                if (dep == null) return;
                //找到之后赋值给成员对象
                pianoScoreListBox = dep as ListBox;
            }

            //根据触发按钮获取点击的行
            var selected = ((ListBoxItem)pianoScoreListBox.ContainerFromElement(eventButton)).Content;
            //手动选中行
            pianoScoreListBox.SelectedItem = selected;

            //播放器未打开, 则创建一个新的播放器
            if (null == mTan8Player)
            {
                mTan8Player = new Tan8PlayerWindow();
                //打开后将对象赋值给ListBox的TAG, 方便主窗口获取
                pianoScoreListBox.Tag = mTan8Player;
            }
            //播放所选曲谱
            mTan8Player.Show();
            mTan8Player.PlaySelected(selected as PianoScore);
        }

        //private void ListBoxItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        //{
        //    e.Handled = true;
        //}

        //private Point myMousePlacementPoint;

        //private void OnListViewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.MiddleButton == MouseButtonState.Pressed)
        //    {
        //        myMousePlacementPoint = Application.Current.MainWindow.PointToScreen(Mouse.GetPosition(Application.Current.MainWindow));
        //    }
        //}

        //private void OnListViewMouseMove(object sender, MouseEventArgs e)
        //{
        //    if(null == pianoScoreListBox)
        //    {
        //        return;
        //    }
        //    ScrollViewer scrollViewer = GetScrollViewer(pianoScoreListBox) as ScrollViewer;

        //    if (e.MiddleButton == MouseButtonState.Pressed)
        //    {
        //        var currentPoint = Application.Current.MainWindow.PointToScreen(Mouse.GetPosition(Application.Current.MainWindow));

        //        if (currentPoint.Y < myMousePlacementPoint.Y)
        //        {
        //            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 3);
        //        }
        //        else if (currentPoint.Y > myMousePlacementPoint.Y)
        //        {
        //            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 3);
        //        }

        //        if (currentPoint.X < myMousePlacementPoint.X)
        //        {
        //            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 3);
        //        }
        //        else if (currentPoint.X > myMousePlacementPoint.X)
        //        {
        //            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 3);
        //        }
        //    }
        //}

        //public static DependencyObject GetScrollViewer(DependencyObject o)
        //{
        //    // Return the DependencyObject if it is a ScrollViewer
        //    if (o is ScrollViewer)
        //    { return o; }

        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(o, i);

        //        var result = GetScrollViewer(child);
        //        if (result == null)
        //        {
        //            continue;
        //        }
        //        else
        //        {
        //            return result;
        //        }
        //    }
        //    return null;
        //}
    }
}
