using AcgnuX.Model.Ten.Dns;
using AcgnuX.Source.Model;
using AcgnuX.Source.Model.Ten.Dns;
using AcgnuX.Utils;
using AcgnuX.Utils.Crypto;
using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AcgnuX.Bussiness.Ten.Dns
{
    public class TenCloudDns
    {
        private static readonly string CNS_DOMAIN = "cns.api.qcloud.com";
        private static readonly string CVM_REQUEST_ADDR = "/v2/index.php";
        private static readonly string PROTOCOL = "https://";
        private readonly AppSecretKey mTenDnsApiSecret;

        public TenCloudDns(AppSecretKey secret)
        {
            mTenDnsApiSecret = secret;
        }

        //public void testDelDnsRecord()
        //{
        //    delDNSRecord("");
        //}

        //public void testAddSubDomain()
        //{
        //    addSubDomain("");
        //}

        //public void testModifyDNS(String ip)
        //{
        //    modifyDNS(ip, "416075548");
        //}

        /// <summary>
        /// 删除DNS解析记录
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="callBackActoin"></param>
        /// <returns></returns>
        public async Task<DnsOperatorResult> DeleteRecordAsync(string recordId)
        {
            SortedDictionary<string, string> urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "recordId", recordId);
            return await CreateTenAccessAsync<DnsOperatorResult>("RecordDelete", CVM_REQUEST_ADDR, urlArgs);
        }

        //private void addSubDomain(String subDomain)
        //{
        //    TreeMap<String, String> urlArgs = new TreeMap<>();
        //    putUrlArg(urlArgs, "domain", subDomain);
        //    //        String createResult = createTenAccess("DomainCreate", CVM_REQUEST_ADDR, urlArgs);
        //    //        System.out.println(createResult);
        //}
        public async Task<DnsOperatorResult> CreateRecordAsync(string name, string type, string line, string value)
        {
            var urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "subDomain", name);
            PutUrlArg(urlArgs, "recordType", type);
            PutUrlArg(urlArgs, "recordLine", line);
            PutUrlArg(urlArgs, "value", value);
            return await CreateTenAccessAsync<DnsOperatorResult>("RecordCreate", CVM_REQUEST_ADDR, urlArgs);
        }

        /// <summary>
        /// 异步创建凭证
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="apiAddr"></param>
        /// <param name="urlArgs"></param>
        /// <returns></returns>
        private async Task<T> CreateTenAccessAsync<T>(String action, String apiAddr, SortedDictionary<string, string> urlArgs) where T : new()
        {
            if (null == urlArgs)
            {
                urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            }
            var nonce = RandomUtil.MakeSring(false, 6);
            PutUrlArg(urlArgs, "Action", action);
            PutUrlArg(urlArgs, "Timestamp", TimeUtil.CurrentMillis() / 1000 + "");
            PutUrlArg(urlArgs, "Nonce", nonce);
            PutUrlArg(urlArgs, "SecretId", mTenDnsApiSecret.SecretId);

            var signature = createSignature("POST", apiAddr, urlArgs);
            PutUrlArg(urlArgs, "Signature", signature);

            var response = await RequestUtil.TaskFormRequestAsync<T>(PROTOCOL + CNS_DOMAIN + apiAddr, urlArgs, System.Net.Http.HttpMethod.Post);

            if (!response.IsSuccessStatusCode)
            {
                return default;
            }
            return await HttpContentJsonExtensions.ReadFromJsonAsync<T>(response.Content); ;
        }

        /// <summary>
        ///  修改DNS解析记录
        /// </summary>
        /// <param name="dnsDomain"></param>
        /// <param name="dnsRecord"></param>
        /// <param name="callBackActoin"></param>
        /// <returns></returns>
        public async Task<DnsOperatorResult> ModifyRecordAsync(int id, string name, string type, string line, string value)
        {
            SortedDictionary<string, string> urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "recordId", Convert.ToString(id));
            PutUrlArg(urlArgs, "subDomain", name);
            PutUrlArg(urlArgs, "recordType", type);
            PutUrlArg(urlArgs, "recordLine", line);
            PutUrlArg(urlArgs, "value", value);
            //        putUrlArg(urlArgs,"ttl", "10");
            //        putUrlArg(urlArgs,"mx", "1");
            return await CreateTenAccessAsync<DnsOperatorResult>("RecordModify", CVM_REQUEST_ADDR, urlArgs);
        }

        /**
         * 生成签名字符串
         * @param method
         * @param apiAddr
         * @param treeMap
         * @return
         */
        private String createSignature(String method, String apiAddr, SortedDictionary<String, String> treeMap)
        {
            //step.1 sort ,tree map does'n t need sort
            //step 2 create url parameter
            String paramStr = RequestUtil.ConcatQueryString(treeMap);
            String accessToSign = method + CNS_DOMAIN + apiAddr + "?" + paramStr;
            return AlgorithmUtil.ToHMACSHA1(accessToSign, mTenDnsApiSecret.SecretKey);
        }

        /**
         * 查询域名解析记录
         * @param domain  主域名
         * @param subDomain  子域名
         * @param recordType  记录值类型  A、CNAME、MX、NS, TXT
         * @return
         */
        public async Task<T> QueryRecordsAsync<T>(String subDomain, String recordType) where T : new()
        {
            var urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "subDomain", subDomain);
            PutUrlArg(urlArgs, "recordType", recordType);
            return await CreateTenAccessAsync<T>("RecordList", CVM_REQUEST_ADDR, urlArgs);
        }

        /**
         * 获取客户端外网ip
         */
        //    private String getClientIP(){
        ////        return "106.52.125.163";
        //        String gatway = "http://www.yxxrui.cn/yxxrui_cabangs_api/myip.ashx";
        //        String clientIp = PrcUtils.doHttpGet(gatway);
        //        System.out.println(clientIp);
        //        return clientIp;
        //    }

        //public JSONObject analyseAndGetData(JSONObject fullresult, String dataKey) throws RuntimeException
        //{
        //    if (0 == fullresult.getInteger("code") && "success".equalsIgnoreCase(fullresult.getString("codeDesc"))) {
        //        return fullresult.getJSONObject(Optional.ofNullable(dataKey).orElse("data"));
        //    }
        //    throw new BizException(MessageFormat.format("{0}({1}) : {2}", fullresult.getString("codeDesc"), fullresult.getInteger("code"), fullresult.getString("message")));
        //}

        private void PutUrlArg(SortedDictionary<string, string> treeMap, String key, String value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            treeMap.Add(key, value);
        }
    }
}
