using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.DesignModel;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.WindowX.Dialog;
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
    public partial class ContactManage : BasePage
    {
        //数据集
        private ContactListViewModel mContactListViewModel = new ContactListViewModel();
        private readonly ContactRepo ContactRepo = ContactRepo.Instance;

        public ContactManage(MainWindow mainWin)
        {
            InitializeComponent();
            mMainWindow = mainWin;
            ContactListControl.DataContext = mContactListViewModel;
        }


        /// <summary>
        /// 加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if(DataUtil.IsEmptyCollection(mContactListViewModel.Items))
            {
                BusyIndicator.IsBusy = true;
                var dataList = await ContactRepo.FindAllAsync(null);
                AddToList(dataList);
                BusyIndicator.IsBusy = false;
            }
        }

        private void AddToList(List<Contact> contacts)
        {
            mContactListViewModel.Items.Clear();
            if (!DataUtil.IsEmptyCollection(contacts))
            {
                var contactVms = new List<ContactItemViewModel>();
                foreach (var item in contacts)
                {
                    contactVms.Add(new ContactItemViewModel(this)
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Uid = item.Uid,
                        Phone = item.Phone,
                        Avatar = new ByteArray(item.Avatar),
                        Platform = item.Platform
                    });
                }
                mContactListViewModel.Items.AddRange(contactVms);
            }
        }

        /// <summary>
        /// 条件筛选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnFilterBoxKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var text = FilterBox.Text;
                BusyIndicator.IsBusy = true;
                var dataList = await ContactRepo.FindAllAsync(text);
                AddToList(dataList);
                BusyIndicator.IsBusy = false;
            }
        }

        /// <summary>
        /// 添加按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAddClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.DBFilePath))
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "答应我, 先去配置数据库"
                });
                return;
            }
            new EditContactDialog(null, this).ShowDialog();
        }

        /// <summary>
        /// 删除联系人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DeleteContact(ContactItemViewModel vm)
        {
            ContactRepo.Del(vm.Id.Value);
            mContactListViewModel.Items.Remove(vm);
        }

        /// <summary>
        /// 联系人编辑完成
        /// </summary>
        /// <param name="contactItemView"></param>
        /// <returns></returns>
        public InvokeResult<ContactItemViewModel> SaveContact(ContactItemViewModel contactItemView)
        {
            if (string.IsNullOrEmpty(contactItemView.Uid) || string.IsNullOrEmpty(contactItemView.Name) || null == contactItemView.Platform)
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "啥都不填想啥呢"
                });
                return InvokeFail(contactItemView);
            }
            if (null == contactItemView.Id)
            {
                var id = ContactRepo.Add(contactItemView);
                contactItemView.Id = id;
                mContactListViewModel.Items.Insert(0, contactItemView);
            }
            else
            {
                ContactRepo.Update(contactItemView);
            }
            return InvokeSuccess(contactItemView);
        }
    }
}
