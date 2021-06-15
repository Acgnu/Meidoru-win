using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.WindowX.Dialog;
using System.Collections.ObjectModel;
using System.Linq;
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
        //数据集
        private ObservableCollection<Account> accountList = new ObservableCollection<Account>();

        public PwdRepositroy(MainWindow mainWin)
        {
            InitializeComponent();
            mMainWindow = mainWin;
            PasswordDataGrid.ItemsSource = accountList;
        }


        /// <summary>
        /// 加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if(accountList.Count == 0)
            {
                LoadAllPassword();
            }
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        private void LoadAllPassword()
        {
            var accountFilePath = ConfigUtil.Instance.AccountJsonPath;
            var itemsInFile = FileUtil.DeserializeJsonFromFile<ObservableCollection<Account>>(accountFilePath);
            if(null != itemsInFile && itemsInFile.Count > 0)
            { 
                foreach(var item in itemsInFile)
                {
                    accountList.Add(item);
                }
            }
        }

        private void OnFilterBoxKeyDown(object sender, KeyEventArgs e)
        {
            var box = sender as TextBox;
            if (string.IsNullOrEmpty(box.Text))
            {
                PasswordDataGrid.ItemsSource = accountList;
            }
            else
            {
                PasswordDataGrid.ItemsSource = accountList.Where(item => item.Site.Contains(box.Text) || item.Describe.Contains(box.Text));
            }
        }

        /// <summary>
        /// 添加按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAddClick(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(ConfigUtil.Instance.AccountJsonPath))
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "未配置账号保存文件路径"
                });
                return;
            }
            new EditAccountDialog(null, this).ShowDialog();
        }

        /// <summary>
        /// 刷新按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OnClickRefreshButton(object sender, RoutedEventArgs e)
        //{
        //    LoadAllPassword();
        //}

        /// <summary>
        /// 右键删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickContextMenuDelete(object sender, RoutedEventArgs e)
        {
            var selected = PasswordDataGrid.SelectedItem as Account;
            if (null == selected) return;

            //删除对话框
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format((string)Application.Current.FindResource("DeleteConfirm"), string.Format("{0} -> {1}", selected.Site, selected.Uname))).ShowDialog();
            if (result.GetValueOrDefault())
            {
                accountList.Remove(selected);
                FileUtil.SaveJsonToFile(accountList, ConfigUtil.Instance.AccountJsonPath);
            }
        }

        /// <summary>
        /// 表格内容双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGridDoubleClick(object sender, RoutedEventArgs e)
        {
            var selected = PasswordDataGrid.SelectedItem as Account;
            if (null == selected) return;
            new EditAccountDialog(selected, this).ShowDialog();
        }

        /// <summary>
        /// 右键复制账号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickContextMenuCopyAccount(object sender, RoutedEventArgs e)
        {
            var selected = PasswordDataGrid.SelectedItem as Account;
            if (null == selected) return;
            Clipboard.SetDataObject(selected.Uname);
        }

        /// <summary>
        /// 右键复制密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickContextMenuCopyPassword(object sender, RoutedEventArgs e)
        {
            var selected = PasswordDataGrid.SelectedItem as Account;
            if (null == selected) return;
            Clipboard.SetDataObject(selected.Upass);
        }

        /// <summary>
        /// 账户编辑完成事件
        /// </summary>
        /// <param name="account"></param>
        /// <returns>编辑的账户</returns>
        public InvokeResult<Account> SaveAccount(Account account)
        {
            if(string.IsNullOrEmpty(account.Site) || string.IsNullOrEmpty(account.Uname) || string.IsNullOrEmpty(account.Upass))
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "啥都不填想啥呢"
                });
                return InvokeFail<Account>(account);
            }
            if (null == account.Id)
            {
                //新增操作, 查询列表里所有账号的ID, 得到最大ID作为新账号的ID
                for(var i = 0; i < accountList.Count; i++)
                {
                    var item = accountList[i];
                    if (item.Id != null && item.Id > account.Id.GetValueOrDefault())
                    {
                        account.Id = item.Id.GetValueOrDefault();
                    }
                }
                account.Id = null == account.Id ? 1 : ++account.Id;
                accountList.Add(account);
            }
            else
            {
                //修改替换原有未知的数据
                accountList[PasswordDataGrid.SelectedIndex] = account;
            }
            //保存到文件
            FileUtil.SaveJsonToFile(accountList, ConfigUtil.Instance.AccountJsonPath);

            return InvokeSuccess(account);
        }

        /// <summary>
        /// 右键菜单展开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContextMenuOpen(object sender, ContextMenuEventArgs e)
        {
            XamlUtil.SelectRow(PasswordDataGrid, e);
            var selected = PasswordDataGrid.SelectedItem;
            if(null == selected) e.Handled = true;
        }
    }
}
