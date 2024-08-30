using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 联系人管理View Model
    /// </summary>
    public class ContactManageViewModel : ViewModelBase
    {
        //数据集
        public ContactListViewModel ContactListData { get; set; } = new ContactListViewModel();
        public bool IsEmpty { get => ContactListData.Items.Count == 0; }

        public bool _IsBusy = false;
        public bool IsBusy { get => _IsBusy; set { _IsBusy = value; RaisePropertyChanged(); } }

        public string FilterText { get; set; }

        //过滤命令
        public ICommand OnFilterInputCommand { get; set; }

        //默认为安全模式, 不展示数据
        public bool SafeMode { get; set; } = true;
        //关闭安全模式口令
        private readonly string _OffSafeModelKey = "turn off safe mode";

        private readonly ContactRepo _ContactRepo = ContactRepo.Instance;


        public ContactManageViewModel()
        {
            OnFilterInputCommand = new RelayCommand(() => Load(true));
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        public async void Load(bool forceSearch)
        {
            if (_OffSafeModelKey.Equals(FilterText))
            {
                //口令正确则关闭安全模式
                SafeMode = false;
                //清空口令以展示所有结果
                FilterText = null;
            }
            if (SafeMode)
            {
                return;
            }
            if (DataUtil.IsEmptyCollection(ContactListData.Items) || forceSearch)
            {
                IsBusy = true;
                var dataList = await _ContactRepo.FindAllAsync(FilterText);
                AddToList(dataList);
                IsBusy = false;
            }
        }

        private void AddToList(List<Contact> contacts)
        {
            ContactListData.Items.Clear();
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
                ContactListData.Items.AddRange(contactVms);
            }
            NotifyIsEmpty();
        }

        /// <summary>
        /// 联系人编辑完成
        /// </summary>
        /// <param name="contactItemView"></param>
        /// <returns></returns>
        public bool SaveContact(ContactItemViewModel contactItemView)
        {
            if (string.IsNullOrEmpty(contactItemView.Uid) || string.IsNullOrEmpty(contactItemView.Name) || null == contactItemView.Platform)
            {
                Messenger.Default.Send(new BubbleTipViewModel
                {
                    AlertLevel = Source.Bussiness.Constants.AlertLevel.ERROR,
                    Text = "啥都不填想啥呢"
                });
                return false;
            }
            if (null == contactItemView.Id)
            {
                var id = _ContactRepo.Add(contactItemView);
                contactItemView.Id = id;
                ContactListData.Items.Insert(0, contactItemView);
            }
            else
            {
                _ContactRepo.Update(contactItemView);
            }
            return true;
        }

        /// <summary>
        /// 删除联系人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DeleteContact(ContactItemViewModel vm)
        {
            _ContactRepo.Del(vm.Id.Value);
            ContactListData.Items.Remove(vm);
        }

        /// <summary>
        /// 通知
        /// </summary>
        private void NotifyIsEmpty()
        {
            RaisePropertyChanged(nameof(IsEmpty));
        }
    }
}
