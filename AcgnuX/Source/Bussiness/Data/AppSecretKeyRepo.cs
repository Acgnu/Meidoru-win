using AcgnuX.Source.Model;
using SharedLib.Utils;

namespace AcgnuX.Source.Bussiness.Data
{
    /// <summary>
    /// 表 app_secret_key repo
    /// </summary>
    public class AppSecretKeyRepo
    {
        /// <summary>
        /// 根据平台查询配置
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public AppSecretKey FindByPlatform(string platform)
        {
            var dbSecret = SQLite.SqlRow(
                string.Format("SELECT secret_id, secret_key, priv_domain, priv_sub_domain, Platform FROM app_secret_keys WHERE platform = '{0}'",
                platform));
            if (null != dbSecret && dbSecret.Length > 0)
            {
                return new AppSecretKey()
                {
                    SecretId = dbSecret[0],
                    SecretKey = dbSecret[1],
                    PrivDomain = dbSecret[2],
                    PrivSubDomain = dbSecret[3],
                    Platform = dbSecret[4]
                };
            }
            return null;
        }
    }
}
