using AcgnuX.Source.Bussiness;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AcgnuX.Source.ViewModel
{
    class DeviceSyncItem
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string NameView { get; set; }
        /// <summary>
        /// 文件MD5
        /// </summary>
        public string Md5View { get; set; }
        /// <summary>
        /// 文件归属文件夹
        /// </summary>
        public string FolderView { get; set; }
        /// <summary>
        /// 文件来源 1 PC, 2 手机
        /// </summary>
        public SyncDeviceType SourceView { get; set; }
        /// <summary>
        /// 文件预览图
        /// </summary>
        //public Bitmap BitImg { get; set; }
        public BitmapImage BitImg { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public SyncContentType ContentType;
        /// <summary>
        /// 图片byte数组
        /// </summary>
        public byte[] ImgByte { get; set; }
    }
}
