using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.imgrespotroy
{
    /// <summary>
    /// 多合一图床
    /// https://www.oleou.com/zt/tuc/
    /// </summary>
    public class OleouImageRepo : IImageRepo
    {
        private static readonly string[] CHANNELS = {
            "tt",       //头条
            "sg",  //搜狗
            "jd",  //京东
            //"c58", //58  58刚上传完没多久就被删了, 不用这个
            "wy", //网易
            "sh", //搜狐
            "hl", //葫芦侠
        };

        public string GetApiCode()
        {
            return ApplicationConstant.IMG_REPO_API[1];
        }

        public InvokeResult<ImageRepoUploadResult> Upload(ImageRepoUploadArg arg)
        {
            var apiType = CHANNELS[RandomUtil.GetRangeRandomNum(0, CHANNELS.Length)];
            ImageRepoUploadResult apiResult = new ImageRepoUploadResult()
            {
                Api = GetApiCode(),
                ApiChannel = apiType
            };
            //执行上传
            //var uploadFileFormName = "sheet_" + RandomUtil.makeSring(false, 8) + ".png";
            var response = RequestUtil.UploadFile("https://image.kieng.cn/upload.html?type=" + apiType, arg.FullFilePath, "image", arg.ExtraArgs["uploadFileFormName"].ToString(), null);
            if (string.IsNullOrEmpty(response))
            {
                return new InvokeResult<ImageRepoUploadResult>()
                {
                    success = false,
                    message = "请求无返回"
                };
            }
            //转换为json对象
            JObject resultModel = (JObject) JsonConvert.DeserializeObject(response);
            int code = Convert.ToInt32(resultModel["code"]);
            if (code != 200)
            {
                return new InvokeResult<ImageRepoUploadResult>()
                {
                    success = false,
                    message = "[" + apiType + "]" + Convert.ToString(resultModel["msg"])
                };
            }
            apiResult.ImgUrl = resultModel["data"]["url"].ToString();
            return new InvokeResult<ImageRepoUploadResult>()
            {
                success = true,
                data = apiResult
            };
        }
    }
}
