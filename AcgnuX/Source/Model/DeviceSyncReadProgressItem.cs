using AcgnuX.Source.Bussiness;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 设备同步读取文件列表进度参数
    /// </summary>
    class DeviceSyncReadProgressItem
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
        /// 文件项
        /// </summary>
        public DeviceSyncItem FileItem { get; set; }
        /// <summary>
        /// 文件源
        /// </summary>
        public SyncDeviceType FileSource { get; set; }
        /// <summary>
        /// 进度通知类型 1 文本提示, 2 文件读取
        /// </summary>
        public ReadSyncFileTaskProgressType ProgressType { get; set; }
        /// <summary>
        /// 通知对象
        /// </summary>
        public MainWindowStatusNotify Notify { get; set; }
    }
}
