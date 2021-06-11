namespace AcgnuX.Model.Ten.Dns
{
    public class DnsRecord
    {
        public int? id { get; set; }
        public int ttl { get; set; }
        public string Value { get; set; }
        public int enabled { get; set; }
        public string status { get; set; }
        public string updated_on { get; set; }
        public int q_project_id { get; set; }
        public string Name { get; set; }
        public string line { get; set; }
        public string line_id { get; set; }
        public string type { get; set; }
        public string remark { get; set; }
        public int mx { get; set; }
        public string hold { get; set; }
    }
}
