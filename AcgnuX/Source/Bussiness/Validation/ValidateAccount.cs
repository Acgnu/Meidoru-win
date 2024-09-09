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
    public class ValidateAccount : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var bindingGroup = value as BindingGroup;
            //0=sourceObj (需要是DataContext)
            var sourceObj = bindingGroup.Items[0];

            object site;
            object uname;
            object upass;

            _ = bindingGroup.TryGetValue(sourceObj, "Site", out site);
            _ = bindingGroup.TryGetValue(sourceObj, "Uname", out uname);
            _ = bindingGroup.TryGetValue(sourceObj, "Upass", out upass);

            if (string.IsNullOrEmpty((string)site))
            {
                return new ValidationResult(false, "需要填写站点哦");
            }

            if (string.IsNullOrEmpty((string) uname))
            {
                return new ValidationResult(false, "需要填写账号哦");
            }

            if (string.IsNullOrEmpty((string) upass))
            {
                return new ValidationResult(false, "需要填写密码哦");
            }
            return ValidationResult.ValidResult;
        }
    }
}
