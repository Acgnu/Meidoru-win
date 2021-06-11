using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Model.Ten.Dns
{
    class TenApiSecret
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
    }
}
