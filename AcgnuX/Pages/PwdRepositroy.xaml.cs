using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.ViewModel;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcgnuX.Pages
{
    /// <summary>
    /// PwdRepositroy.xaml 的交互逻辑
    /// </summary>
    public partial class PwdRepositroy : BasePage
    {
        //view Mdoel
        private PwdRepositoryViewModel _ViewModel;
        public PwdRepositroy()
        {
            InitializeComponent();
            _ViewModel = DataContext as PwdRepositoryViewModel;
        }


        /// <summary>
        /// 加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _ViewModel.Load();
        }

        private void OnFilterBoxKeyDown(object sender, KeyEventArgs e)
        {
            _ViewModel.Load();
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
                Messenger.Default.Send(new BubbleTipViewModel
                {
                    AlertLevel = AlertLevel.ERROR,
                    Text = "未配置账号保存文件路径"
                });
                return;
            }
            new EditAccountDialog(null, _ViewModel).ShowDialog();
        }

        /// <summary>
        /// 右键删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickContextMenuDelete(object sender, RoutedEventArgs e)
        {
            var selected = PwdDataGrid.SelectedItem as AccountViewModel;
            if (null == selected) return;

            //删除对话框
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, string.Format("{0} -> {1}", selected.Site, selected.Uname))).ShowDialog();
            if (result.GetValueOrDefault())
            {
                _ViewModel.DeleteItem(selected);
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
            new EditAccountDialog(selected, _ViewModel).ShowDialog();
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
