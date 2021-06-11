using System;
using System.Security.Cryptography;
using System.Text;

namespace AcgnuX.Utils.Crypto
{
    class AlgorithmUtil
    {
        public static string ToHMACSHA1(string encryptText, string encryptKey)
        {
            //HMACSHA1加密
            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.UTF8.GetBytes(encryptKey);
            byte[] dataBuffer = Encoding.UTF8.GetBytes(encryptText);
            byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
