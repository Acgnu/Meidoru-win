﻿using AcgnuX.Controls;
using AcgnuX.Source.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    public class ContactListViewModel : ViewModelBase
    {
        /// <summary>
        /// 联系人列表
        /// </summary>
        public RangeObservableCollection<ContactItemViewModel> Items { get; set; } = new RangeObservableCollection<ContactItemViewModel>();

        /// <summary>
        /// 选项变更命令
        /// </summary>
        public ICommand OnSelectedCommand { get; set; }

        public ContactListViewModel()
        {
            OnSelectedCommand = new RelayCommand<ContactItemViewModel>(OnSelected);
        }

        /// <summary>
        /// 选项变更动作
        /// </summary>
        private void OnSelected(ContactItemViewModel current)
        {
            foreach (var item in Items)
            {
                item.IsSelected = false;
            }
            current.IsSelected = true;
        }
    }
}
