using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.WindowX.Dialog;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.Pages
{
    /// <summary>
    ///onOpenContextMenuxaml 的交互逻辑
    /// </summary>
    public partial class DnsManage
    {
        public DnsManageViewModel ViewModel { get; }

        public DnsManage()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetService<DnsManageViewModel>();
            DataContext = ViewModel;
        }

        /// <summary>
        /// 右键删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickContextMenuDelete(object sender, MouseButtonEventArgs e)
        {
            XamlUtil.SelectRow(DnsRecordDataGrid, e);
            var selected = DnsRecordDataGrid.SelectedItem as DnsItemViewModel;
            if (null == selected) return;
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, selected.Name)).ShowDialog();
            if (result.GetValueOrDefault())
            {
                ViewModel.DeleteItem(selected);
            }
        }

        /// <summary>
        /// 表格内容双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGridDoubleClick(object sender, RoutedEventArgs e)
        {
            var selected = DnsRecordDataGrid.SelectedItem as DnsItemViewModel;
            if (null == selected) return;
            new EditDnsRecordDialog(selected).ShowDialog();
        }

        /// <summary>
        /// 新增按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAddClick(object sender, RoutedEventArgs e)
        {
            if (null == ViewModel._AlidnsClient)
            {
                WindowUtil.ShowBubbleError("未配置访问密钥");
                return;
            }
            var result = new EditDnsRecordDialog(new DnsItemViewModel()
            {
                _AlidnsClient = ViewModel._AlidnsClient
            }).ShowDialog();
            if (result.GetValueOrDefault())
            {
                ViewModel.Load(true);
            }
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Load(false);
        }
    }
}
