using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// DeviceSyncPathConfigDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceSyncPathConfigDialog : BaseDialog
    {
        //内容的ViewModel
        private readonly DeviceSyncPathConfigDialogViewModel _ContentViewModel;

        public DeviceSyncPathConfigDialog()
        {
            InitializeComponent();
            DataContext = this;
            _ContentViewModel = (DeviceSyncPathConfigDialogViewModel) ContentBorder.DataContext;
        }

        /// <summary>
        /// 同步配置行双击选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSyncConfigDataGridDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (null == _ContentViewModel.SelectedItem) return;
            //打开修改对话框
            var dialog = new EditSyncConfigDialog(_ContentViewModel.SelectedItem);
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault() == true)
            {
                _ContentViewModel.NotifyCheckBoxChange();
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
            if (null == _ContentViewModel.SelectedItem) return;
            //删除对话框
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, string.Format("{0}", _ContentViewModel.SelectedItem.PcPath))).ShowDialog();
            if (result.GetValueOrDefault())
            {
                _ContentViewModel.DeleteSelected();
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
                Messenger.Default.Send(new BubbleTipViewModel
                {
                    AlertLevel = AlertLevel.ERROR,
                    Text = "答应我, 先去配置数据库"
                });
                return;
            }
            //打开修改对话框
            var dialog = new EditSyncConfigDialog(null);
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault() == true)
            {
                _ContentViewModel.AddNewSyncConfig(dialog.SyncConfig);
            }
        }
    }
}
