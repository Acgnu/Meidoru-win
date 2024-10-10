using AcgnuX.Utils;
using SharedLib.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AlidnsLib
{
    public class AlidnsClient
    {
        private const string Algorithm = "ACS3-HMAC-SHA256";
        private readonly string AccessKeyId;
        private readonly string AccessKeySecret;
        private readonly string Domain;
        private readonly bool Debug = false;

        public AlidnsClient(string key, string secret, string domain, bool debug)
        {
            AccessKeyId = key;
            AccessKeySecret = secret;
            Domain = domain;
            Debug = debug;
        }

        /// <summary>
        /// 生成签名信息
        /// </summary>
        /// <param name="request"></param>
        private void GetAuthorization(AlidnsRequest request)
        {
            try
            {
                // Step 1: Concatenate the canonical request string
                var canonicalQueryString = string.Join("&", request.QueryParam
                    .Select(kvp => $"{PercentEncode(kvp.Key)}={PercentEncode(kvp.Value.ToString())}"));

                string requestPayload = request.Body ?? "";
                string hashedRequestPayload = AlgorithmUtil.Sha256Hex(requestPayload);
                request.Headers["x-acs-content-sha256"] = hashedRequestPayload;

                var canonicalHeaders = new StringBuilder();
                var signedHeadersSb = new StringBuilder();

                foreach (var header in request.Headers.Where(h => h.Key.ToLower().StartsWith("x-acs-") ||
                                                                  h.Key.ToLower() == "host" ||
                                                                  h.Key.ToLower() == "content-type")
                                                       .OrderBy(h => h.Key.ToLower()))
                {
                    canonicalHeaders.Append($"{header.Key.ToLower()}:{header.Value.ToString().Trim()}\n");
                    signedHeadersSb.Append($"{header.Key.ToLower()};");
                }

                string signedHeaders = signedHeadersSb.ToString().TrimEnd(';');
                string canonicalRequest = $"{request.HttpMethod}\n{request.CanonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{hashedRequestPayload}";
                WriteDebugLine($"canonicalRequest=========>\n{canonicalRequest}");

                // Step 2: Concatenate the string to be signed
                string hashedCanonicalRequest = AlgorithmUtil.Sha256Hex(canonicalRequest);
                string stringToSign = $"{Algorithm}\n{hashedCanonicalRequest}";
                WriteDebugLine($"stringToSign=========>\n{stringToSign}");

                // Step 3: Calculate the signature
                string signature = BitConverter.ToString(AlgorithmUtil.Hmac256(Encoding.UTF8.GetBytes(AccessKeySecret), stringToSign)).Replace("-", "").ToLower();
                WriteDebugLine($"signature=========>{signature}");

                // Step 4: Concatenate the Authorization
                string authorization = $"{Algorithm} Credential={AccessKeyId},SignedHeaders={signedHeaders},Signature={signature}";
                WriteDebugLine($"authorization=========>{authorization}");
                request.Headers["Authorization"] = authorization;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get authorization");
                Console.WriteLine(e);
            }
        }

        private string PercentEncode(string value)
        {
            return Uri.EscapeDataString(value)
                ?.Replace("+", "%20")
                //.Replace("*", "%2A")
                .Replace("%7E", "~");
        }

        /// <summary>
        /// 调用API
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<T> CallApi<T>(AlidnsRequest request) where T : class
        {
            using (var httpClient = new HttpClient())
            {
                var uriBuilder = new UriBuilder($"https://{request.Host}{request.CanonicalUri}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                foreach (var param in request.QueryParam)
                {
                    query[param.Key] = param.Value.ToString();
                }
                uriBuilder.Query = query.ToString();

                HttpRequestMessage httpRequest;
                switch (request.HttpMethod)
                {
                    case "GET":
                        httpRequest = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
                        break;
                    case "POST":
                        httpRequest = new HttpRequestMessage(HttpMethod.Post, uriBuilder.Uri);
                        if (!string.IsNullOrEmpty(request.Body))
                        {
                            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
                        }
                        break;
                    case "DELETE":
                        httpRequest = new HttpRequestMessage(HttpMethod.Delete, uriBuilder.Uri);
                        break;
                    case "PUT":
                        httpRequest = new HttpRequestMessage(HttpMethod.Put, uriBuilder.Uri);
                        if (!string.IsNullOrEmpty(request.Body))
                        {
                            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
                        }
                        break;
                    default:
                        throw new ArgumentException("Unsupported HTTP method");
                }

                foreach (var header in request.Headers)
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToString());
                }
                var response = await httpClient.SendAsync(httpRequest);
                return await HttpContentJsonExtensions.ReadFromJsonAsync<T>(response.Content);
            }
        }

        private async Task<T> Execute<T>(AlidnsRequest request) where T : class
        {
            GetAuthorization(request);
            return await CallApi<T>(request);
        }

        /// <summary>
        /// 显示调试信息
        /// </summary>
        /// <param name="content"></param>
        private void WriteDebugLine(string content)
        {
            if (Debug)
            {
                Console.WriteLine(content);
            }
        }

        /// <summary>
        /// 创建DNS记录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="line"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<DnsOperatorResult> CreateRecordAsync(string name, string type, string line, string value)
        {         
            var request = CreateDefaultRequest("AddDomainRecord");

            request.QueryParam["DomainName"] = Domain;
            request.QueryParam["RR"] = name;
            request.QueryParam["Type"] = type;
            request.QueryParam["Value"] = value;

            return await Execute<DnsOperatorResult>(request);
        }

        /// <summary>
        /// 更新DNS记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="line"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<DnsOperatorResult> ModifyRecordAsync(string id, string name, string type, string line, string value)
        {
            var request = CreateDefaultRequest("UpdateDomainRecord");

            request.QueryParam["RR"] = name;
            request.QueryParam["RecordId"] = id;
            request.QueryParam["Type"] = type;
            request.QueryParam["Value"] = value;

            return await Execute<DnsOperatorResult>(request);
        }

        /// <summary>
        /// 查询DNS记录列表
        /// </summary>
        /// <returns></returns>
        public async Task<DnsRecordResult> QueryRecordsAsync()
        {
            var request = CreateDefaultRequest("DescribeDomainRecords");

            request.QueryParam["DomainName"] = Domain;
            request.QueryParam["PageSize"] = 500;

            return await Execute<DnsRecordResult>(request);
        }

        /// <summary>
        /// 删除DNS记录
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public async Task<DnsOperatorResult> DeleteRecordAsync(string recordId)
        {
            var request = CreateDefaultRequest("DeleteDomainRecord");

            request.QueryParam["RecordId"] = recordId;

            return await Execute<DnsOperatorResult>(request);
        }

        /// <summary>
        /// 初始化请求参数
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private AlidnsRequest CreateDefaultRequest(string action)
        {
            // RPC interface request
            string httpMethod = "POST";
            string canonicalUri = "/";
            string host = "alidns.aliyuncs.com";
            string xAcsAction = action;
            string xAcsVersion = "2015-01-09";
            var request = new AlidnsRequest(httpMethod, canonicalUri, host, xAcsAction, xAcsVersion);
            return request;
        }
    }
}

