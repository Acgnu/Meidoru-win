using AcgnuX.Model.Ten.Dns;
using AcgnuX.Source.Model.Ten.Dns;
using System.Collections.Generic;

namespace AcgnuX.Source.Bussiness.Ten.Dns
{
    public class DnsMainData
    {
        public DnsDomain domain { get; set; }
        public List<DnsRecord> records { get; set; }
    }
}