using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using SharedLib.Data;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑账号的弹窗
    /// </summary>
    public partial class EditCrawlDialog : BaseDialog
    {
        //视图对象
        public CrawlRuleViewModel ContentViewModel { get; }

        private ProxyFactoryV2 _ProxyFactoryV2 { get; }
        private CrawlRuleRepo _CrawlRuleRepo { get; }

        public EditCrawlDialog(CrawlRuleViewModel crawlRule)
        {
            InitializeComponent();
            ContentViewModel = crawlRule;
            _ProxyFactoryV2 = App.Current.Services.GetService<ProxyFactoryV2>();
            _CrawlRuleRepo = App.Current.Services.GetService<CrawlRuleRepo>();
            DataContext = this;
            FormStackPanel.BindingGroup.BeginEdit();
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

            if (!FormStackPanel.BindingGroup.CommitEdit())
            {
                button.IsEnabled = true;
                return;
            }

            if (0 == ContentViewModel.Id)
            {
                _CrawlRuleRepo.Add(ContentViewModel._CrawlRule);
            }
            else
            {
                _CrawlRuleRepo.Update(ContentViewModel._CrawlRule);
            }

            if (ContentViewModel.Enable == Convert.ToByte(1))
            {
                _ProxyFactoryV2.RestartCrawlIPService();
            }
            AnimateClose((s, a) => DialogResult = true);
        }

        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                WindowUtil.ShowBubbleError(e.Error.ErrorContent.ToString());
            }
        }
    }
}