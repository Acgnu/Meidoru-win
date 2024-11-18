using AcgnuX.Source.Bussiness.Constants;
using System.Text.RegularExpressions;

namespace AcgnuX.Source.Utils
{
    public class MediaUtil
    {
        /// <summary>
        /// 根据后缀名判断是否为支持的媒体文件类型, 用于读取MTP缩略图
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static SyncContentType GetMediaTypeByName(string fileName)
        {
            if (Regex.IsMatch(fileName, @"(.*?)\.(mp3|aac|flac|wma)$", RegexOptions.IgnoreCase))
            {
                return SyncContentType.AUDIO;
            }
            else if (Regex.IsMatch(fileName, @"(.*?)\.(mp4|3gp|wmv)$", RegexOptions.IgnoreCase))
            {
                return SyncContentType.VIDEO;
            }
            else if (Regex.IsMatch(fileName, @"(.*?)\.(jpg|jpeg|png|bmp|gif)$", RegexOptions.IgnoreCase))
            {
                return SyncContentType.IMAGE;
            }
            else
            {
                return SyncContentType.OTHER;
            }
        }

        /// <summary>
        /// 读取音频文件的专辑图片
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>byte数组</returns>
        public static byte[] GetAudioFileAlbum(string filePath)
        {
            try
            {
                TagLib.File file = TagLib.File.Create(filePath);
                var firstPicture = file.Tag.Pictures.FirstOrDefault();
                if (firstPicture != null)
                {
                    return firstPicture.Data.Data;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
