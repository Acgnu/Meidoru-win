using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Utils;
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
    public class SyncConfigViewModel : MediaSyncConfig
    {
        public string PcPathView
        {
            get { return PcPath; }
            set 
            {
                PcPath = value;
                OnPropertyChanged(nameof(PcPath));
            }
        }
        public string MobilePathView
        {
            get { return MobilePath; }
            set
            {
                MobilePath = value;
                OnPropertyChanged(nameof(MobilePath));
            }
        }
        public byte EnableView
        {
            get { return Enable; }
            set
            {
                Enable = value;
                OnPropertyChanged(nameof(Enable));
            }
        }
    }
}
