using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlidnsLib
{
    public class AlidnsRequest
    {
        public string HttpMethod { get; }
        public string CanonicalUri { get; }
        public string Host { get; }
        public string XAcsAction { get; }
        public string XAcsVersion { get; }
        public SortedDictionary<string, object> Headers { get; } = new SortedDictionary<string, object>(StringComparer.Ordinal);
        public string Body { get; set; }
        public SortedDictionary<string, object> QueryParam { get; } = new SortedDictionary<string, object>(StringComparer.Ordinal);

        public AlidnsRequest(string httpMethod, string canonicalUri, string host, string xAcsAction, string xAcsVersion)
        {
            HttpMethod = httpMethod;
            CanonicalUri = canonicalUri;
            Host = host;
            XAcsAction = xAcsAction;
            XAcsVersion = xAcsVersion;
            InitBuilder();
        }

        private void InitBuilder()
        {
            Headers["host"] = Host;
            Headers["x-acs-action"] = XAcsAction;
            Headers["x-acs-version"] = XAcsVersion;
            Headers["x-acs-date"] = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            Headers["x-acs-signature-nonce"] = Guid.NewGuid().ToString();
        }
    }
}
