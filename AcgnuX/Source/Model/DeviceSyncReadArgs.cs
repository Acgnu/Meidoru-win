using AcgnuX.Source.Bussiness;
using AcgnuX.Source.Model;
using AcgnuX.ViewModel;
using MediaDevices;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 设备同步读取文件列表任务参数
    /// </summary>
    class DeviceSyncReadArgs
    {
        /// <summary>
        /// 目标设备
        /// </summary>
        public MediaDevice Device { get; set; }
        /// <summary>
        /// 目标设备驱动
        /// </summary>
        public DeviceDriverViewModel MediaDrive { get; set; }
    }
}
