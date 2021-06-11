using AcgnuX.Pages;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

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
            if(0 == McrawlRule.Id)
            {
                SaveCrawlRule(McrawlRule);
            }
            else
            {
                ModifyCrawlRule(McrawlRule);
            }
            button.IsEnabled = true;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 新增抓取规则
        /// </summary>
        /// <param name="crawlRule"></param>
        /// <returns></returns>
        private int SaveCrawlRule(CrawlRuleViewModel crawlRule)
        {
            var row = SQLite.ExecuteNonQuery(string.Format("INSERT INTO crawl_rules(ID, NAME, URL, PARTTEN, MAX_PAGE, ENABLE) VALUES ((SELECT MAX(ID)+1 FROM crawl_rules), '{0}', '{1}', '{2}', {3}, {4})",
                crawlRule.Name,
                crawlRule.Url,
                crawlRule.Partten,
                crawlRule.MaxPage,
                crawlRule.Enable));
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
            return row;
        }

        private void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}