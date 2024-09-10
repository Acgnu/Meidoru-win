using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace AcgnuX.Source.Bussiness.Validation
{
    public class ValidateSyncPath : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var bindingGroup = value as BindingGroup;
            //0=sourceObj (需要是DataContext)
            var sourceObj = bindingGroup.Items[0];

            object pcPath;
            object moblePath;

            _ = bindingGroup.TryGetValue(sourceObj, "PcPath", out pcPath);
            _ = bindingGroup.TryGetValue(sourceObj, "MobilePath", out moblePath);

            if (string.IsNullOrEmpty((string)pcPath))
            {
                return new ValidationResult(false, "需要填写电脑端目录");
            }

            if (string.IsNullOrEmpty((string)moblePath))
            {
                return new ValidationResult(false, "需要填写移动端目录");
            }
            return ValidationResult.ValidResult;
        }
    }
}
