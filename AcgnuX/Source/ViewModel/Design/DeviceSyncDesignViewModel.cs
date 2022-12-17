using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.ViewModel.Design
{
    /// <summary>
    /// 设备同步列表设计ViewModel
    /// </summary>
    public class DeviceSyncDesignViewModel : DeviceSyncViewModel
    {
        public DeviceSyncDesignViewModel()
        {
            ProgressText = "Design Progress Text";
            ProgressAlertLevel = Bussiness.Constants.AlertLevel.INFO;
            ProgressValue = 100;
            var dCmp = new FileItemsCompareDesignViewModel();
            ListData = new System.Collections.ObjectModel.ObservableCollection<FileItemsCompareViewModel>
            {
               new FileItemsCompareViewModel
               {
                   PcFileItems = dCmp.PcFileItems,
                   MobileFileItems = dCmp.MobileFileItems,
                   PcFolderPath = dCmp.PcFolderPath,
                   MobileFolderPath = dCmp.MobileFolderPath
               },
               new FileItemsCompareViewModel
               {
                   PcFileItems = dCmp.PcFileItems,
                   MobileFileItems = dCmp.MobileFileItems,
                   PcFolderPath = dCmp.PcFolderPath,
                   MobileFolderPath = dCmp.MobileFolderPath
               },
               new FileItemsCompareViewModel
               {
                   PcFileItems = dCmp.PcFileItems,
                   MobileFileItems = dCmp.MobileFileItems,
                   PcFolderPath = dCmp.PcFolderPath,
                   MobileFolderPath = dCmp.MobileFolderPath
               }
            };
        }
    }
}
