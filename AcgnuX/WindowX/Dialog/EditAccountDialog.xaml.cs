using AcgnuX.Pages;
using AcgnuX.Source.Model;
using AcgnuX.Source.ViewModel;
using AcgnuX.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑账号的弹窗
    /// </summary>
    public partial class EditAccountDialog : BaseDialog
    {
        //编辑的数据vm
        public AccountViewModel AccountViewModel { get; set; }
        //父viewmodel
        private readonly PwdRepositoryViewModel _PwdRepositoryViewModel;

        public EditAccountDialog(AccountViewModel accountVm, PwdRepositoryViewModel pwdRepositoryViewModel)
        {
            InitializeComponent();
            _PwdRepositoryViewModel = pwdRepositoryViewModel;
            AccountViewModel = accountVm ?? new AccountViewModel();
            DataContext = this;
        }

        /// <summary>
        /// 保存按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;

            //引发事件
            var result = await _PwdRepositoryViewModel.SaveItem(AccountViewModel);
            button.IsEnabled = true;
            if (result.success) Close();
        }
    }
}