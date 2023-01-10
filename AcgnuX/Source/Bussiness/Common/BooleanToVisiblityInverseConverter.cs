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
    /// 反向bool转visibile
    /// true => Visibility.Collapsed
    /// false => Visibility.Visible
    /// </summary>
    public class BooleanToVisiblityInverseConverter : BaseValueConverter<BooleanToVisiblityInverseConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return (bool)value ? Visibility.Collapsed : Visibility.Visible;
            else
                return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
