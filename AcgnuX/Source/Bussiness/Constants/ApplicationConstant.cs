
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AcgnuX.Source.Bussiness.Constants
{
    /// <summary>
    /// 通用常量类
    /// </summary>
    public static class ApplicationConstant
    {
        // 乐谱分享包名称
        public static readonly string SHARE_ZIP_NAME = "sheet.zip";
        // 分享乐谱的临时路径
        public static readonly string SHARE_TEMP_FOLDER_NAME = "share_tmp";
        // 使用bandzip的压缩参数
        public static readonly string BANDZIP_ZIP_CMD = "";
        // 默认的封面文件名
        public static readonly string DEFAULT_COVER_NAME = "cover.jpg";
        // 默认的乐谱图片格式
        public static readonly string DEFAULT_SHEET_PAGE_FORMAT = ".png";
        //抓取IP服务名称
        public static readonly string CRAWL_IP_SERVICE_NAME = "CrawlIPService";
        //默认的数据库初始化文件
        public readonly static string DB_INIT_FILE = "init.sql";

        #region MVVM消息
        //单个tan8乐谱下载完成消息
        public static readonly string TAN8_DOWNLOAD_COMPLETE = "tan8_sheet_download_complete";
        #endregion

        //默认的乐谱图标
        private static BitmapImage mDefaultSheetCover = null;
        public static BitmapImage GetDefaultSheetCover()
        {
            if(null == mDefaultSheetCover)
            {
                mDefaultSheetCover = new BitmapImage(new Uri(
                    string.Format("pack://application:,,,/{0};component/Assets/Images/piano-cover-default.jpg", System.Reflection.Assembly.GetEntryAssembly().GetName().Name),
                    UriKind.RelativeOrAbsolute));
            }
            return mDefaultSheetCover;
        }


        //默认文件图标
        private static BitmapImage mDefaultFileIcon = null;
        public static BitmapImage GetDefaultFileIcon()
        {
            if (null == mDefaultFileIcon)
            {
                mDefaultFileIcon = new BitmapImage(new Uri(
                    string.Format("pack://application:,,,/{0};component/Assets/Images/icon_unknow_file.png", System.Reflection.Assembly.GetEntryAssembly().GetName().Name),
                    UriKind.RelativeOrAbsolute));
            }
            return mDefaultFileIcon;
        }


        //默认的音频图标
        private static BitmapImage mDefaultAudioIcon = null;
        public static BitmapImage GetDefaultAudioIcon()
        {
            if (null == mDefaultAudioIcon)
            {
                mDefaultAudioIcon = new BitmapImage(new Uri(
                    string.Format("pack://application:,,,/{0};component/Assets/Images/icon_audio_file.png", System.Reflection.Assembly.GetEntryAssembly().GetName().Name),
                    UriKind.RelativeOrAbsolute));
            }
            return mDefaultAudioIcon;
        }


        //默认的图片图标
        private static BitmapImage mDefaultImageIcon = null;
        public static BitmapImage GetDefaultImageIcon()
        {
            if (null == mDefaultImageIcon)
            {
                mDefaultImageIcon = new BitmapImage(new Uri(
                    string.Format("pack://application:,,,/{0};component/Assets/Images/icon_image_file.png", System.Reflection.Assembly.GetEntryAssembly().GetName().Name),
                    UriKind.RelativeOrAbsolute));
            }
            return mDefaultImageIcon;
        }


        //默认的视频图标
        private static BitmapImage mDefaultVideoIcon = null;
        public static BitmapImage GetDefaultVideoIcon()
        {
            if (null == mDefaultVideoIcon)
            {
                mDefaultVideoIcon = new BitmapImage(new Uri(
                    string.Format("pack://application:,,,/{0};component/Assets/Images/icon_video_file.png", System.Reflection.Assembly.GetEntryAssembly().GetName().Name),
                    UriKind.RelativeOrAbsolute));
            }
            return mDefaultVideoIcon;
        }
    }
}
