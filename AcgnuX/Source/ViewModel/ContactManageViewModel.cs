using AcgnuX.Controls;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.WindowX.Dialog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 联系人管理View Model
    /// </summary>
    public class ContactManageViewModel : ObservableObject
    {
        /// <summary>
        /// 联系人列表
        /// </summary>
        public ObservableCollection<ContactItemViewModel> ContactItems { get; set; } = new ObservableCollection<ContactItemViewModel>();

        public bool IsEmpty { get => ContactItems.Count == 0; }

        public bool isBusy = false;
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value); }

        public string FilterText { get; set; }

        //过滤命令
        public ICommand OnFilterInputCommand { get; set; }

        public ICommand OnEditCommand { get; }

        public ICommand OnDeleteCommand { get; }
        /// <summary>
        /// 选项变更命令
        /// </summary>
        public ICommand OnSelectedCommand { get; }

        //默认为安全模式, 不展示数据
        public bool SafeMode { get; set; } = false;
        //关闭安全模式口令
        private readonly string _OffSafeModelKey = "turn off safe mode";

        private readonly ContactRepo _ContactRepo;
        private ICollectionView _CollectionView;

        public ContactManageViewModel(ContactRepo contactRepo)
        {
            this._ContactRepo = contactRepo;
            OnFilterInputCommand = new RelayCommand(ExecuteFilterCommand);
            OnDeleteCommand = new RelayCommand<ContactItemViewModel>(ExecuteDeleteCommand);
            OnEditCommand = new RelayCommand<ContactItemViewModel>(ExecuteEditCommand);
            OnSelectedCommand = new RelayCommand<ContactItemViewModel>(ExecuteSelectCommand);
            OnFilterInputCommand = new RelayCommand(ExecuteFilterCommand);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        public async void Load()
        {
            if (SafeMode)
            {
                return;
            }
            if (ContactItems.Count > 0) return; //only load once on loaded event

            IsBusy = true;
            var dataList = await _ContactRepo.FindAllAsync(null);
            AddToList(dataList);
            _CollectionView = CollectionViewSource.GetDefaultView(ContactItems);
            IsBusy = false;
        }

        private void AddToList(List<Contact> contacts)
        {
            if (!DataUtil.IsEmptyCollection(contacts))
            {
                foreach (var item in contacts)
                {
                    ContactItems.Add(new ContactItemViewModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Uid = item.Uid,
                        Phone = item.Phone,
                        Avatar = new ByteArray(item.Avatar),
                        Platform = item.Platform
                    });
                }
            }
            OnPropertyChanged(nameof(IsEmpty));
        }

        /// <summary>
        /// 删除联系人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ExecuteDeleteCommand(ContactItemViewModel item)
        {
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, item.Name)).ShowDialog();
            if (result.GetValueOrDefault())
            {
                _ContactRepo.Del(item.Id.Value);
                ContactItems.Remove(item);
            }
        }

        /// <summary>
        /// 删除联系人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ExecuteEditCommand(ContactItemViewModel item)
        {
            new EditContactDialog(item).ShowDialog();
        }

        /// <summary>
        /// 选中
        /// </summary>
        /// <param name="item"></param>
        public void ExecuteSelectCommand(ContactItemViewModel item)
        {
            foreach (var loopItem in ContactItems)
            {
                if (loopItem.IsSelected)
                {
                    loopItem.IsSelected = false;
                }
            }
            item.IsSelected = true;
        }

        /// <summary>
        /// 执行过滤命令
        /// </summary>
        private void ExecuteFilterCommand()
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                if (null == _CollectionView || null == _CollectionView.Filter) return;

                _CollectionView.Filter = null;
                return;
            }

            if (_OffSafeModelKey.Equals(FilterText))
            {
                //口令正确则关闭安全模式
                SafeMode = false;
                //清空口令以展示所有结果
                FilterText = null;
                Load();
                return;
            }

            _CollectionView.Filter = new Predicate<object>((o) => (o as ContactItemViewModel).Name.Contains(FilterText));
        }
    }
}
