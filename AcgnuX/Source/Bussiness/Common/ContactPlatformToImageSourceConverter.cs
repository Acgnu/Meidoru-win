using AcgnuX.Source.Bussiness.Constants;
using System;
using System.Globalization;

namespace AcgnuX.Source.Bussiness.Common
{
    /// <summary>
    /// 联系人枚举字符转换器
    /// </summary>
    public class ContactPlatformToStringConverter : BaseValueConverter<ContactPlatformToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(targetType, value.ToString());
        }
    }
}
