using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 设置的视图模型
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        public string AccountJsonPath { get; set; }
        public string PianoScorePath { get; set; }
        public string DbFilePath { get; set; }
        public string SkinFolderPath { get; set; }

        /// <summary>
        /// 账号密码文件存储的json完整路径
        /// </summary>
        public string AccountJsonPathView
        {
            get { return AccountJsonPath; }
            set
            {
                AccountJsonPath = value;
                RaisePropertyChanged();
            }
        }
        //乐谱文件目录
        public string PianoScorePathView
        {
            get { return PianoScorePath; }
            set
            {
                PianoScorePath = value;
                RaisePropertyChanged();
            }
        }
        //数据库文件
        public string DbFilePathView
        {
            get { return DbFilePath; }
            set
            {
                DbFilePath = value;
                RaisePropertyChanged();
            }
        }
        //皮肤目录
        public string SkinFolderPathView
        {
            get { return SkinFolderPath; }
            set
            {
                SkinFolderPath = value;
                RaisePropertyChanged();
            }
        }
        //IP代理数量
        private int _ProxyCount = 0;
        public int ProxyCount { get => _ProxyCount; set { _ProxyCount = value; RaisePropertyChanged(); } }
        //抓取规则
        public ObservableCollection<CrawlRuleViewModel> CrawlRuls { get; set; } = new ObservableCollection<CrawlRuleViewModel>();
        //是否全选
        public bool IsCheckedAll { get; set; }
        //checbox事件
        public ICommand OnCrawlRuleCheckboxClick { get; set; }


        public SettingsViewModel()
        {
            AccountJsonPath = Settings.Default.AccountFilePath ?? "";
            PianoScorePath = Settings.Default.Tan8HomeDir ?? "";
            DbFilePath = Settings.Default.DBFilePath ?? "";
            SkinFolderPath = Settings.Default.SkinFolderPath ?? "";
            //代理池数量变更监听
            ProxyCount = ProxyFactoryV2.GetProxyCount;
            ProxyFactoryV2.mProxyPoolCountChangeHandler += OnProxyPoolCountChange;
            OnCrawlRuleCheckboxClick = new RelayCommand<object>((sender) => OnGridCheckBoxClick(sender));
            InitCrawlRules();
            SQLite.OnDbFileSetEvent += InitCrawlRules;
        }

        /// <summary>
        /// 初始化抓取规则
        /// </summary>
        private void InitCrawlRules()
        {
            //从数据库读取规则
            var dataSet = SQLite.SqlTable("SELECT id, name, url, partten, max_page, exception_desc, enable FROM crawl_rules", null);
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
                    ExceptionDesc = dataRow["exception_desc"].ToString(),
                    Enable = Convert.ToByte(dataRow["enable"]),
                });
            }
            CheckIsCheckedAll(false);
        }

        /// <summary>
        /// 表格checkbox点击事件
        /// </summary>
        /// <param name="sender"></param>
        private void OnGridCheckBoxClick(object sender)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.Tag.Equals("0"))
            {
                //全选
                foreach (var crawlRule in CrawlRuls)
                {
                    crawlRule.Enable = checkBox.IsChecked.GetValueOrDefault() ? Convert.ToByte(1) : Convert.ToByte(0);
                }
                //for (int i = 0; i < mCrawlRulsDataGrid.Items.Count; i++)
                //{
                //    //获取行
                //    DataGridRow neddrow = (DataGridRow)this.mCrawlRulsDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                //    //获取该行的某列
                //    CheckBox cb = (CheckBox)this.mCrawlRulsDataGrid.Columns[mCrawlRulsDataGrid.Columns.Count - 1].GetCellContent(neddrow);
                //    cb.IsChecked = checkBox.IsChecked;
                //}
                UpdateCrawlRulesEnable(null, checkBox.IsChecked.GetValueOrDefault());
            }
            else
            {
                //单选
                UpdateCrawlRulesEnable(Convert.ToInt32(checkBox.Tag), checkBox.IsChecked.GetValueOrDefault());
                CheckIsCheckedAll(true);
            }
            ProxyFactoryV2.RestartCrawlIPService();
        }

        /// <summary>
        /// IP代理池数量变化通知
        /// </summary>
        /// <param name="curNum"></param>
        private void OnProxyPoolCountChange(int curNum)
        {
            ProxyCount = curNum;
        }

        /// <summary>
        /// 更新规则启用状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="enable"></param>
        private void UpdateCrawlRulesEnable(int? id, bool enable)
        {
            SQLite.ExecuteNonQuery(string.Format("UPDATE crawl_rules SET enable = {0} {1}",
                enable ? 1 : 0,
                null == id ? "" : " WHERE id = " + id), null);
        }
        
        /// <summary>
        /// 检查是否全部选中
        /// </summary>
        /// <param name="doNotify"></param>
        public void CheckIsCheckedAll(bool doNotify)
        {
            IsCheckedAll = CrawlRuls.Where(item => item.Enable == 1).Count() == CrawlRuls.Count();
            if(doNotify) RaisePropertyChanged(nameof(IsCheckedAll));
        }
    }
}
