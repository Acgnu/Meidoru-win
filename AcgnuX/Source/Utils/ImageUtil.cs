using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// 图片处理工具类
    /// </summary>
    public class ImageUtil
    {
        /// <summary>
        /// 将bmpimg转换为 System.Drawing.Bitmap
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        /// <summary>
        /// 返回不占用文件的 BitmapImage
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage GetBitmapImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// 返回不占用文件的 BitmapImage
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static BitmapImage GetBitmapImage(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return GetBitmapImageFromStream(stream);
        }

        public static BitmapImage GetBitmapImageFromStream(Stream stream)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// 创建图片缩略图
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="h">高度px</param>
        /// <param name="w">宽度px</param>
        /// <returns></returns>
        public static Image CreateThumbnail(string filePath, int h, int w)
        {
            try
            {
                Bitmap myBitmap = new Bitmap(filePath);
                Image thumbnail = myBitmap.GetThumbnailImage(w, h, new Image.GetThumbnailImageAbort(() => false), IntPtr.Zero);
                return thumbnail;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        /// <summary>
        /// byte 转 Image
        /// </summary>
        /// <param name="iamgebytes"></param>
        /// <returns></returns>
        public static Image ByteArrayToImage(byte[] iamgebytes)
        {
            MemoryStream ms = new MemoryStream(iamgebytes);
            Image image = Image.FromStream(ms);
            return image;
        }

        /// <summary>
        /// Image 转 byte
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, GetImageFormat(image.RawFormat));
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 返回文件格式
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private static ImageFormat GetImageFormat(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Jpeg)) return ImageFormat.Jpeg;
            else if (format.Equals(ImageFormat.Bmp)) return ImageFormat.Bmp;
            else if (format.Equals(ImageFormat.Gif)) return ImageFormat.Gif;
            else if (format.Equals(ImageFormat.Icon)) return ImageFormat.Icon;
            else return ImageFormat.Png;
        }

        /// <summary>
        /// 校验图片是否损坏
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true 文件有效</returns>
        public static bool CheckImgIsValid(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                var bitmap = GetBitmapImage(path);
                if (null == bitmap)
                {
                    GC.Collect();
                    return false;
                }
                bitmap = null;
                GC.Collect();
                return true;
            }
            catch (Exception)
            {
                GC.Collect();
            }
            return false;
        }

        /// <summary>
        /// bitmap转imagesource, 例如可将Resources中的Image转换成XAML可用的图片源
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            MemoryStream memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            ImageSourceConverter converter = new ImageSourceConverter();
            ImageSource source = (ImageSource)converter.ConvertFrom(memory);
            return source;
        }

        /// <summary>
        /// 从文件夹中随机读取一张背景图片
        /// </summary>
        /// <returns></returns>
        public static System.Windows.Media.Brush LoadImageAsBrush(string fileFullPath, double opacity, int decodeH, int decodeW)
        {
            if (!File.Exists(fileFullPath)) return null;

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelHeight = decodeH;
            bi.DecodePixelWidth = decodeW;
            bi.UriSource = new Uri(fileFullPath, UriKind.Absolute);
            bi.EndInit();
            return new ImageBrush(bi)
            {
                TileMode = TileMode.None,
                Stretch = Stretch.UniformToFill,
                Opacity = opacity
            };
        }

        /// <summary>
        /// 从ImageBrush中读取文件字节数组
        /// </summary>
        /// <param name="imageBrush"></param>
        /// <returns></returns>
        public static byte[] ImageBrushToByte(ImageBrush imageBrush)
        {
            var bitmapImage = imageBrush.ImageSource as BitmapImage;
            byte[] resultBytes = new byte[bitmapImage.StreamSource.Length];
            using (var stream = bitmapImage.StreamSource)
            {
                stream.Read(resultBytes, 0, (int)bitmapImage.StreamSource.Length - 1);
            }
            return resultBytes;
        }
    }
}
