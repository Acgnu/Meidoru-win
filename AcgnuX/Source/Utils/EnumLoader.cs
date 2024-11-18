using EnumsNET;
using System.ComponentModel;

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
        public static string GetDesc(Enum enumValue)
        {
            var enumType = enumValue.GetType();
            var enumAttr = Enums.GetAttributes(enumType, enumValue);
            var desc = enumAttr.Get<DescriptionAttribute>();
            return desc.Description;
            //FieldInfo[] fieldinfo = source.GetFields();
            //foreach (FieldInfo item in fieldinfo)
            //{
            //    if (item.Name != enumName) continue;
            //    Object[] obj = item.GetCustomAttributes(typeof(DescriptionAttribute), false);
            //    if (obj.Length == 0) continue;
            //    DescriptionAttribute desc = (DescriptionAttribute)obj[0];
            //    return desc.Description;
            //}
            //return enumName;
        }

        public static int GetValue()
        {
            return 0;
        }

        /// <summary>
        /// 根据枚举值获得枚举
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Enum GetByValue(Type enumType, string value)
        {
            return Enums.Parse(enumType, value) as Enum;
        }
    }
}