﻿using AcgnuX.Pages;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    public class ContactItemViewModel : ViewModelBase
    {
        /// <summary>
        /// 数据ID
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// 联系人平台 
        /// </summary>
        private ContactPlatform? platform;
        public ContactPlatform? Platform
        {
            get { return platform; }
            set { platform = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 联系人名称/备注名称
        /// </summary>
        private string name;
        public string Name 
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 联系人UID
        /// </summary>
        private string uid;
        public string Uid
        {
            get { return uid; }
            set { uid = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 联系人手机号
        /// </summary>
        private string phone;
        public string Phone
        {
            get { return phone; }
            set { phone = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 联系人头像/二维码
        /// </summary>
        private ByteArray avatar;
        public ByteArray Avatar
        {
            get { return avatar; }
            set { avatar = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// True if this item is currently selected
        /// </summary>
        private bool isSelected = false;
        public bool IsSelected {
            get { return isSelected; }
            set
            {
                if (!IsSelected.Equals(value))
                {
                    isSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ContactManageViewModel _MngViewModel;

        /// <summary>
        /// Opens the current message thread
        /// </summary>
        public ICommand OnEditCommand { get; set; }

        public ICommand OnDeleteCommand { get; set; }

        public ICommand OnSelectedCommand { get; set; }

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ContactItemViewModel(ContactManageViewModel mngVm)
        {
            // Create commands
            OnEditCommand = new RelayCommand(OnEditItem);
            OnDeleteCommand = new RelayCommand(DeleteItem);
            _MngViewModel = mngVm;
        }

        #endregion

        #region Command Methods

        public void OnEditItem()
        {
            new EditContactDialog(this, _MngViewModel).ShowDialog();
        }

        public void DeleteItem()
        {
            IsSelected = true;
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, Name)).ShowDialog();
            if (result.GetValueOrDefault())
            {
                _MngViewModel.DeleteContact(this);
            }
        }

        #endregion
    }
}
