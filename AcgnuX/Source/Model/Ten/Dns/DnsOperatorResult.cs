using System.Collections.Generic;

namespace AcgnuX.Source.Model.Ten.Dns
{
    public class DnsOperatorResult
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<DnsOptResultData> data { get; set; }
    }
}