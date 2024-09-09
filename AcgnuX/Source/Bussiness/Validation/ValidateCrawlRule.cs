using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace AcgnuX.Source.Bussiness.Validation
{
    public class ValidateCrawlRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var bindingGroup = value as BindingGroup;
            var sourceObj = bindingGroup.Items[0];

            var name = bindingGroup.GetValue(sourceObj, "Name");
            var url = bindingGroup.GetValue(sourceObj, "Url");
            var partten = bindingGroup.GetValue(sourceObj, "Partten");
            var maxPage = bindingGroup.GetValue(sourceObj, "MaxPage");

            if (string.IsNullOrEmpty((string)name))
            {
                return new ValidationResult(false, "名称未填写");
            }

            if (string.IsNullOrEmpty((string)url))
            {
                return new ValidationResult(false, "站点未填写");
            }

            if (string.IsNullOrEmpty((string)partten))
            {
                return new ValidationResult(false, "匹配规则未填写");
            }

            if (string.IsNullOrEmpty((string)maxPage))
            {
                return new ValidationResult(false, "抓取页数未填写");
            }

            return ValidationResult.ValidResult;
        }
    }
}
