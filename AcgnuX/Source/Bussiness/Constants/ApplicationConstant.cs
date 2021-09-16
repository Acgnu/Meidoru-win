using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //图床API
        public static readonly string[] IMG_REPO_API = {
            "meet",     //遇见图床APICODE
            "oleou",     //多合一
            "prnt"  //prnt
        };
    }
}
