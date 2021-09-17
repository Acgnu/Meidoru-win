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
    /// 遇见图床
    /// https://www.hualigs.cn/
    /// </summary>
    public class MeetImageRepo : IImageRepo
    {
        //遇见图床 的有效API列表
        private static readonly string[] CHANNELS = {
        "bilibili",       //bilibili
        //"toutiao",  //头条
        "sougou",  //搜狗
        //"huluxia", //葫芦
        //"sina", //新浪
        //"xiaomi", //小米
        "catbox", //猫盒
        //"qihoo", //奇虎
        //"suning", //苏柠
        //"netease", //往亿
        "niupic", //牛图
        "baidu", //百度
        //"qdoc", //腾毒
        //"bcebos", //瓣盒
        //"bjbcebos", //八戒
        //"ouliu",//蒲柳
        //"postimages", //贴图
        //"chaoxing",//超星
        "ai58",//五八
        "imgbox",//图盒
        //"imageshack",//图盒
        "imgur",//图绘
        "gtimg",//极图
        //"vxichina",//微聊
        //"renren",//人人
        //"taihe",//太河
        //"bkimg",//佰书
        "muke",//慕课
        //"bitauto",//易车
        //"vimcn",//微梦
        //"ali", //艾力
        //"smms" //末世
        };

        public string GetApiCode()
        {
            return ApplicationConstant.IMG_REPO_API[0];
        }

        public InvokeResult<ImageRepoUploadResult> Upload(ImageRepoUploadArg arg)
        {
            //image File    是 表单名称
            //apiType String 是   公有云CDN，上传的图床类型（this、ali、huluxia等等），支持字符串传入（英文逗号分隔，例如：ali,huluxia）
            //token String  是 用户的秘钥，不填则上传至游客组（前提：支持游客上传）
            Dictionary<string, object> args = new Dictionary<string, object>();
            var apiType = CHANNELS[RandomUtil.GetRangeRandomNum(0, CHANNELS.Length)];
            args.Add("apiType", apiType);
            args.Add("token", "8cf8e506934880417492bde929777da3");
            ImageRepoUploadResult apiResult = new ImageRepoUploadResult()
            {
                Api = GetApiCode(),
                ApiChannel = apiType
            };
            //执行上传
            var response = RequestUtil.UploadFile("https://www.hualigs.cn/api/upload", arg.FullFilePath, "image", arg.ExtraArgs["uploadFileFormName"].ToString(), args);
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
            apiResult.ImgUrl = resultModel["data"]["url"][apiType].ToString();
            return new InvokeResult<ImageRepoUploadResult>()
            {
                success = true,
                data = apiResult
            };
        }
    }
}
