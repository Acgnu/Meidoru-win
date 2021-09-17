using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using RestSharp;
using RestSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace AcgnuX.Utils
{
    public class RequestUtil
    {
        public static readonly string CONNECTION_ERROR = "[CONNECTION_ERROR] 连接出错";
        private int ByteSize = 1024;
        /// <summary>
        /// 下载中的后缀，下载完成去掉
        /// </summary>
        private const string Suffix = ".downloading";

        public event Action<int> ShowDownloadPercent;

        /**
         * HTTP协议POST请求添加参数的封装方法
         */
        public static String ConcatQueryString(SortedDictionary<String, String> paramsMap)
        {
            if (null == paramsMap || paramsMap.Count == 0)
            {
                return "";
            }
            var param = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in paramsMap)
            {
                param.Append("&").Append(kvp.Key.Trim()).Append("=").Append(kvp.Value.Trim());
            }
            return param.Remove(0, 1).ToString();
        }

        /// <summary>
        /// 发起数据异步请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">远程地址</param>
        /// <param name="prams">参数</param>
        /// <param name="callBackActoin">回调函数</param>
        /// <param name="type">请求方法</param>
        /// <returns>请求句柄</returns>
        public static RestRequestAsyncHandle AsyncRequest<T>(string url, SortedDictionary<string, string> prams, Action<IRestResponse<T>> callBackActoin, Method type) where T : new()
        {
            var request = new RestRequest(type);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.OnBeforeDeserialization = response => response.ContentType = ContentType.Json;
            foreach (KeyValuePair<string, string> kvp in prams)
            {
                request.AddParameter(kvp.Key, kvp.Value);
            }
            var client = new RestClient(url);
            var asyncHandle = client.ExecuteAsync<T>(request, callBackActoin);
            return asyncHandle;
        }

        /// <summary>
        /// 发起同步请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="prams"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IRestResponse<T> SyncRequest<T>(string url, SortedDictionary<string, string> prams, Method type) where T : new()
        {
            var request = new RestRequest(type);

            foreach (KeyValuePair<string, string> kvp in prams)
            {
                request.AddParameter(kvp.Key, kvp.Value);
            }

            var client = new RestClient(url);

            var response = client.Execute<T>(request);

            return response;
        }

        /// <summary>
        /// Http方式下载文件
        /// </summary>
        /// <param name="url">http地址</param>
        /// <param name="localfile">本地文件</param>
        /// <returns>错误码</returns>
        public int DownloadFile(string url, string localfile)
        {
            int ret = 0;
            string localfileReal = localfile;
            string localfileWithSuffix = localfileReal + Suffix;

            try
            {
                long startPosition = 0;
                FileStream writeStream = null;

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(localfileReal))
                    return 1;

                //取得远程文件长度
                long remoteFileLength = GetHttpLength(url);
                if (remoteFileLength == 0)
                    return 2;

                if (File.Exists(localfileReal))
                    return 0;

                //判断要下载的文件是否存在
                if (File.Exists(localfileWithSuffix))
                {
                    writeStream = File.OpenWrite(localfileWithSuffix);
                    startPosition = writeStream.Length;
                    if (startPosition > remoteFileLength)
                    {
                        writeStream.Close();
                        File.Delete(localfileWithSuffix);
                        writeStream = new FileStream(localfileWithSuffix, FileMode.Create);
                    }
                    else if (startPosition == remoteFileLength)
                    {
                        DownloadFileOk(localfileReal, localfileWithSuffix);
                        writeStream.Close();
                        return 0;
                    }
                    else
                        writeStream.Seek(startPosition, SeekOrigin.Begin);
                }
                else
                    writeStream = new FileStream(localfileWithSuffix, FileMode.Create);

                HttpWebRequest req = null;
                HttpWebResponse rsp = null;
                try
                {
                    req = (HttpWebRequest)HttpWebRequest.Create(url);
                    if (startPosition > 0)
                        req.AddRange((int)startPosition);

                    rsp = (HttpWebResponse)req.GetResponse();
                    using (Stream readStream = rsp.GetResponseStream())
                    {
                        byte[] btArray = new byte[ByteSize];
                        long currPostion = startPosition;
                        int contentSize = 0;
                        while ((contentSize = readStream.Read(btArray, 0, btArray.Length)) > 0)
                        {
                            writeStream.Write(btArray, 0, contentSize);
                            currPostion += contentSize;

                            ShowDownloadPercent?.Invoke((int)(currPostion * 100 / remoteFileLength));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取远程文件失败！exception：\n" + ex.ToString());
                    ret = 3;
                }
                finally
                {
                    if (writeStream != null)
                        writeStream.Close();
                    if (rsp != null)
                        rsp.Close();
                    if (req != null)
                        req.Abort();

                    if (ret == 0)
                        DownloadFileOk(localfileReal, localfileWithSuffix);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取远程文件失败！exception：\n" + ex.ToString());
                ret = 4;
            }
            return ret;
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        private void DownloadFileOk(string localfileReal, string localfileWithSuffix)
        {
            try
            {
                //去掉.downloading后缀
                FileInfo fi = new FileInfo(localfileWithSuffix);
                fi.MoveTo(localfileReal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                //通知完成
                ShowDownloadPercent?.Invoke(100);
            }
        }

        // 从文件头得到远程文件的长度
        private long GetHttpLength(string url)
        {
            long length = 0;
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            try
            {
                req = (HttpWebRequest)HttpWebRequest.Create(url);
                rsp = (HttpWebResponse)req.GetResponse();
                if (rsp.StatusCode == HttpStatusCode.OK)
                    length = rsp.ContentLength;
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取远程文件大小失败！exception：\n" + ex.ToString());
            }
            finally
            {
                if (rsp != null)
                    rsp.Close();
                if (req != null)
                    req.Abort();
            }
            return length;
        }

        /// <summary>
        /// 从指定URL的网页中爬取内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="proxyAddress">网页代理 可以为null</param>
        /// <returns></returns>
        public static InvokeResult<string> CrawlContentFromWebsit(string url, string proxyAddress, int timeout)
        {
            try
            {
                HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(url);
                if (!string.IsNullOrEmpty(proxyAddress))
                {
                    WebProxy proxy = new WebProxy(proxyAddress);
                    Req.Proxy = proxy;
                }
                Req.Timeout = timeout;
                Req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
                Req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36";
                Req.Method = "GET";
                HttpWebResponse Resp = (HttpWebResponse)Req.GetResponse();
                Encoding code = Encoding.GetEncoding("UTF-8");
                using (StreamReader sr = new StreamReader(Resp.GetResponseStream(), code))
                {
                    var content = sr.ReadToEnd();
                    return new InvokeResult<string>()
                    {
                        success = true,
                        data = content
                    };
                }
            }
            catch (Exception e)
            {
                //Message = "无法连接到远程服务器"
                Console.WriteLine(e.Message + " : " + url);
            }
            return new InvokeResult<string>()
            {
                success = false,
                data = CONNECTION_ERROR
            };
        }


        /// <summary>
        /// 从指定URL的网页中爬取内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="proxyAddress">网页代理 可以为null</param>
        /// <returns></returns>
        public static InvokeResult<string> CrawlContentFromWebsit(string url, string proxyAddress)
        {
            return CrawlContentFromWebsit(url, proxyAddress, 5000);
        }

        /// <summary>
        /// HTTP上传文件
        /// </summary>
        /// <param name="url">上传地址</param>
        /// <param name="fullFilePath">文件完整路径</param>
        /// <param name="formFileName">form表单的文件key</param>
        /// <param name="uploadFileName">form表单的文件名称</param>
        /// <param name="args">其他参数</param>
        /// <returns></returns>
        public static string UploadFile(string url, string fullFilePath, string formFileName, string uploadFileName, Dictionary<string, object> args)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(3);
                var content = new MultipartFormDataContent();
                //添加字符串参数，参数名为
                if(null != args)
                {
                    foreach (KeyValuePair<string, object> kvp in args)
                    {
                        content.Add(new StringContent(kvp.Value.ToString()), kvp.Key);
                    }
                }
                //添加文件参数，参数名为formFileName，文件名
                content.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(fullFilePath)), formFileName, uploadFileName);

                var result = client.PostAsync(url, content).Result.Content.ReadAsStringAsync().Result;
                return result;
            }
        }
    }
}
