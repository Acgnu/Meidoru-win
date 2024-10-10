namespace AlidnsLib
{
    public class DnsRecord
    {
        public string RecordId { get; set; }
        public string RR { get; set; }
        public int TTL { get; set; }
        public string Value { get; set; }
        public string Status { get; set; }
        public long UpdateTimestamp { get; set; }
        public string DomainName { get; set; }
        public string Line { get; set; }
        public string Type { get; set; }
    }
}
