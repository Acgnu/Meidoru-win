using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.Common
{
    public class StringToImageConverter : BaseValueConverter<StringToImageConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(string.IsNullOrEmpty(value.ToString()))
            {
                return ApplicationConstant.GetDefaultSheetCover();
            }
            return ImageUtil.GetBitmapImage($"#{value}");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
