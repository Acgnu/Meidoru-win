using AcgnuX.Pages;
using AcgnuX.Source.Model;
using System.Windows;
using System.Windows.Controls;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑账号的弹窗
    /// </summary>
    public partial class EditAccountDialog : BaseDialog
    {
        public Account Macount { get; set; }
        //父Page
        private PwdRepositroy mPwdRepositroy;

        public EditAccountDialog(Account account, PwdRepositroy pwdRepositroy)
        {
            InitializeComponent();
            mPwdRepositroy = pwdRepositroy;
            Macount = account;
            DataContext = this;
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

            //引发事件
            var result = mPwdRepositroy.SaveAccount(GetEditAccount());
            button.IsEnabled = true;
            if (result.success) Close();
        }

        /// <summary>
        /// 提取表单数据
        /// </summary>
        /// <returns></returns>
        private Account GetEditAccount()
        {
            return new Account()
            {
                Id = Macount?.Id,
                Site = TextBlockSite.Text,
                Uname = TextBlockUname.Text,
                Upass = TextBlockUpass.Text,
                Describe = TextBlockDescribe.Text,
                Remark = TextBlockRemark.Text,
            };
        }

        /// <summary>
        /// 键盘按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                OnConfirmClick(ConfirmButton, null);
            }
        }
    }
}