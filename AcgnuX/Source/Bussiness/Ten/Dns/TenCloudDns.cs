using AcgnuX.Source.Bussiness.Ten.Dns;
using AcgnuX.Source.Model;
using AcgnuX.Source.Model.Ten.Dns;
using AcgnuX.Utils;
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

        /// <summary>
        /// 删除DNS解析记录
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="callBackActoin"></param>
        /// <returns></returns>
        public async Task<DnsOperatorResult> DeleteRecordAsync(int recordId)
        {
            SortedDictionary<string, string> urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "recordId", Convert.ToString(recordId));
            return await CreateTenAccessAsync<DnsOperatorResult>("RecordDelete", CVM_REQUEST_ADDR, urlArgs);
        }

        //private void addSubDomain(string subDomain)
        //{
        //    TreeMap<string, string> urlArgs = new TreeMap<>();
        //    putUrlArg(urlArgs, "domain", subDomain);
        //    //        string createResult = createTenAccess("DomainCreate", CVM_REQUEST_ADDR, urlArgs);
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
        private async Task<T> CreateTenAccessAsync<T>(string action, string apiAddr, SortedDictionary<string, string> urlArgs) where T : new()
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
        private string createSignature(string method, string apiAddr, SortedDictionary<string, string> treeMap)
        {
            //step.1 sort ,tree map does'n t need sort
            //step 2 create url parameter
            string paramStr = RequestUtil.ConcatQueryString(treeMap);
            string accessToSign = method + CNS_DOMAIN + apiAddr + "?" + paramStr;
            return AlgorithmUtil.ToHMACSHA1(accessToSign, mTenDnsApiSecret.SecretKey);
        }

        /**
         * 查询域名解析记录
         * @param domain  主域名
         * @param subDomain  子域名
         * @param recordType  记录值类型  A、CNAME、MX、NS, TXT
         * @return
         */
        public async Task<DnsRecordResult> QueryRecordsAsync(string subDomain, string recordType)
        {
            var urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "subDomain", subDomain);
            PutUrlArg(urlArgs, "recordType", recordType);
            return await CreateTenAccessAsync<DnsRecordResult>("RecordList", CVM_REQUEST_ADDR, urlArgs);
        }

        /**
         * 获取客户端外网ip
         */
        //    private string getClientIP(){
        ////        return "106.52.125.163";
        //        string gatway = "http://www.yxxrui.cn/yxxrui_cabangs_api/myip.ashx";
        //        string clientIp = PrcUtils.doHttpGet(gatway);
        //        System.out.println(clientIp);
        //        return clientIp;
        //    }

        private void PutUrlArg(SortedDictionary<string, string> treeMap, string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            treeMap.Add(key, value);
        }
    }
}
