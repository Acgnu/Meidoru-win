using AcgnuX.Source.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AcgnuX.Source.ViewModel
{
    class DeviceSyncListImgViewItem : BasePropertyChangeNotifyModel
    {
        /// <summary>
        /// 图片名称
        /// </summary>
        public string ImgNameView { get; set; }
        /// <summary>
        /// 图片MD5
        /// </summary>
        public string ImgMd5View { get; set; }
        /// <summary>
        /// 图片归属文件夹
        /// </summary>
        public string ImgFolderView { get; set; }
        /// <summary>
        /// 图片来源 1 PC, 2 手机
        /// </summary>
        public byte ImgSourceView { get; set; }
        /// <summary>
        /// 图片bitmap数据
        /// </summary>
        public BitmapImage BitImg { get; set; }
    }
}
