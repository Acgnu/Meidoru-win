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
    public class SettingsViewModel : Settings
    {
        /// <summary>
        /// 账号密码文件存储的json完整路径
        /// </summary>
        public string AccountJsonPathView
        {
            get { return AccountJsonPath; }
            set
            {
                AccountJsonPath = value;
                OnPropertyChanged(nameof(AccountJsonPath));
            }
        }
        //乐谱文件目录
        public string PianoScorePathView
        {
            get { return PianoScorePath; }
            set
            {
                PianoScorePath = value;
                OnPropertyChanged(nameof(PianoScorePath));
            }
        }
        //数据库文件
        public string DbFilePathView
        {
            get { return DbFilePath; }
            set
            {
                DbFilePath = value;
                OnPropertyChanged(nameof(DbFilePath));
            }
        }
        //IP代理数量
        public int ProxyCount { get; set; }
        //抓取规则
        public ObservableCollection<CrawlRuleViewModel> CrawlRuls { get; set; } = new ObservableCollection<CrawlRuleViewModel>();
        //同步配置
        public ObservableCollection<SyncConfigViewModel> SyncConfigs { get; set; } = new ObservableCollection<SyncConfigViewModel>();
        //是否全选
        public bool IsCheckedAll { get; set; }
        public bool IsSyncConfigCheckedAll { get; set; }
        //checbox事件
        public ICommand OnCrawlRuleCheckboxClick { get; set; }
        public ICommand OnSyncConfigCheckboxClick { get; set; }

        public SettingsViewModel()
        {
            AccountJsonPath = ConfigUtil.Instance.AccountJsonPath ?? "";
            PianoScorePath = ConfigUtil.Instance.PianoScorePath ?? "";
            DbFilePath = ConfigUtil.Instance.DbFilePath ?? "";
            //代理池数量变更监听
            ProxyCount = ProxyFactoryV2.GetProxyCount;
            ProxyFactoryV2.mProxyPoolCountChangeHandler += OnProxyPoolCountChange;
            OnCrawlRuleCheckboxClick = new RelayCommand<object>((sender) => OnGridCheckBoxClick(sender));
            OnSyncConfigCheckboxClick = new RelayCommand<object>((sender) => OnSyncConfigGridCheckBoxClick(sender));
            InitCrawlRules();
            InitSyncConfig();
            SQLite.OnDbFileSetEvent += InitCrawlRules;
        }

        /// <summary>
        /// 初始化抓取规则
        /// </summary>
        private void InitCrawlRules()
        {
            //从数据库读取规则
            var dataSet = SQLite.SqlTable("SELECT id, name, url, partten, max_page, enable FROM crawl_rules", null);
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
            CheckIsCheckedAll(false);
        }

        /// <summary>
        /// 初始同步配置
        /// </summary>
        private void InitSyncConfig()
        {
            //从数据库读取
            var dataSet = SQLite.SqlTable("SELECT id, pc_path, mobile_path, enable FROM media_sync_config", null);
            if (null == dataSet) return;
            //封装进对象
            foreach (DataRow dataRow in dataSet.Rows)
            {
                SyncConfigs.Add(new SyncConfigViewModel()
                {
                    Id = Convert.ToInt32(dataRow["id"]),
                    PcPath = Convert.ToString(dataRow["pc_path"]),
                    MobilePath = Convert.ToString(dataRow["mobile_path"]),
                    Enable = Convert.ToByte(dataRow["enable"])
                });
            }
            CheckSyncConfigIsCheckedAll(false);
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
                    crawlRule.EnableView = checkBox.IsChecked.GetValueOrDefault() ? Convert.ToByte(1) : Convert.ToByte(0);
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
        /// 同步配置表格checkbox点击事件
        /// </summary>
        /// <param name="sender"></param>
        private void OnSyncConfigGridCheckBoxClick(object sender)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.Tag.Equals("0"))
            {
                //全选
                foreach (var syncConfig in SyncConfigs)
                {
                    syncConfig.EnableView = checkBox.IsChecked.GetValueOrDefault() ? Convert.ToByte(1) : Convert.ToByte(0);
                }
                UpdateSyncConfigEnable(null, checkBox.IsChecked.GetValueOrDefault());
            }
            else
            {
                //单选
                UpdateSyncConfigEnable(Convert.ToInt32(checkBox.Tag), checkBox.IsChecked.GetValueOrDefault());
                CheckSyncConfigIsCheckedAll(true);
            }
        }

        /// <summary>
        /// IP代理池数量变化通知
        /// </summary>
        /// <param name="curNum"></param>
        private void OnProxyPoolCountChange(int curNum)
        {
            ProxyCount = curNum;
            OnPropertyChanged(nameof(ProxyCount));
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
        /// 更新同步配置启用状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="enable"></param>
        private void UpdateSyncConfigEnable(int? id, bool enable)
        {
            SQLite.ExecuteNonQuery(string.Format("UPDATE media_sync_config SET enable = {0} {1}",
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
            if(doNotify) OnPropertyChanged(nameof(IsCheckedAll));
        }

        /// <summary>
        /// 检查同步配置是否全部选中
        /// </summary>
        /// <param name="doNotify"></param>
        public void CheckSyncConfigIsCheckedAll(bool doNotify)
        {
            IsSyncConfigCheckedAll = SyncConfigs.Where(item => item.Enable == 1).Count() == SyncConfigs.Count();
            if(doNotify) OnPropertyChanged(nameof(IsSyncConfigCheckedAll));
        }
    }
}
