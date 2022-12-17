using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.ViewModel.Design
{
    public class FileItemsCompareDesignViewModel : FileItemsCompareViewModel
    {
        public FileItemsCompareDesignViewModel()
        {
            PcFolderPath = "C:/Administrator/Photos";
            MobileFolderPath = "DCIM/Camera";
            var dv = new FileItemListDesignViewModel();
            PcFileItems = dv;
            MobileFileItems = dv;
        }
    }
}
