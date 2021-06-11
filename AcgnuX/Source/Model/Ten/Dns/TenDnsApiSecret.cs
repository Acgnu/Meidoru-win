using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Model.Ten.Dns
{
    class TenDnsApiSecret : TenApiSecret
    {
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
