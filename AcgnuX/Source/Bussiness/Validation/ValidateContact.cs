using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace AcgnuX.Source.Bussiness.Validation
{
    public class ValidateContact : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var bindingGroup = value as BindingGroup;
            //0=sourceObj (需要是DataContext)
            var sourceObj = bindingGroup.Items[0];

            object uid;
            object name;

            _ = bindingGroup.TryGetValue(sourceObj, "Name", out name);
            _ = bindingGroup.TryGetValue(sourceObj, "Uid", out uid);

            if (string.IsNullOrEmpty((string)name))
            {
                return new ValidationResult(false, "名称未填写");
            }

            if (string.IsNullOrEmpty((string)uid))
            {
                return new ValidationResult(false, "UID未填写");
            }
            return ValidationResult.ValidResult;
        }
    }
}
