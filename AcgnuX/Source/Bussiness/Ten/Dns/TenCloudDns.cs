﻿using AcgnuX.Model.Ten.Dns;
using AcgnuX.Source.Model.Ten.Dns;
using AcgnuX.Utils;
using AcgnuX.Utils.Crypto;
using RestSharp;
using System;
using System.Collections.Generic;

namespace AcgnuX.Bussiness.Ten.Dns
{
    class TenCloudDns
    {
        private static readonly string CNS_DOMAIN = "cns.api.qcloud.com";
        private static readonly string CVM_REQUEST_ADDR = "/v2/index.php";
        private static readonly string PROTOCOL = "https://";
        private TenDnsApiSecret mTenDnsApiSecret;

        public TenCloudDns(TenDnsApiSecret secret)
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
        public RestRequestAsyncHandle DelDNSRecord(string recordId, Action<IRestResponse<DnsOperatorResult>> callBackActoin)
        {
            SortedDictionary<string, string> urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "recordId", recordId);
            return createTenAccess("RecordDelete", CVM_REQUEST_ADDR, urlArgs, callBackActoin);
        }

        //private void addSubDomain(String subDomain)
        //{
        //    TreeMap<String, String> urlArgs = new TreeMap<>();
        //    putUrlArg(urlArgs, "domain", subDomain);
        //    //        String createResult = createTenAccess("DomainCreate", CVM_REQUEST_ADDR, urlArgs);
        //    //        System.out.println(createResult);
        //}

        public RestRequestAsyncHandle AddDNSRecord(DnsRecord dnsRecord, Action<IRestResponse<DnsOperatorResult>> callBackActoin)
        {
            var urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "subDomain", dnsRecord.Name);
            PutUrlArg(urlArgs, "recordType", dnsRecord.type);
            PutUrlArg(urlArgs, "recordLine", dnsRecord.line);
            PutUrlArg(urlArgs, "value", dnsRecord.Value);
            return createTenAccess("RecordCreate", CVM_REQUEST_ADDR, urlArgs, callBackActoin);
        }

        /**
         * 创建一个腾讯云连接
         * @param action
         * @param apiAddr
         * @param urlArgs
         * @return 接口返回
         */
        private RestRequestAsyncHandle createTenAccess<T>(String action, String apiAddr, SortedDictionary<string, string> urlArgs, Action<IRestResponse<T>> callBackActoin) where T : new()
        {
            if (null == urlArgs)
            {
                urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            }
            var nonce = RandomUtil.makeSring(false, 6);
            PutUrlArg(urlArgs, "Action", action);
            PutUrlArg(urlArgs, "Timestamp", TimeUtil.CurrentMillis() / 1000 + "");
            PutUrlArg(urlArgs, "Nonce", nonce);
            PutUrlArg(urlArgs, "SecretId", mTenDnsApiSecret.SecretId);

            var signature = createSignature("POST", apiAddr, urlArgs);
            PutUrlArg(urlArgs, "Signature", signature);
            return RequestUtil.AsyncRequest<T>(PROTOCOL + CNS_DOMAIN + apiAddr, urlArgs, callBackActoin, Method.POST);
            //restTemplate.getMessageConverters().add(new FastJsonHttpMessageConverter());
            //return restTemplate.postForObject(PROTOCOL + CNS_DOMAIN + apiAddr, convertMap(urlArgs), JSONObject.class);
        }

        /// <summary>
        ///  修改DNS解析记录
        /// </summary>
        /// <param name="dnsDomain"></param>
        /// <param name="dnsRecord"></param>
        /// <param name="callBackActoin"></param>
        /// <returns></returns>
        public RestRequestAsyncHandle ModifyDNS(DnsRecord dnsRecord, Action<IRestResponse<DnsOperatorResult>> callBackActoin)
        {
            SortedDictionary<string, string> urlArgs = new SortedDictionary<string, string>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "recordId", Convert.ToString(dnsRecord.id));
            PutUrlArg(urlArgs, "subDomain", dnsRecord.Name);
            PutUrlArg(urlArgs, "recordType", dnsRecord.type);
            PutUrlArg(urlArgs, "recordLine", dnsRecord.line);
            PutUrlArg(urlArgs, "value", dnsRecord.Value);
            //        putUrlArg(urlArgs,"ttl", "10");
            //        putUrlArg(urlArgs,"mx", "1");
            return createTenAccess("RecordModify", CVM_REQUEST_ADDR, urlArgs, callBackActoin);
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
        public RestRequestAsyncHandle queryRecordList<T>(String subDomain, String recordType, Action<IRestResponse<T>> callBackActoin) where T : new()
        {
            var urlArgs = new SortedDictionary<String, String>(StringComparer.Ordinal);
            PutUrlArg(urlArgs, "domain", mTenDnsApiSecret.PrivDomain);
            PutUrlArg(urlArgs, "subDomain", subDomain);
            PutUrlArg(urlArgs, "recordType", recordType);
            return createTenAccess("RecordList", CVM_REQUEST_ADDR, urlArgs, callBackActoin);
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

        /**
         * 将k-v参数转换成rest template能够使用的类型
         * @param treeMap
         * @return
         */
        public static List<Parameter> convertMap(SortedDictionary<string, string> treeMap)
        {
            List<Parameter> pmsx = new List<Parameter>();
            foreach (KeyValuePair<string, string> kvp in treeMap)
            {
                var item = new Parameter(kvp.Key.Trim(), kvp.Value.Trim(), ParameterType.QueryStringWithoutEncode);
                pmsx.Add(item);
            }
            return pmsx;
        }

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
