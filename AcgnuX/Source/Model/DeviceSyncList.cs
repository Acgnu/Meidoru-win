using AcgnuX.Source.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcgnuX.Source.ViewModel
{
    class DeviceSyncList
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
        public List<DeviceSyncItem> PcItemList { get; set; }
        /// <summary>
        /// 仅存于移动端的文件
        /// </summary>
        public List<DeviceSyncItem> MobileItemList { get; set; }
    }
}
