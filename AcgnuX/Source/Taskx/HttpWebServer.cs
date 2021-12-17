using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Resources;

namespace AcgnuX.Source.Taskx.Http
{
    /// <summary>
    /// 简易HTTP服务
    /// </summary>
    public class HttpWebServer
    {
        //监听的地址
        private readonly string LISTENING_ADDRESS = @"http://127.0.0.1:7777/";

        //flash发送消息时的主动推送触发事件
        public event EditConfirmHandler<PianoScore> editConfirmHnadler;

        /// <summary>
        /// 启动监听
        /// </summary>
        public void StartListen()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListener.Prefixes.Add(LISTENING_ADDRESS);
            httpListener.Start();
            new Thread(new ThreadStart(delegate
            {
                while (true)
                {
                    try
                    {
                        HttpListenerContext httpListenerContext = httpListener.GetContext();
                        httpListenerContext.Response.StatusCode = 200;
                        using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                        {
                            DispacherRequest(httpListenerContext);
                            //httpListenerContext.Response.ContentType = "application/octet-stream";
                            //string fileName = @"D:\\Download\Tan8\a小调圆舞曲\page.0.png";
                            ////httpListenerContext.Response.AddHeader("Content-Disposition", "attachment;FileName=" + fileName);

                            //using (System.IO.FileStream fs = new FileStream(@"D:\Download\Tan8\a小调圆舞曲\page.0.png", FileMode.Open, FileAccess.Read))
                            //{
                            //    byte[] data = new byte[fs.Length];
                            //    httpListenerContext.Response.ContentLength64 = data.Length;
                            //    System.IO.Stream output = httpListenerContext.Response.OutputStream;
                            //    output.Write(data, 0, data.Length);
                            //    output.Close();
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            })).Start();
        }

        /// <summary>
        /// 分发请求
        /// </summary>
        /// <param name="httpListenerContext"></param>
        private void DispacherRequest(HttpListenerContext httpListenerContext)
        {
            //下载封面
            if (httpListenerContext.Request.Url.LocalPath.Equals("/yuepu/preview"))
            {
                ResponsePreview(httpListenerContext);
            }
            //下载sounds.swf
            if (httpListenerContext.Request.Url.LocalPath.Equals("/yuepu/flash/sound"))
            {
                ResponseSoundFlash(httpListenerContext);
            }
            //获取乐谱信息
            if (httpListenerContext.Request.Url.LocalPath.Equals("/yuepu/info"))
            {
                ResponseYuepuInfo(httpListenerContext);
            }
            //下载乐谱页
            if (httpListenerContext.Request.Url.LocalPath.Equals("/yuepu/page"))
            {
                ResponseYuepuPage(httpListenerContext);
            }
            //下载播放文件
            if (httpListenerContext.Request.Url.LocalPath.Equals("/yuepu/playfile"))
            {
                ResponsePlayFile(httpListenerContext);
            }
            //监听曲谱地址
            if (httpListenerContext.Request.Url.LocalPath.Equals("/yuepu/fetch"))
            {
                FetchPianoScore(httpListenerContext);
            }
            //v2版曲谱地址
            if(httpListenerContext.Request.Url.LocalPath.Equals("/yuepu/info/v2"))
            {
                ResponseV2Yuepu(httpListenerContext);
            }
        }

        /// <summary>
        /// 响应乐谱页
        /// </summary>
        /// <param name="httpListenerContext"></param>
        private void ResponseYuepuPage(HttpListenerContext httpListenerContext)
        {
            //ypid=666.1.png
            var ypidQuery = httpListenerContext.Request.QueryString["ypid"];
            var ypidSplit = ypidQuery.Split('.');
            //返回指定页
            var previewImgPath = Path.Combine(ConfigUtil.Instance.PianoScorePath, ypidSplit[0], "page." + ypidSplit[1] + ".png");
            WriteFile(previewImgPath, httpListenerContext);
        }

        /// <summary>
        /// 响应播放文件
        /// </summary>
        /// <param name="httpListenerContext"></param>
        private void ResponsePlayFile(HttpListenerContext httpListenerContext)
        {
            //ypid=666
            var ypid = httpListenerContext.Request.QueryString["ypid"];
            var v = httpListenerContext.Request.QueryString["v"];
            if (!string.IsNullOrEmpty(ypid))
            {
                var playFileSuf = ".ypa2";
                if("2".Equals(v))
                {
                    playFileSuf = ".ypdx";
                }
                //返回指定页
                var previewImgPath = Path.Combine(ConfigUtil.Instance.PianoScorePath, ypid, "play" + playFileSuf);
                WriteFile(previewImgPath, httpListenerContext);
            }
        }

        /// <summary>
        /// 监听flash主动发送的曲谱信息地址
        /// </summary>
        /// <param name="httpListenerContext"></param>
        private void FetchPianoScore(HttpListenerContext httpListenerContext)
        {
            //?ypid=29189&sccode=0373ef7aa7c3e092b8c4e09748574186&r1=8538&r2=5971&input=123
            editConfirmHnadler?.Invoke(new PianoScore()
            {
                id = Convert.ToInt32(httpListenerContext.Request.QueryString["ypid"]),
                SheetUrl = string.Format("http://www.77music.com/flash_get_yp_info.php?ypid={0}&sccode={1}&r1={2}&r2={3}&input=123",
                httpListenerContext.Request.QueryString["ypid"],
                httpListenerContext.Request.QueryString["sccode"],
                httpListenerContext.Request.QueryString["r1"],
                httpListenerContext.Request.QueryString["r2"]),
                Ver = 1
            });
        }

        /// <summary>
        /// 响应乐谱串
        /// </summary>
        /// <param name="httpListenerContext"></param>
        private void ResponseYuepuInfo(HttpListenerContext httpListenerContext)
        {
            var ypid = httpListenerContext.Request.QueryString["ypid"];
            //找到存储的曲谱源数据
            var rawData = SQLite.sqlone("SELECT origin_data FROM tan8_music WHERE ypid = @ypid", new SQLiteParameter[] { new SQLiteParameter("@ypid", ypid) });

            //从乐谱信息解析到对象
            var tan8Music = DataUtil.ParseToModel(rawData);

            //替换乐谱页和播放文件下载地址
            var pageUrl = LISTENING_ADDRESS + "yuepu/page?ypid=" + ypid;
            var playFileUrl = LISTENING_ADDRESS + "yuepu/playfile?v=1&ypid=" + ypid;

            //响应体
            StringBuilder resHtm = new StringBuilder();
            resHtm.Append("<html><body>yp_create_time=<yp_create_time>" + tan8Music.yp_create_time + "</yp_create_time><br/>");
            resHtm.Append("yp_title=<yp_title>" + tan8Music.yp_title + "</yp_title><br/>");
            resHtm.Append("yp_page_count=<yp_page_count>" + tan8Music.yp_page_count + "</yp_page_count><br/>");
            resHtm.Append("yp_page_width=<yp_page_width>" + tan8Music.yp_page_width + "</yp_page_width><br/>");
            resHtm.Append("yp_page_height=<yp_page_height>" + tan8Music.yp_page_height + "</yp_page_height><br/>");
            resHtm.Append("yp_is_dadiao=<yp_is_dadiao>" + tan8Music.yp_is_dadiao + "</yp_is_dadiao><br/>");
            resHtm.Append("yp_key_note=<yp_key_note>" + tan8Music.yp_key_note + "</yp_key_note><br/>");
            resHtm.Append("yp_is_yanyin=<yp_is_yanyin>" + tan8Music.yp_is_yanyin + "</yp_is_yanyin><br/>");
            resHtm.Append("ypad_url=<ypad_url>" + pageUrl + "</ypad_url>");
            resHtm.Append("ypad_url2=<ypad_url2>" + playFileUrl + "</ypad_url2></body></html>");

            //刷写结果
            using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
            {
                writer.Write(resHtm);
            }
        }

            /// <summary>
            /// 响应sound.swf
            /// </summary>
            /// <param name="httpListenerContext"></param>
        private void ResponseSoundFlash(HttpListenerContext httpListenerContext)
        {
            WriteFile(Environment.CurrentDirectory + @"\Assets\flash\sounds.swf" , httpListenerContext);
        }

        /// <summary>
        /// 获取乐谱
        /// </summary>
        /// <param name="httpListenerContext"></param>
        //public static const flash_prev_yp_info_URL:String="https://www.tan8.com/flash_prev_yp_info.php?ypid=;
        //http://oss.tan8.com/yuepuku/56/28064/prev_28064.0.png
        private void ResponsePreview(HttpListenerContext httpListenerContext)
        {
            var ypid = httpListenerContext.Request.QueryString["ypid"];
            //根据乐谱ID得到数据中乐谱名称
            if (!string.IsNullOrEmpty(ypid))
            {
                //根据名称返回文件夹中的乐谱第一页
                var previewImgPath = Path.Combine(ConfigUtil.Instance.PianoScorePath, ypid, "page.0.png");
                WriteFile(previewImgPath, httpListenerContext);
            }
            else
            {
                //没有则返回默认图 (避免flash播放器报错, 无法用程序退出)
                WriteStream(FileUtil.GetApplicationResourceAsStream(@"/Assets/Images/tan8_sheet_preview_default.png"), httpListenerContext);
            }
        }

        /// <summary>
        /// 响应V2版乐谱资源
        /// </summary>
        /// <param name="httpListenerContext"></param>
        private void ResponseV2Yuepu(HttpListenerContext httpListenerContext)
        {
            //http://www.tan8.com/codeindex.php?d=api&c=check77playerPower&m=index&ypid=76202&uid=999999999&token=fbf0ab24ee47bfa1cb460e41c1f61fdb
            var ypid = httpListenerContext.Request.QueryString["ypid"];
            var method = httpListenerContext.Request.QueryString["m"];
            string responseText = "";
            if (method.Equals("index"))
            {
                responseText = "{\"responseCode\":\"1000\",\"message\":\"\\u6b63\\u5e38\",\"power\":{\"openPower\":\"1\",\"printPower\":\"1\",\"printCount\":\"30\",\"vstPower\":\"1\",\"pdfPower\":\"1\"}}";
            }
            //http://www.tan8.com/codeindex.php?d=api&c=check77playerPower&m=getYpdsUrl&ypid=76202&uid=999999999&token=d58c1bf196fc7e95c7fe05bd693a2af0
            if (method.Equals("getYpdsUrl"))
            {
                var responsBuilder = new StringBuilder("{\"data\":{\"code\":\"1000\",\"message\":\"\\u83b7\\u53d6\\u6210\\u529f\",\"result\":{\"ypdsUrl\":\"")
                    //.Append("http:\\/\\/oss.tan8.com\\/yuepuku\\/115\\/57806\\/57806_hhdafigb.ypds")
                    .Append("http:\\/\\/127.0.0.1:7777\\/yuepu\\/playfile?v=2&ypid=" + ypid)
                    .Append("\",\"ypdxUrl\":\"")
                    //.Append("http:\\/\\/oss.tan8.com\\/yuepuku\\/115\\/57806\\/57806_hhdafigb.ypdx")
                    .Append("http:\\/\\/127.0.0.1:7777\\/yuepu\\/playfile?v=2&ypid=" + ypid)
                    .Append("\"}}}");
                responseText = responsBuilder.ToString();
            }
            //http://www.tan8.com/codeindex.php?d=api&c=check77playerPower&m=addDynamic&ypid=74205&uid=999999999&token=092e0f2b7e27b1aaf003b2f5d42d25e1&dynamicType=preservePianoPdf
            if (method.Equals("addDynamic"))
            {
                responseText = "{\"data\":{\"code\":\"1000\",\"message\":\"\\u6b63\\u5e38\",\"result\":{\"printCount\":\"30\"}}}";
            }
            //刷写结果
            using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
            {
                writer.Write(responseText);
            }
        }

        /// <summary>
        /// 响应文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="httpListenerContext"></param>
        private void WriteFile(string filePath, HttpListenerContext httpListenerContext)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                //add for v2
                httpListenerContext.Response.ContentType = "application/octet-stream";
                httpListenerContext.Response.ContentLength64 = fs.Length;
                ///---end
                Stream output = httpListenerContext.Response.OutputStream;
                byte[] picbyte = new byte[fs.Length];
                using (BinaryReader br = new BinaryReader(fs))
                {
                    picbyte = br.ReadBytes(Convert.ToInt32(fs.Length));
                    output.Write(picbyte, 0, picbyte.Length);
                    output.Close();
                }
            }
        }

        /// <summary>
        /// 以流的方式响应文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="httpListenerContext"></param>
        private void WriteStream(StreamResourceInfo streamResourceInfo, HttpListenerContext httpListenerContext)
        {
            try
            {
                Stream output = httpListenerContext.Response.OutputStream;
                byte[] picbyte;
                using (BinaryReader br = new BinaryReader(streamResourceInfo.Stream))
                {
                    picbyte = br.ReadBytes(Convert.ToInt32(streamResourceInfo.Stream.Length));
                    output.Write(picbyte, 0, picbyte.Length);
                    output.Close();
                }
            } 
            finally
            {
                streamResourceInfo.Stream.Close();
            }
        }

        /// <summary>
        /// 根据乐谱ID从数据库查询名称
        /// </summary>
        /// <param name="ypid"></param>
        /// <returns></returns>
        //private string GetDbFileName(string ypid)
        //{
        //    return SQLite.sqlone("SELECT name FROM tan8_music WHERE ypid = @ypid", new SQLiteParameter[] { new SQLiteParameter("@ypid", ypid) });
        //}
    }
}
