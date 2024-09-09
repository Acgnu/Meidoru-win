using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.ViewModel;
using AcgnuX.WindowX.Dialog;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcgnuX.Pages
{
    /// <summary>
    /// PwdRepositroy.xaml 的交互逻辑
    /// </summary>
    public partial class PwdRepositroy
    {
        //view Mdoel
        public PwdRepositoryViewModel ViewModel { get; }

        public PwdRepositroy()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetService<PwdRepositoryViewModel>();
            DataContext = ViewModel;
        }


        /// <summary>
        /// 加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Load();
        }

        /// <summary>
        /// 添加按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAddClick(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(Settings.Default.AccountFilePath))
            {
                WindowUtil.ShowBubbleError("未配置账号保存文件路径");
                return;
            }
            var newItemViewModel = new AccountViewModel();
            var result = new EditAccountDialog(newItemViewModel).ShowDialog();
            if (result.GetValueOrDefault())
            {
                ViewModel.Accounts.Insert(0, newItemViewModel);
            }
        }

        /// <summary>
        /// 右键删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickContextMenuDelete(object sender, RoutedEventArgs e)
        {
            var selected = PwdDataGrid.SelectedItem as AccountViewModel;
            if (null == selected) return;

            //删除对话框
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, string.Format("{0} -> {1}", selected.Site, selected.Uname))).ShowDialog();
            if (result.GetValueOrDefault())
            {
                await ViewModel.DeleteItem(selected);
            }
        }

        /// <summary>
        /// 表格内容双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGridDoubleClick(object sender, RoutedEventArgs e)
        {
            var selected = ((DataGrid)sender).SelectedItem as AccountViewModel;
            if (null == selected) return;
            _ = new EditAccountDialog(selected).ShowDialog();
        }

        /// <summary>
        /// 右键菜单展开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContextMenuOpen(object sender, ContextMenuEventArgs e)
        {
            var grid = (DataGrid)sender;
            XamlUtil.SelectRow(grid, e);
            var selected = grid.SelectedItem;
            if(null == selected) e.Handled = true;
        }
    }
}
