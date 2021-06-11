using System;
using System.Text;

namespace AcgnuX.Utils
{
    class RandomUtil
    {
        public static string makeSring(bool b, int n)//b：是否有复杂字符，n：生成的字符串长度
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
    }
}
