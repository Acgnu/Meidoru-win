using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AcgnuX.Source.Bussiness.Common
{
    /// <summary>
    /// 字符截断转换器, 可以把过长的字符串截断成需要的长度
    /// </summary>
    public class StringTruncateConverter : BaseValueConverter<StringTruncateConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var len = System.Convert.ToInt32(parameter.ToString());    //ConverterParameter中传入的允许最大的字符长度, 必传
            var stringValue = value as string; //字符值
            return stringValue.Length > len ? stringValue.Substring(0, len) + "..." : stringValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
