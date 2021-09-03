using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AcgnuX.WindowX
{
    /// <summary>
    /// Tan8DownloadRecordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Tan8DownloadRecordWindow : BaseDialog
    {
        //标识条件复选框是否初始化完成
        private bool IsCheckBoxInited = false;
        /// <summary>
        /// 启动下载事件
        /// </summary>
        public event EditConfirmHandler<PianoScore> editConfirmHnadler;

        public Tan8DownloadRecordWindow()
        {
            InitializeComponent();
            IsCheckBoxInited = true;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadDownloadRecord();
        }
        /// <summary>
        /// 加载曲谱下载记录
        /// </summary>
        private void LoadDownloadRecord()
        {
            //查询条件
            var condition = new StringBuilder();
            var context = DataContext as PianoScoreDownloadRecordViewModel;
            var downloadList = context.DownloadRecordList;
            var filterBox = context.FilterBoxList;
            //循环容器获取选中的项
            var checkedBox = filterBox.Where(box => box.IsChecked);
            foreach (var item in checkedBox)
            {
                condition.Append(item.Value).Append(",");
            }
            downloadList.Clear();
            //一个都没选则返回
            if (condition.Length == 0) return;
            condition.Length -= 1;

            var dataSet = SQLite.SqlTable(string.Format("SELECT id, ypid, name, strftime('%Y-%m-%d %H:%M:%S', create_time) create_time, result FROM tan8_music_down_record WHERE code in({0}) ORDER BY id DESC LIMIT 100", condition.ToString()), null);
            if (null == dataSet) return;
            //封装进对象
            foreach (DataRow dataRow in dataSet.Rows)
            {
                //拼接得到cover路径
                downloadList.Add(new Tan8SheetDownloadRecord()
                {
                    Id = Convert.ToInt32(dataRow["id"]),
                    Ypid = Convert.ToInt32(dataRow["ypid"]),
                    Name = Convert.ToString(dataRow["name"]),
                    Create_time = Convert.ToString(dataRow["create_time"]),
                    Result = Convert.ToString(dataRow["result"])
                });
            }
        }

        /// <summary>
        /// 下载记录条件复选框点击事件
        /// 重新加载记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadRecordConditionCheckboxClick(object sender, RoutedEventArgs e)
        {
            //由于页面创建的时候就会触发选中事件, 此处手动限制, 只有在所有数据加载之后才会触发此事件
            if (!IsCheckBoxInited) return;
            LoadDownloadRecord();
        }

        /// <summary>
        /// 下载记录键盘按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadRecordGridKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Delete == e.Key)
            {
                BatchDeleteDownloadRecord();
            }
        }

        /// <summary>
        /// 上下文菜单删除点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeleteContextMenuClick(object sender, RoutedEventArgs e)
        {
            BatchDeleteDownloadRecord();
        }

        /// <summary>
        /// 以递增方式复制ID
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCopyIncrIdContextMenuClick(object sender, RoutedEventArgs e)
        {
            //XamlUtil.SelectRow(DownloadRecordDataGrid, e);
            var record = DownloadRecordDataGrid.SelectedItem as Tan8SheetDownloadRecord;
            if (null == record) return;
            var pianoScore = new PianoScore()
            {
                autoDownload = true,
                id = record.Ypid + 1
            };
            editConfirmHnadler?.Invoke(pianoScore);
        }

        /// <summary>
        /// 批量删除下载记录
        /// </summary>
        private void BatchDeleteDownloadRecord()
        {
            //没有选中的直接返回
            if (DownloadRecordDataGrid.SelectedItems.Count == 0) return;
            //获取选中的ID, 一次性删除
            var ids = "";
            var context = DataContext as PianoScoreDownloadRecordViewModel;
            var downloadList = context.DownloadRecordList;
            while (DownloadRecordDataGrid.SelectedItems.Count > 0)
            {
                //拼接ID
                var record = DownloadRecordDataGrid.SelectedItems[0] as Tan8SheetDownloadRecord;
                ids += "," + record.Id;
                //从集合中移除
                downloadList.Remove(record);
            }
            if (ids.Length == 0) return;
            //删除数据库中的记录
            SQLite.ExecuteNonQuery(string.Format("DELETE FROM tan8_music_down_record WHERE ID IN ({0})", ids.Substring(1)), null);
        }

        /// <summary>
        /// 弹8曲谱下载完成事件
        /// </summary>
        /// <param name="pianoScore"></param>
        public void OnTan8SheetDownloadFinish(PianoScore pianoScore)
        {
            Dispatcher.Invoke(() =>
            {
                var dataRow = SQLite.SqlRow(string.Format("SELECT id, ypid, name, strftime('%Y-%m-%d %H:%M:%S', create_time) create_time, result  FROM tan8_music_down_record WHERE ypid = {0} ORDER BY create_time DESC LIMIT 1", pianoScore.id));
                var context = DataContext as PianoScoreDownloadRecordViewModel;
                var downloadList = context.DownloadRecordList;
                downloadList.Insert(0, new Tan8SheetDownloadRecord()
                {
                    Id = Convert.ToInt32(dataRow[0]),
                    Ypid = Convert.ToInt32(dataRow[1]),
                    Name = Convert.ToString(dataRow[2]),
                    Create_time = Convert.ToString(dataRow[3]),
                    Result = Convert.ToString(dataRow[4])
                });
            });
        }

        /// <summary>
        /// 重写窗口关闭事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;  //标识取消close
            this.Hide();      // 隐藏, 方便再次调用show()
        }
    }
}
