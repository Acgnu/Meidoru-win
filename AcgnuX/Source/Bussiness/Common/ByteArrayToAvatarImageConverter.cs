using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    defaultAvatar = new BitmapImage(new Uri(
                    string.Format("pack://application:,,,/{0};component/Assets/Images/avatar_default.jpg", System.Reflection.Assembly.GetEntryAssembly().GetName().Name),
                    UriKind.Absolute));
                }
                return defaultAvatar;
            }
            set => defaultAvatar = value;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value || ((byte[])value).Length == 0)
            {
                return DefaultAvatar;
            }
            return ImageUtil.GetBitmapImage((byte[])value);
            //return ImageUtil.GetBitmapImage($"#{value}");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
