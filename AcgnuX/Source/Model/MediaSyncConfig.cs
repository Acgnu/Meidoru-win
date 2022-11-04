using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 指定需要同步的路径对应关系
    /// </summary>
    public class MediaSyncConfig : ViewModelBase
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// PC路径
        /// </summary>
        public string PcPath { get; set; }
        /// <summary>
        /// 移动端路径
        /// </summary>
        public string MobilePath { get; set; }
        /// <summary>
        /// 是否启用 1 是 0 否
        /// </summary>
        public byte Enable { get; set; }
    }
}
