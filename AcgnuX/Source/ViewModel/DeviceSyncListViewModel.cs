using AcgnuX.Source.Model;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace AcgnuX.Source.ViewModel
{
    class DeviceSyncListViewModel : ViewModelBase
    {
        /// <summary>
        /// PC文件夹路径
        /// </summary>
        public string PcFolderNameView { get; set; }
        /// <summary>
        /// 移动端文件夹路径
        /// </summary>
        public string MobileFolderNameView { get; set; }
        /// <summary>
        /// 仅存于PC的文件
        /// </summary>
        public ObservableCollection<DeviceSyncItem> PcItemList { get; set; }
        /// <summary>
        /// 仅存于移动端的文件
        /// </summary>
        public ObservableCollection<DeviceSyncItem> MobileItemList { get; set; }
    }
}
