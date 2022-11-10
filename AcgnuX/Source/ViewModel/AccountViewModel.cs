using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.ViewModel
{
    /// <summary>
    /// 密码库账号模型
    /// </summary>
    public class AccountViewModel : ViewModelBase
    {
        public short? Id { get; set; }
        public string Site { get; set; }
        public string Describe { get; set; }
        public string Uname { get; set; }
        public string Upass { get; set; }
        public string Remark { get; set; }
    }
}
