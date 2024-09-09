using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// DeviceSyncPathConfigDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceSyncPathConfigDialog : BaseDialog
    {
        //内容的ViewModel
        public DeviceSyncPathConfigDialogViewModel ContentViewModel { get; }

        public DeviceSyncPathConfigDialog()
        {
            InitializeComponent();
            DataContext = this;
            ContentViewModel = App.Current.Services.GetService<DeviceSyncPathConfigDialogViewModel>();
        }

        /// <summary>
        /// 同步配置行双击选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSyncConfigDataGridDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (null == ContentViewModel.SelectedItem) return;
            //打开修改对话框
            var dialog = new EditSyncConfigDialog(ContentViewModel.SelectedItem);
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault() == true)
            {
                ContentViewModel.NotifyCheckBoxChange();
            }
        }

        /// <summary>
        /// 同步规则删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSyncConfigItemMouseRightClick(object sender, MouseButtonEventArgs e)
        {
            XamlUtil.SelectRow(SyncPathDataGrid, e);
            if (null == ContentViewModel.SelectedItem) return;
            //删除对话框
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, string.Format("{0}", ContentViewModel.SelectedItem.PcPath))).ShowDialog();
            if (result.GetValueOrDefault())
            {
                ContentViewModel.DeleteSelected();
            }
        }

        /// <summary>
        /// 添加规则按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddSyncConfigClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.DBFilePath))
            {
                WindowUtil.ShowBubbleError("答应我, 先去配置数据库");
                return;
            }
            //打开修改对话框
            var dialog = new EditSyncConfigDialog(null);
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault() == true)
            {
                ContentViewModel.AddNewSyncConfig(dialog.SyncConfig);
            }
        }
    }
}
