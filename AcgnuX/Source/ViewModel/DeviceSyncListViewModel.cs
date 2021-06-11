﻿using AcgnuX.Source.Model;
using System.Collections.ObjectModel;

namespace AcgnuX.Source.ViewModel
{
    class DeviceSyncListViewModel : BasePropertyChangeNotifyModel
    {
        public string FolderNameAndView { get; set; }
        public ObservableCollection<DeviceSyncListImgViewItem> ImgItemList { get; set; }
    }
}
