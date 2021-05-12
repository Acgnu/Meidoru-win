using AcgnuX.Pages;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.WindowX.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    class CrawlConfigViewModel : BasePropertyChangeNotifyModel
    {
        public ObservableCollection<CrawlRuleViewModel> CrawlRuls { get; set; } = new ObservableCollection<CrawlRuleViewModel>();
        private DataGrid mCrawlRulsDataGrid;
        //private Settings mHostPage;
        public int ProxyCount { get; set; }
        public int ProxyCountAndView
        {
            get { return ProxyCount; }
            set
            {
                ProxyCount = value;
                OnPropertyChanged(nameof(ProxyCount));
            }
        }

        public CrawlConfigViewModel(Settings hostPage)
        {
            //mHostPage = hostPage;
            //获取规则表格对象
            mCrawlRulsDataGrid = hostPage.CrawlConfigDataGrid;
            //设置数据源
            mCrawlRulsDataGrid.ItemsSource = CrawlRuls;
            //设置内容双击事件
            mCrawlRulsDataGrid.MouseDoubleClick += MCrawlRulsDataGrid_MouseDoubleClick;
            //表格右键展开菜单事件
            mCrawlRulsDataGrid.MouseRightButtonDown += OnItemMouseRightClick;
            //表格上下文菜单
            //(mCrawlRulsDataGrid.ContextMenu.Items[0] as MenuItem).Click += OnDeleteContextMenuClick;
            //添加按钮
            hostPage.AddCrawlButton.Click += OnAddCrawlClick;
            //代理池数量变更监听
            ProxyCount = ProxyFactory.GetProxyCount();
            ProxyFactory.mProxyPoolCountChangeHandler += OnProxyPoolCountChange;
            hostPage.ProxyCountTextBlock.DataContext = this;
        }

        /// <summary>
        /// 行双击选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MCrawlRulsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (null == mCrawlRulsDataGrid.SelectedItem) return;
            //打开修改对话框
            var dialog = new EditCrawlDialog(mCrawlRulsDataGrid.SelectedItem as CrawlRuleViewModel);
            var result = dialog.ShowDialog();
            if(result.GetValueOrDefault() == true)
            {
                //从对话框中删除旧的数据, 把新的数据添加到对应行
                var current = CrawlRuls.IndexOf(dialog.McrawlRule);
                CrawlRuls.RemoveAt(current);
                CrawlRuls.Insert(current, dialog.McrawlRule);
            }
        }

        /// <summary>
        /// 从数据库读取规则
        /// </summary>
        public void LoadRules()
        {
            CrawlRuls.Clear();
            var dataSet = SQLite.SqlTable("SELECT id, name, url, partten, max_page, enable FROM crawl_rules");
            if (null == dataSet) return;
            //封装进对象
            foreach (DataRow dataRow in dataSet.Rows)
            {
                CrawlRuls.Add(new CrawlRuleViewModel()
                {
                    Id = Convert.ToInt32(dataRow["id"]),
                    Name = Convert.ToString(dataRow["name"]),
                    Url = Convert.ToString(dataRow["url"]),
                    Partten = Convert.ToString(dataRow["partten"]),
                    MaxPage = Convert.ToInt32(dataRow["max_page"]),
                    Enable = Convert.ToByte(dataRow["enable"]),
                });
            }
        }

        /// <summary>
        /// 添加规则按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddCrawlClick(object sender, RoutedEventArgs e)
        {
            //打开修改对话框
            var dialog = new EditCrawlDialog(null);
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault() == true)
            {
                CrawlRuls.Add(dialog.McrawlRule);
            }
        }

        /// <summary>
        /// 规则删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemMouseRightClick(object sender, MouseButtonEventArgs e)
        {
            XamlUtil.SelectRow(mCrawlRulsDataGrid, e);
            var selected = mCrawlRulsDataGrid.SelectedItem as CrawlRuleViewModel;
            if (null == selected) return;
            //删除对话框
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format((string)Application.Current.FindResource("DeleteConfirm"), string.Format("{0}", selected.Name))).ShowDialog();
            if (result.GetValueOrDefault())
            {
                SQLite.ExecuteNonQuery(string.Format("DELETE FROM crawl_rules WHERE ID = {0}", selected.Id));
                CrawlRuls.Remove(selected);
            }
        }

        /// <summary>
        /// IP代理池数量变化通知
        /// </summary>
        /// <param name="curNum"></param>
        private void OnProxyPoolCountChange(int curNum)
        {
            ProxyCountAndView = curNum;
        }
    }
}
