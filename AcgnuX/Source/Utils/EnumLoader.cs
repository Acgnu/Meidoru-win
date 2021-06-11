using System;
using System.ComponentModel;
using System.Reflection;

namespace AcgnuX.Utils
{
    public class EnumLoader
    {
        /// 
        /// 读取枚举的Description
        ///  调用示例：DataTable dt = ConvertEnumToDataTable(typeof (BankCode));
        /// 枚举Type
        /// 需要读取
        /// 
        public static string GetEnumDesc(Type source, string enumName)
        {
            FieldInfo[] fieldinfo = source.GetFields();
            foreach (FieldInfo item in fieldinfo)
            {
                if (item.Name != enumName) continue;
                Object[] obj = item.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (obj.Length == 0) continue;
                DescriptionAttribute desc = (DescriptionAttribute)obj[0];
                return desc.Description;
            }
            return enumName;
        }
    }
}