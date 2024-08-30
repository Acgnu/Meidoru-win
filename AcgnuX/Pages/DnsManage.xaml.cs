using AcgnuX.Bussiness.Ten.Dns;
using AcgnuX.Model.Ten.Dns;
using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Bussiness.Ten.Dns;
using AcgnuX.Source.Model;
using AcgnuX.Source.Model.Ten.Dns;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcgnuX.Pages
{
    /// <summary>
    ///onOpenContextMenuxaml 的交互逻辑
    /// </summary>
    public partial class DnsManage : BasePage
    {
        private readonly DnsManageViewModel _ViewModel;

        public DnsManage()
        {
            InitializeComponent();
            _ViewModel = DataContext as DnsManageViewModel;
        }

        /// <summary>
        /// 右键删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickContextMenuDelete(object sender, MouseButtonEventArgs e)
        {
            XamlUtil.SelectRow(DnsRecordDataGrid, e);
            var selected = DnsRecordDataGrid.SelectedItem as DnsItemViewModel;
            if (null == selected) return;
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, selected.Name)).ShowDialog();
            if (result.GetValueOrDefault())
            {
                await selected.Delete();
                _ViewModel.OnRefreshClick();
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
            new EditDnsRecordDialog(selected, _ViewModel.OnRefreshClick).ShowDialog();
        }

        /// <summary>
        /// 新增按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAddClick(object sender, RoutedEventArgs e)
        {
            if (null == _ViewModel.TenDnsClient)
            {
                Messenger.Default.Send(new BubbleTipViewModel
                {
                    AlertLevel = AlertLevel.ERROR,
                    Text = "未配置访问密钥"
                });
                return;
            }
            new EditDnsRecordDialog(new DnsItemViewModel()
            {
                Line = "默认",
                TenDnsClient = _ViewModel.TenDnsClient
            }, _ViewModel.OnRefreshClick).ShowDialog();
        }
    }
}
