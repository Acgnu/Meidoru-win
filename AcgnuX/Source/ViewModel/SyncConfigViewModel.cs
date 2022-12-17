using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Utils;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 同步配置视图模型
    /// </summary>
    public class SyncConfigViewModel : ViewModelBase
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// PC路径
        /// </summary>
        private string _PcPath;
        public string PcPath { get => _PcPath; set { _PcPath = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 移动端路径
        /// </summary>
        private string _MobilePath;
        public string MobilePath { get => _MobilePath; set { _MobilePath = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 是否启用 1 是 0 否
        /// </summary>
        private bool _Enable;
        public bool Enable { get => _Enable; set { _Enable = value; RaisePropertyChanged(); } }
    }
}
