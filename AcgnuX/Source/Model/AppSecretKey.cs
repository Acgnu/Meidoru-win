using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 秘钥配置
    /// </summary>
    public class AppSecretKey
    {
        /// <summary>
        /// 腾讯私钥ID
        /// </summary>
        public string SecretId { get; set; }
        /// <summary>
        /// 腾讯私钥
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// 平台
        /// </summary>
        public string Platform { get; set; }
        /// <summary>
        /// 主域名
        /// </summary>
        public string PrivDomain { get; set; }
        /// <summary>
        /// 私有二级域名
        /// </summary>
        public string PrivSubDomain { get; set; }
    }
}
