using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.imgrespotroy
{
    /// <summary>
    /// Prnt图床
    /// https://www.oleou.com/zt/tuc/
    /// </summary>
    public class PrntImageRepo
    {
        public string GetApiCode()
        {
            return ApplicationConstant.IMG_REPO_API[2];
        }

        public InvokeResult<ImageRepoUploadResult> Upload(ImageRepoUploadArg arg)
        {
            InvokeResult<ImageRepoUploadResult> invokeResult = new InvokeResult<ImageRepoUploadResult>()
            {
                success = false,
                message = "请求无返回"
            };
            ImageRepoUploadResult apiResult = new ImageRepoUploadResult()
            {
                Api = GetApiCode(),
                ApiChannel = GetApiCode()
            };
            //var response = string.Empty;
            //using (HttpClient client = new HttpClient())
            //{
            //    var content = new MultipartFormDataContent();
            //    content.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(arg.FullFilePath)), "image", arg.ExtraArgs["uploadFileFormName"].ToString());
            //    content.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.13; rv:65.0) Gecko/20100101 Firefox/65.0");
            //    response = client.PostAsync("https://prntscr.com/upload.php", content).Result.Content.ReadAsStringAsync().Result;
            //}

            //执行上传
            var response = RequestUtil.UploadFile("https://prntscr.com/upload.php", arg.FullFilePath, "image", arg.ExtraArgs["uploadFileFormName"].ToString(), null);
            if (string.IsNullOrEmpty(response))
            {
                return invokeResult;
            }
            //转换为json对象
            var resultModel = (JObject) JsonConvert.DeserializeObject(response);
            var status = Convert.ToString(resultModel["status"]);
            if (!status.Equals("success"))
            {
                invokeResult.message = Convert.ToString(resultModel["msg"]);
                return invokeResult;
            }
            var ImgAddress = resultModel["data"].ToString();
            var crawlResult = RequestUtil.CrawlContentFromWebsit(ImgAddress, null);

            //设置匹配规则
            Match mstr = Regex.Match(crawlResult.data, "(?m)<meta property=\"og: image\" content=\"(.*?)\"/>");
            if(!mstr.Success)
            {
                invokeResult.message = "数据抓取失败";
                return invokeResult;
            }
            //开始逐行爬取IP
            while (mstr.Success)
            {
                apiResult.ImgUrl = mstr.Groups[0].Value + mstr.Groups[1].Value;
                break;
            }

            invokeResult.success = true;
            invokeResult.data = apiResult;
            return invokeResult;
        }
    }
}
