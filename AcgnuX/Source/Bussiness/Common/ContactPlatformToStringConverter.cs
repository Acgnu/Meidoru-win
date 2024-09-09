using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AcgnuX.Source.Bussiness.Common
{
    /// <summary>
    /// 联系人平台枚举转图片转换器
    /// </summary>
    public class ContactPlatformToImageSourceConverter : BaseValueConverter<ContactPlatformToImageSourceConverter>
    {
        /// <summary>
        /// QQ图标
        /// </summary>
        private BitmapImage bitmapQQ;
        /// <summary>
        /// 微信图标
        /// </summary>
        private BitmapImage bitmapWE;


        /// <summary>
        ///  获取/设置 QQ图标
        /// </summary>
        private BitmapImage BitmapQQ 
        {
            get {
                if(null == bitmapQQ)
                {
                    bitmapQQ = new BitmapImage(new Uri(
                    string.Format("pack://application:,,,/{0};component/Assets/Images/icon_qq.png", System.Reflection.Assembly.GetEntryAssembly().GetName().Name),
                    UriKind.Absolute));
                }
                return bitmapQQ;
            }
            set => bitmapQQ = value;
        }

        /// <summary>
        /// 获取/设置 微信图标
        /// </summary>
        private BitmapImage BitmapWE
        {
            get { 
                if(null == bitmapWE)
                {
                    bitmapWE = new BitmapImage(new Uri(
                     string.Format("pack://application:,,,/{0};component/Assets/Images/icon_we.png", System.Reflection.Assembly.GetEntryAssembly().GetName().Name),
                     UriKind.Absolute));
                }
                return bitmapWE; 
            }
            set => bitmapWE = value;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var platform = (ContactPlatform) value;
            switch (platform)
            {
                case ContactPlatform.QQ:
                    return BitmapQQ;
                case ContactPlatform.WE:
                    return BitmapWE;
                default:
                    throw new NotImplementedException();
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
