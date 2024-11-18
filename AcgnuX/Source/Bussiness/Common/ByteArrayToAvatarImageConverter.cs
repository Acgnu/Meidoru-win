using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace AcgnuX.Source.Bussiness.Common
{
    public class ByteArrayToAvatarImageConverter : BaseValueConverter<ByteArrayToAvatarImageConverter>
    {
        /// <summary>
        /// 默认头像
        /// </summary>
        private BitmapImage defaultAvatar;

        /// <summary>
        ///  获取/设置 QQ图标
        /// </summary>
        private BitmapImage DefaultAvatar
        {
            get
            {
                if (null == defaultAvatar)
                {
                    var uri = new Uri("../../Assets/Images/avatar_default.jpg", UriKind.Relative);
                    using (var fileStream = App.GetResourceStream(uri).Stream)
                    {
                        defaultAvatar = ImageUtil.GetBitmapImageFromStream(fileStream);
                        return defaultAvatar;
                    }
                }
                return defaultAvatar;
            }
            set => defaultAvatar = value;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (null == value || ((byte[])value).Length == 0)
            //{
            //    return DefaultAvatar;
            //}
            //return ImageUtil.GetBitmapImage((byte[])value);
            //return ImageUtil.GetBitmapImage($"#{value}");
            byte[] bytes;
            if (value is byte[] v)
            {
                bytes = v;
            }
            else if (value is ByteArray ba)
            {
                bytes = ba.Data;
            }
            else
            {
                bytes = new byte[0];
            }
            Task<BitmapImage> task;
            if (bytes == null || bytes.Count() == 0)
            {
                task = Task.Run(() =>
                {
                    return DefaultAvatar;
                    //var stream = FileUtil.GetApplicationResourceAsStream(@"../../Assets/Images/avatar_default.jpg");
                    //return ImageUtil.GetBitmapImageFromStream(stream.Stream);
                });
            }
            else
            {
                task = Task.Run(() =>
                {
                    return ImageUtil.GetBitmapImage(bytes);
                });
            }
            return new TaskCompletionNotifier<BitmapImage>(task);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
