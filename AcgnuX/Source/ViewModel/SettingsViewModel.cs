using AcgnuX.Properties;
using AcgnuX.Source.Taskx;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedLib.Utils;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using SharedLib.Data;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 设置的视图模型
    /// </summary>
    public class SettingsViewModel : ObservableObject
    {
        /// <summary>
        /// 账号文件
        /// </summary>
        public string AccountJsonPath 
        { 
            get => Settings.Default.AccountFilePath; 
            set => SetProperty(Settings.Default.AccountFilePath, value, Settings.Default, (s, v) => { s.AccountFilePath = v; s.Save(); }); 
        }
        /// <summary>
        /// 乐谱目录
        /// </summary>
        public string PianoScorePath 
        {
            get => Settings.Default.Tan8HomeDir;
            set => SetProperty(Settings.Default.Tan8HomeDir, value, Settings.Default, (s, v) => { s.Tan8HomeDir = v; s.Save(); });
        }

        /// <summary>
        /// 数据库文件目录
        /// </summary>
        public string DbFilePath
        {
            get => Settings.Default.DBFilePath;
            set => SetProperty(Settings.Default.DBFilePath, value, Settings.Default, (s, v) => { s.DBFilePath = v; s.Save(); });
        }
        /// <summary>
        /// 皮肤目录
        /// </summary>
        public string SkinFolderPath
        {
            get => Settings.Default.SkinFolderPath;
            set => SetProperty(Settings.Default.SkinFolderPath, value, Settings.Default, (s, v) => { s.SkinFolderPath = v; s.Save(); });
        }

        //IP代理数量
        private int _proxyCount = 0;
        public int ProxyCount { get => _proxyCount; set => SetProperty(ref _proxyCount, value); }
        //抓取规则
        public ObservableCollection<CrawlRuleViewModel> CrawlRuls { get; set; } = new ObservableCollection<CrawlRuleViewModel>();
        //是否全选
        public bool IsCheckedAll { get; set; }
        //checbox事件
        public ICommand OnCrawlRuleCheckboxClick { get; set; }

        private readonly ProxyFactoryV2 _ProxyFactoryV2;
        private readonly CrawlRuleRepo _CrawlRuleRepo;

        public SettingsViewModel(ProxyFactoryV2 proxyFactoryV2, CrawlRuleRepo crawlRuleRepo)
        {
            _CrawlRuleRepo = crawlRuleRepo;
            _ProxyFactoryV2 = proxyFactoryV2;
            //代理池数量变更监听
            ProxyCount = _ProxyFactoryV2.GetProxyCount;
            //IP代理池数量变化通知
            _ProxyFactoryV2.mProxyPoolCountChangeHandler += new Action<int>((curNum) => ProxyCount = curNum);
            OnCrawlRuleCheckboxClick = new RelayCommand<object>((sender) => OnGridCheckBoxClick(sender));
            SQLite.OnDbFileSetEvent += LoadCrawlRule;
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
                _CrawlRuleRepo.UpdateCrawlRulesEnable(null, checkBox.IsChecked.GetValueOrDefault());
            }
            else
            {
                //单选
                _CrawlRuleRepo.UpdateCrawlRulesEnable(Convert.ToInt32(checkBox.Tag), checkBox.IsChecked.GetValueOrDefault());
                CheckIsCheckedAll(true);
            }
            _ProxyFactoryV2.RestartCrawlIPService();
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
        /// 读取抓取规则
        /// </summary>
        internal async void LoadCrawlRule()
        {
            CrawlRuls.Clear();
            var result = await _CrawlRuleRepo.FindCrawlRuleAsync();
            foreach (var rule in result)
            {
                CrawlRuls.Add(new CrawlRuleViewModel(rule));
            }
            CheckIsCheckedAll(false);
        }

        /// <summary>
        /// 删除抓取规则
        /// </summary>
        /// <param name="item"></param>
        internal void DeleteCrawlRule(CrawlRuleViewModel item)
        {
            _CrawlRuleRepo.Delete(item.Id);
            CrawlRuls.Remove(item);
            CheckIsCheckedAll(true);
        }
    }
}
