using SharedLib.Model;
using SharedLib.Utils;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SharedLib.ImageNetRepository
{
    /// <summary>
    /// Prnt图床
    /// https://www.oleou.com/zt/tuc/
    /// </summary>
    public class PrntImageRepo
    {
        public string GetApiCode()
        {
            return "prnt";
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
            var response = RequestUtil.UploadFile(
                "https://prntscr.com/upload.php",
                arg.FullFilePath,
                "image",
                arg.ExtraArgs["uploadFileFormName"],
                 null);
            if (string.IsNullOrEmpty(response))
            {
                return invokeResult;
            }
            //转换为json对象
            var jsonDoc = JsonSerializer.Deserialize<JsonDocument>(response);
            var status = jsonDoc.RootElement.GetProperty("status").GetString();
            if (!status.Equals("success"))
            {
                invokeResult.message = jsonDoc.RootElement.GetProperty("msg").GetString();
                return invokeResult;
            }
            var ImgAddress = jsonDoc.RootElement.GetProperty("data").GetString();
            var crawlResult = RequestUtil.CrawlContentFromWebsit(ImgAddress, null);

            //设置匹配规则
            Match mstr = Regex.Match(crawlResult.data, "(?m)<meta property=\"og: image\" content=\"(.*?)\"/>");
            if (!mstr.Success)
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
