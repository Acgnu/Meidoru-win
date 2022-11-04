using AcgnuX.Model.Ten.Dns;
using AcgnuX.Pages;
using AcgnuX.Source.Model.Ten.Dns;
using AcgnuX.Source.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑DNS的弹窗
    /// </summary>
    public partial class EditDnsRecordDialog : BaseDialog
    {
        //内容vm
        public EditDnsRecordDialogViewModel ContentViewModel { get; set; }

        public EditDnsRecordDialog(DnsItemViewModel itemVm, Action loadAction)
        {
            InitializeComponent();
            //窗口vm
            DataContext = this;
            ContentViewModel = new EditDnsRecordDialogViewModel(itemVm, Close, loadAction);
        }
    }
}