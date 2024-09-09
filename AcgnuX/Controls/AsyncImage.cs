using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using AcgnuX.Source.Utils;
using AcgnuX.Source.Model;
using System.Windows.Media;

namespace AcgnuX.Controls
{
    /// <summary>
    /// 异步图片控件
    /// </summary>
    public class AsyncImage : Image
    {
        /// <summary>
        /// 图片路径属性
        /// </summary>
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register(
                nameof(ImagePath),
                typeof(string),
                typeof(AsyncImage),
                new PropertyMetadata(async (o, e) =>
                    await ((AsyncImage)o).LoadImageAsync((string)e.NewValue)));

        /// <summary>
        /// 图片字节属性
        /// </summary>
        public static readonly DependencyProperty ImageBytesProperty =
            DependencyProperty.Register(
                nameof(ImageBytes),
                typeof(ByteArray),
                typeof(AsyncImage),
                new PropertyMetadata(async (o, e) =>
                    await ((AsyncImage)o).LoadImageBytesAsync((ByteArray)e.NewValue)));


        /// <summary>
        /// 图片路径, 适用于以文件路径创建
        /// </summary>
        public string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        /// <summary>
        /// 图片字节, 适用于以文件字节创建
        /// </summary>
        public ByteArray ImageBytes
        {
            get { return (ByteArray)GetValue(ImageBytesProperty); }
            set { SetValue(ImageBytesProperty, value); }
        }

        /// <summary>
        /// 异步读取文件路径
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private async Task LoadImageAsync(string imagePath)
        {
            Source = await Task.Run(() =>
            {
                if (!File.Exists(imagePath)) return null;

                return ImageUtil.GetBitmapImage(imagePath);
            });
        }

        /// <summary>
        /// 异步读取文件字节
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        private async Task LoadImageBytesAsync(ByteArray byteArray)
        {
            Source = await Task.Run(() =>
            {
                if (null == byteArray || byteArray.IsEmpty())
                {
                    return null;
                }
                BitmapImage readedSource = null;
                try
                {
                    readedSource = ImageUtil.GetBitmapImage(byteArray.Data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("字节数组转BitmapImage异常");
                    Console.WriteLine(ex);
                }
                return readedSource;
            });
        }
    }
}
