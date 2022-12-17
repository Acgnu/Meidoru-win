using AcgnuX.Source.Bussiness;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.ViewModel;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 文件同步任务参数
    /// </summary>
    public class DeviceSyncTaskArgs
    {
        //标识任务是否正常执行完成
        public bool IsOk { get; set; } = false;
        /// <summary>
        /// 选中的同步列表
        /// </summary>
        public FileItemsCompareViewModel Item { get; set; }
        /// <summary>
        /// 进度消息
        /// </summary>
        public BubbleTipViewModel ProgressMessage { get; set; }
        /// <summary>
        /// 进度类型 1 子项目同步完成, 2 队列同步完成
        /// </summary>
        public SyncTaskProgressType ProgressType { get; set; }
        /// <summary>
        /// 待删除的子项目类型
        /// </summary>
        public SyncDeviceType Source { get; set; }
        /// <summary>
        /// 待删除的子项目项
        /// </summary>
        public FileItemViewModel DeviceSyncItem { get; set; }
    }
}
