using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Resources;

namespace AcgnuX.Source.Utils
{
    public static class XamlUtil
    {
        /// <summary>
        /// 选中DataGrid中的行
        /// 如果在表格中有其他响应鼠标事件的组件, 会导致grid的row无法响应
        /// 此处使用响应的组件找到其所属的行并选中
        /// </summary>
        /// <param name="grid">表格</param>
        /// <param name="e">事件</param>
        public static void SelectRow(DataGrid grid, RoutedEventArgs e)
        {
            grid.SelectedItem = null;
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null) return;

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                cell.Focus();

                while ((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                DataGridRow row = dep as DataGridRow;
                grid.SelectedItem = row.DataContext;
            }
        }

        /// <summary>
        /// 判断一个滚动条是否触底
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <returns></returns>
        public static bool IsScrollToBottom(ScrollViewer scrollViewer)
        {
            var viewH = scrollViewer.ViewportHeight;
            var scrollOffset = scrollViewer.VerticalOffset;
            var extH = scrollViewer.ExtentHeight;

            if (viewH == 0) return false;

            return viewH + scrollOffset == extH;
        }

        public static ListView GetParentListView(RoutedEventArgs e)
        {
            ListView parentView = null;
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListView))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep != null)
            {
                //找到之后赋值给成员对象
                parentView = dep as ListView;
            }
            return parentView;
        }

        /// <summary>
        /// 以文本方式读取资源文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetApplicationResourceAsString(string path)
        {
            var uri = new Uri(path, UriKind.Relative);
            var info = System.Windows.Application.GetResourceStream(uri);
            StreamReader reader = new StreamReader(info.Stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            info.Stream.Close();
            return text;
        }

        /// <summary>
        /// 以流的方式读取资源文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static StreamResourceInfo GetApplicationResourceAsStream(string path)
        {
            var uri = new Uri(path, UriKind.Relative);
            var info = System.Windows.Application.GetResourceStream(uri);
            return info;
        }
    }
}
