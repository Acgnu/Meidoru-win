using AcgnuX.Source.Utils;
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
        public AccountViewModel AccountViewModel { get; }

        public EditAccountDialog(AccountViewModel accountVm)
        {
            InitializeComponent();
            AccountViewModel = accountVm ?? new AccountViewModel();
            DataContext = this;
            FormStackPanel.BindingGroup.BeginEdit();
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

            if (!FormStackPanel.BindingGroup.CommitEdit())
            {
                button.IsEnabled = true;
                return;
            }

            //引发事件
            await AccountViewModel.SaveAsync();
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