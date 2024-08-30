using System;
using System.Text;

namespace SharedLib.Utils
{
    public class RandomUtil
    {
        /// <summary>
        /// b：是否有复杂字符，n：生成的字符串长度
        /// </summary>
        /// <param name="b"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string MakeSring(bool b, int n)
        {
            string str = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (b)
            {
                str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";//复杂字符
            }
            StringBuilder sbuilder = new StringBuilder();
            Random rd = new Random();
            for (int i = 0; i < n; i++)
            {
                sbuilder.Append(str.Substring(rd.Next(0, str.Length), 1));
            }
            return sbuilder.ToString();
            //return "MNVUbM";
        }
        
        /// <summary>
        /// 获取指定范围内的随机整数
        /// </summary>
        /// <param name="start">开始范围</param>
        /// <param name="end">结束范围</param>
        /// <returns></returns>
        public static int GetRangeRandomNum(int start , int end)
        {
            Random rand = new Random();
            return rand.Next(start, end);
        }

        /// <summary>
        /// 从传入的项目中随机获取一个
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static string GetRandomItem(string[] items)
        {
            return items[GetRangeRandomNum(0, items.Length - 1)];
        }
    }
}
