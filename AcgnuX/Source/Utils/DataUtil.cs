using AcgnuX.Source.Model;
using System;
using System.Text.RegularExpressions;

namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// 一般的数据处理工具类
    /// </summary>
    public class DataUtil
    {
        /// <summary>
        /// 简单的解析普通的xml格式文档, 单节点
        /// </summary>
        /// <param name="source">接口返回的原始字符串</param>
        /// <param name="key">需要解析的键</param>
        /// <returns>值</returns>
        public static string GetXmlNodeValue(string source, string key)
        {
            string keyOpen = "<" + key + ">";
            string keyClose = "</" + key + ">";
            var value = source.Substring(source.IndexOf(keyOpen) + keyOpen.Length, source.LastIndexOf(keyClose) - source.IndexOf(keyOpen) - keyOpen.Length);
            return value;
        }

        /// <summary>
        /// 将77乐谱解析到对象
        /// </summary>
        /// <param name="source">接口返回的数据源</param>
        /// <returns>Qumusic</returns>
        public static Tan8music ParseToModel(string source)
        {
            return new Tan8music()
            {
                yp_create_time = Convert.ToUInt32(GetXmlNodeValue(source, "yp_create_time")),
                yp_title = GetXmlNodeValue(source, "yp_title"),
                yp_page_count = Convert.ToByte(GetXmlNodeValue(source, "yp_page_count")),
                ypad_url = GetXmlNodeValue(source, "ypad_url"),
                ypad_url2 = GetXmlNodeValue(source, "ypad_url2").Replace("ypa2", "ypdx"),
                yp_page_width = Convert.ToInt16(GetXmlNodeValue(source, "yp_page_width")),
                yp_page_height = Convert.ToInt16(GetXmlNodeValue(source, "yp_page_height")),
                yp_is_dadiao = Convert.ToByte(GetXmlNodeValue(source, "yp_is_dadiao")),
                yp_key_note = Convert.ToByte(GetXmlNodeValue(source, "yp_key_note")),
                yp_is_yanyin = Convert.ToByte(GetXmlNodeValue(source, "yp_is_yanyin"))
            };
        }

        /// <summary>
        /// 判断字符串是否为数字
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true 是</returns>
        public static bool IsNum(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
    }
}
