using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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

        private readonly ContactRepo _ContactRepo = ContactRepo.Instance;


        public ContactManageViewModel()
        {
            OnFilterInputCommand = new RelayCommand(OnFilterInput);
        }

        public async void Load()
        {
            if (DataUtil.IsEmptyCollection(ContactListData.Items))
            {
                IsBusy = true;
                var dataList = await _ContactRepo.FindAllAsync(null);
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
        /// 过滤输入框命令
        /// </summary>
        private async void OnFilterInput()
        {
            IsBusy = true;
            var dataList = await _ContactRepo.FindAllAsync(FilterText);
            AddToList(dataList);
            IsBusy = false;
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
