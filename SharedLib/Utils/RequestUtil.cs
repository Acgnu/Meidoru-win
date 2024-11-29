using SharedLib.Model;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace SharedLib.Utils
{
    public class RequestUtil
    {
        public static readonly string CONNECTION_ERROR = "[CONNECTION_ERROR] 连接出错";
        private int ByteSize = 1024;
        /// <summary>
        /// 下载中的后缀，下载完成去掉
        /// </summary>
        private const string Suffix = ".downloading";

        //public event Action<int> ShowDownloadPercent;

        /**
         * HTTP协议POST请求添加参数的封装方法
         */
        public static string ConcatQueryString(SortedDictionary<string, string> paramsMap)
        {
            if (DataUtil.IsEmptyCollection(paramsMap))
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
        /// 异步请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="prams"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static async Task<string> TaskFormRequestAsync(string url, SortedDictionary<string, string> prams, HttpMethod method)
        {
            try
            {
                var client = new HttpClient();
                HttpResponseMessage response;
                if (method == HttpMethod.Post)
                {
                    var body = new FormUrlEncodedContent(prams);
                    response = await client.PostAsync(url, body);
                }
                else if (method == HttpMethod.Get)
                {
                    //TODO: 拼接参数
                    response = await client.GetAsync(url);
                }
                else
                {
                    throw new InvalidOperationException("不支持的请求方式");
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine(string.Format("{0} post fail with status code {1}", url, response.StatusCode));
                    return string.Empty;
                }
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return String.Empty;
        }

        public static async Task<HttpResponseMessage> TaskJsonRequestAsync<T>(string url, SortedDictionary<string, string> prams, HttpMethod method) where T : new()
        {
            var client = new HttpClient();
            if (method == HttpMethod.Post)
            {
                return await HttpClientJsonExtensions.PostAsJsonAsync(client, url, prams);
            }
            if (method == HttpMethod.Get)
            {
                //
            }
            throw new InvalidOperationException("不支持的请求方式");
        }

        /// <summary>
        /// Http方式下载文件
        /// </summary>
        /// <param name="url">http地址</param>
        /// <param name="localfile">本地文件</param>
        /// <returns>错误码</returns>
        public static int DownloadFile(string url, string localfile)
        {
            int ret = 0;
            string localfileReal = localfile;
            string localfileWithSuffix = localfileReal + Suffix;

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(localfileReal))
                return 1;

            if (File.Exists(localfileReal))
                return 0;

            try
            {
                long startPosition = 0;
                //取得远程文件长度
                using HttpClient client = new();
                var headResponse = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).Result;
                headResponse.EnsureSuccessStatusCode();
                long remoteFileLength = headResponse.Content.Headers.ContentLength ?? 0;

                if (remoteFileLength == 0)
                    return 2;

                //判断要下载的文件是否存在
                if (File.Exists(localfileWithSuffix))
                {
                    startPosition = new FileInfo(localfileWithSuffix).Length;
                    if (startPosition > remoteFileLength)
                    {
                        FileUtil.DeleteFile(localfileWithSuffix);
                        startPosition = 0;
                    }
                    else if (startPosition == remoteFileLength)
                    {
                        return 0;
                    }
                }

                client.DefaultRequestHeaders.Range = new RangeHeaderValue(startPosition, null);
                using var readStream = client.GetStreamAsync(url).Result;

                //long currPostion = startPosition;
                //int contentSize = 0;

                using var fileStream = new FileStream(localfileWithSuffix, FileMode.OpenOrCreate, FileAccess.Write);
                fileStream.Seek(startPosition, SeekOrigin.Begin);

                readStream.CopyTo(fileStream);

                //currPostion += contentSize;

                //ShowDownloadPercent?.Invoke((int)(currPostion * 100 / remoteFileLength));
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取远程文件失败！exception：\n" + ex.ToString());
                ret = 4;
            }
            finally
            {
                if (ret == 0)
                {
                    try
                    {
                        //去掉.downloading后缀
                        FileInfo fi = new(localfileWithSuffix);
                        fi.MoveTo(localfileReal);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        //通知完成
                        //ShowDownloadPercent?.Invoke(100);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 从指定URL的网页中爬取内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="proxyAddress">网页代理 可以为null</param>
        /// <returns></returns>
        public static InvokeResult<string> CrawlContentFromWebsit(string url, string? proxyAddress, int timeout)
        {
            try
            {
                SocketsHttpHandler httpHandler;
                if (!string.IsNullOrEmpty(proxyAddress))
                {
                    httpHandler = new()
                    {
                        Proxy = new WebProxy(proxyAddress)
                    };
                }
                else
                {
                    httpHandler = new SocketsHttpHandler();
                }
                using var Req = new HttpClient(httpHandler);
                Req.Timeout = TimeSpan.FromMilliseconds(timeout);
                Req.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3"); 
                Req.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
                //Req.CookieContainer = new CookieContainer();
                //Req.CookieContainer.Add(new Cookie("https_ydclearance", "d35f9958b5272f5073e030f7-28c7-453f-bae7-16b9294a006e-1726306381"));
                var respon = Req.GetAsync(url).Result;
                respon.EnsureSuccessStatusCode();
                return new InvokeResult<string>()
                {
                    success = true,
                    data = respon.Content.ReadAsStringAsync().Result
                };
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
        public static InvokeResult<string> CrawlContentFromWebsit(string url, string? proxyAddress)
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
                if (null != args)
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
