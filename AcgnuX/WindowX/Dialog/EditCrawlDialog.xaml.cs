using AcgnuX.Pages;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑账号的弹窗
    /// </summary>
    public partial class EditCrawlDialog : BaseDialog {
        //视图对象
        public CrawlRuleViewModel McrawlRule { get; set; } = new CrawlRuleViewModel()
        {
            MaxPage = 1,
            Enable = 1
        };

        public EditCrawlDialog(CrawlRuleViewModel crawlRule)
        {
            InitializeComponent();
            DataContext = this;
            if (null != crawlRule)
            {
                McrawlRule = crawlRule;
            }
        }

        /// <summary>
        /// 保存按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            int successRow;
            ManualTriggerSourceUpdate();
            if (0 == McrawlRule.Id)
            {
                successRow = SaveCrawlRule(McrawlRule);
            }
            else
            {
                successRow = ModifyCrawlRule(McrawlRule);
            }
            if (successRow > 0)
            {
                DialogResult = true;
            }
            button.IsEnabled = true;
            Close();
        }

        /// <summary>
        /// 手动更新源
        /// </summary>
        private void ManualTriggerSourceUpdate()
        {
            //手动表格更新vm
            BindingExpression binding = TextBlockName.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = TextBlockSite.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = TextBlockParttten.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = TextBlockPage.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = CheckboxEnable.GetBindingExpression(CheckBox.IsCheckedProperty);
            binding.UpdateSource();
        }

        /// <summary>
        /// 新增抓取规则
        /// </summary>
        /// <param name="crawlRule"></param>
        /// <returns></returns>
        private int SaveCrawlRule(CrawlRuleViewModel crawlRule)
        {
            if(string.IsNullOrEmpty(crawlRule.Name) || string.IsNullOrEmpty(crawlRule.Url) || string.IsNullOrEmpty(crawlRule.Partten))
            {
                return 0;
            }
            var row = SQLite.ExecuteNonQuery(string.Format("INSERT INTO crawl_rules(ID, NAME, URL, PARTTEN, MAX_PAGE, ENABLE) VALUES ((SELECT MAX(ID)+1 FROM crawl_rules), '{0}', '{1}', '{2}', {3}, {4})",
                crawlRule.Name,
                crawlRule.Url,
                crawlRule.Partten,
                crawlRule.MaxPage,
                crawlRule.Enable));
           
            if(row > 0)
            {
                //查询最新添加的记录ID
                var newID = SQLite.sqlone("SELECT MAX(id) FROM crawl_rules");
                crawlRule.Id = Convert.ToInt32(newID);
            }
            if(crawlRule.Enable == Convert.ToByte(1))
            {
                ProxyFactory.RestartCrawlIPTask();
            }
            return row;
        }

        /// <summary>
        /// 修改抓取规则
        /// </summary>
        /// <param name="crawlRule"></param>
        /// <returns></returns>
        private int ModifyCrawlRule(CrawlRuleViewModel crawlRule)
        {
            var row = SQLite.ExecuteNonQuery(string.Format("UPDATE crawl_rules SET NAME = '{0}', URL = '{1}', PARTTEN = '{2}', MAX_PAGE = {3}, ENABLE = {4} WHERE ID = {5}",
                crawlRule.Name,
                crawlRule.Url,
                crawlRule.Partten,
                crawlRule.MaxPage,
                crawlRule.Enable,
                crawlRule.Id));
            if (crawlRule.Enable == Convert.ToByte(1))
            {
                ProxyFactory.RestartCrawlIPTask();
            }
            return row;
        }

        private void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}