using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AcgnuX.Source.Bussiness.imgrespotroy;

namespace PatchTool
{
    class Program
    {

        [DllImport("User32.dll")]
        public static extern int MessageBox(int h, string m, string c, int type);


        /// <summary>
        /// 建议的执行顺序
        /// 1.Clean0PageYuepu
        /// 2.ShowNameNotExistsFolder
        /// 3.ShowFolderNotExistsDB
        /// 4.RedownloadRepeat
        /// 5.RedownloadOldVer
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //TestConvertImg();
            //if(true)
            //{
            //    return;
            //}
            var command = "?";
            var autoDel = false;
            var autoCopy = false;
            var savePath = string.Empty;
            var oldHomePath = string.Empty;
            var dbPath = string.Empty;
            var threadCount = 5;
            var maxYpid = 0;
            var overwrite = false;
            if (args.Length > 0)
            {
                command = args[0];
                foreach (var arg in args)
                {
                    if (arg.StartsWith("-f"))
                    {
                        savePath = arg.Substring(2);
                    }
                    if (arg.StartsWith("-d"))
                    {
                        dbPath = arg.Substring(2);
                    }
                    if (arg.Equals("-r"))
                    {
                        autoDel = true;
                    }
                    if (arg.StartsWith("-i"))
                    {
                        maxYpid = Convert.ToInt32(arg.Substring(2));
                    }
                    if(arg.StartsWith("-h"))
                    {
                        oldHomePath = arg.Substring(2);
                    }
                    if (arg.Equals("-c"))
                    {
                        autoCopy = true;
                    }
                    if(arg.StartsWith("-t"))
                    {
                        threadCount = Convert.ToInt32(arg.Substring(2));
                    }
                    if(arg.Equals("-o"))
                    {
                        overwrite = true;
                    }
                }
            }
            switch (command)
            {
                case "?": ShowTips(); break;
                case "ckdir": ShowFolderNotExistsDB(dbPath, savePath, autoDel); break;
                case "cknon": Clean0PageYuepu(dbPath, autoDel); break;
                case "ckrpt": CheckRepeat(dbPath, savePath, autoDel); break;
                case "ckold": RedownloadOldVer(dbPath, maxYpid); break;
                case "ckdb": ShowNameNotExistsFolder(dbPath, savePath); break;
                case "ckodh": CheckOldHome(dbPath, oldHomePath, autoCopy); break;
                case "ckwb": CheckWhiteBlackPreview(dbPath, overwrite, threadCount); break;
                case "ckimg" : CheckSheetPreviewImg(dbPath, threadCount); break;
                case "ckdirnm": CheckDirName(dbPath); break;
            }
            //Console.ReadKey();
        }

        private static void ShowTips()
        {
            Console.WriteLine("太麻烦了懒得写, 去看源码");
        }

        /// <summary>
        /// 显示数据库中有但是文件夹里没有的
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="savePath"></param>
        private static void ShowNameNotExistsFolder(string dbPath, string savePath)
        {
            Console.WriteLine("检查仅存在数据库中的乐谱");
            InitDB(dbPath);
            var totalNe = 0;
            var cur = 0;
            var delSQL = new StringBuilder("DELETE FROM tan8_music WHERE ypid in (");
            //找出文件夹存在, 而数据库中不存在的
            var ypHomePath = ConfigUtil.Instance.Load().PianoScorePath;
            var dataSet = SQLite.SqlTable("SELECT ypid, name FROM tan8_music", null);
            var total = dataSet.Rows.Count;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                cur++;
                if (!Directory.Exists(Path.Combine(ypHomePath, dataRow["ypid"] as string)))
                {
                    totalNe++;
                    Console.WriteLine(dataRow["name"] + "   ----进度:" + cur + "/" + total + "");
                    delSQL.Append(dataRow["ypid"]).Append(",");
                }
            }
            delSQL.Length -= 1;
            delSQL.Append(")");
            Console.WriteLine("共" + totalNe + "个");
            if(totalNe > 0)
            {
                Console.WriteLine(delSQL.ToString());
                if (!string.IsNullOrEmpty(savePath))
                {
                    FileUtil.SaveStringToFile(delSQL.ToString(), Path.GetDirectoryName(savePath), Path.GetFileName(savePath));
                    Console.WriteLine("SQL文件已保存至:" + savePath);
                }
            }
        }

        /// <summary>
        /// ckdir -fD:\\丢雷老谋.txt -d
        /// </summary>
        private static void ShowFolderNotExistsDB(string dbPath, string savePath, bool autoDel)
        {
            Console.WriteLine("检查不存在于数据的文件夹");
            InitDB(dbPath);
            //找出文件夹存在, 而数据库中不存在的
            var ypHomePath = ConfigUtil.Instance.Load().PianoScorePath;
            var dir = Directory.GetDirectories(ypHomePath);
            var allFolderNames = new StringBuilder();
            var total = 0;
            foreach (var d in dir)
            {
                var ypid = Path.GetFileName(d);
                var num = SQLite.sqlone("select count(1) num from tan8_music where ypid = @ypid", new SQLiteParameter[] { new SQLiteParameter("@ypid", ypid) });
                var num32 = Convert.ToInt32(num);
                if(num32 == 0)
                {
                    total++;
                    allFolderNames.Append(ypid).Append("\r\n");
                    Console.WriteLine(ypid);
                    if (autoDel) FileUtil.DeleteDirWithName(ypHomePath, ypid);
                }
            }
            if(allFolderNames.Length > 0)
            {
                Console.WriteLine("\n共" + total + "个");
                if(!string.IsNullOrEmpty(savePath))
                {
                    FileUtil.SaveStringToFile(allFolderNames.ToString(), Path.GetDirectoryName(savePath), Path.GetFileName(savePath));
                    Console.WriteLine("文件已保存至" + savePath);
                }
            }
            else
            {
                Console.WriteLine("没有不匹配的文件夹");
                //MessageBox(0, "没有不匹配的文件夹", "哇塞", 0);
            }
        }

        /// <summary>
        /// 重新下载重名的
        /// </summary>
        private static void CheckRepeat(string dbPath, string savePath, bool autoDel)
        {
            Console.WriteLine("检查并重新下载重名的乐谱");
            InitDB(dbPath);
            var ypHomePath = ConfigUtil.Instance.Load().PianoScorePath;
            var dataSet = SQLite.SqlTable("SELECT count(1) num, name FROM tan8_music GROUP BY name HAVING num > 1 ORDER BY num DESC", null);
            var reDownBuilder = new StringBuilder("INSERT INTO tan8_music_down_task values");
            var delSQLBuilder = new StringBuilder("DELETE FROM tan8_music WHERE ypid in (");
            var total = 0;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                var repeatNameIds = SQLite.sqlcolumn("SELECT ypid FROM tan8_music WHERE name = @name", new List<SQLiteParameter>() { new SQLiteParameter("name", dataRow["name"]) });
                if (repeatNameIds.Count <= 1) continue;
                total++;
                repeatNameIds.ForEach(e => {
                    reDownBuilder.Append("(").Append(e).Append(")").Append(",");
                    delSQLBuilder.Append(e).Append(",");
                });
                Console.WriteLine(dataRow["name"] + " - " + dataRow["num"]);
                //指定了ID保存路径, 自动删除才有效
                if(autoDel && !string.IsNullOrEmpty(savePath))
                {
                    repeatNameIds.ForEach(e => {
                        FileUtil.DeleteDirWithName(ypHomePath, e);
                    });
                }
            }
            if(total == 0)
            {
                Console.WriteLine("没有重名的乐谱");
            }
            else
            {
                Console.WriteLine("共" + total + "个需要重新下载");
                reDownBuilder.Length -= 1;
                delSQLBuilder.Length -= 1;
                delSQLBuilder.Append(")");
                reDownBuilder.Append(";").Append(delSQLBuilder);
                if (!string.IsNullOrEmpty(savePath))
                {
                    FileUtil.SaveStringToFile(reDownBuilder.ToString(), Path.GetDirectoryName(savePath), Path.GetFileName(savePath));
                    Console.WriteLine("SQL文件已保存至:" + savePath);
                }
                else
                {
                    Console.WriteLine(reDownBuilder.ToString());
                    Console.WriteLine("没有指定保存路径");
                }
            }
        }

        /// <summary>
        /// 重新下载旧版本的
        /// </summary>
        private static void RedownloadOldVer(string dbPath, int maxYpid)
        {
            Console.WriteLine("检查并重新下载旧版本的乐谱播放文件");
            InitDB(dbPath);
            //找出所有旧版本的谱子
            var dataSet = SQLite.SqlTable("select ypid, name, origin_data from tan8_music where ypid <= @ypid", new List<SQLiteParameter>()
            {
                new SQLiteParameter("ypid", maxYpid)
            });
            var ypHomePath = ConfigUtil.Instance.Load().PianoScorePath;
            var fixNum = 0;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                if (!File.Exists(Path.Combine(ypHomePath, Convert.ToString(dataRow["ypid"]), "play.ypdx")))
                {
                    fixNum++;
                    var tan8Music = DataUtil.ParseToModel(Convert.ToString(dataRow["origin_data"]));
                    var downResult = new FileDownloader().DownloadFile(tan8Music.ypad_url2, Path.Combine(ypHomePath, Convert.ToString(dataRow["ypid"]), "play.ypdx"));
                    Console.WriteLine(Convert.ToInt32(dataRow["ypid"]) + " - " + Convert.ToString(dataRow["name"]) + "   播放文件下载" + (downResult == 0 ? "成功" : "失败"));
                }
            }
            Console.WriteLine("重新下载旧乐谱播放文件数量: " + fixNum);
        }

        /// <summary>
        /// 查找并删除没有有效乐谱页的乐谱
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="autoDel"></param>
        private static void Clean0PageYuepu(string dbPath, bool autoDel)
        {
            InitDB(dbPath);
            var total = 0;
            var ypHomePath = ConfigUtil.Instance.Load().PianoScorePath;
            var yp0Ypids = SQLite.sqlcolumn("SELECT ypid FROM tan8_music WHERE yp_count = 0", null);
            foreach(var ypid in yp0Ypids)
            {
                var files = Directory.GetFiles(Path.Combine(ypHomePath, ypid));
                var hasPlayFile = false;
                if(files.Length > 0)
                {
                    foreach(var fileName in files)
                    {
                        if(fileName.EndsWith("ypdx") || fileName.EndsWith("ypa2"))
                        {
                            hasPlayFile = true;
                            break;
                        }
                    }
                }
                if (!hasPlayFile)
                {
                    total++;
                    Console.WriteLine(ypid);
                    if (autoDel)
                    {
                        FileUtil.DeleteDirWithName(ypHomePath, ypid);
                    }
                }
            }
            Console.WriteLine("共" + total + "个");
        }

        /// <summary>
        /// 从旧乐谱库直接复制到新乐谱库
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="oldHomePath"></param>
        /// <param name="autoCopy"></param>
        private static void CheckOldHome(string dbPath, string oldHomePath, bool autoCopy)
        {
            Console.WriteLine("检查旧乐谱谱库");
            if (string.IsNullOrEmpty(oldHomePath))
            {
                Console.WriteLine("没有指定旧乐谱路径");
                return;
            }
            InitDB(dbPath);
            var ypHomePath = ConfigUtil.Instance.Load().PianoScorePath;
            var dataSet = SQLite.SqlTable("SELECT * FROM tan8_music_old", null);
            var total = dataSet.Rows.Count;
            var copyTotal = 0;
            var cur = 0;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                Console.WriteLine(dataRow["name"] + "   ----进度:" + ++cur + "/" + total + "");
                if (Directory.Exists(Path.Combine(oldHomePath, dataRow["name"] as string)))
                {
                    var files = Directory.GetFiles(Path.Combine(oldHomePath, dataRow["name"] as string));
                    var hasPlayFile = false;
                    if (files.Length > 0)
                    {
                        foreach (var fileName in files)
                        {
                            if (fileName.EndsWith("ypdx") || fileName.EndsWith("ypa2"))
                            {
                                hasPlayFile = true;
                                break;
                            }
                        }
                    }
                    if (hasPlayFile)
                    {
                        if (!autoCopy) continue;

                        var i = SQLite.ExecuteNonQuery("insert or ignore into tan8_music(ypid, `name`, star, yp_count, origin_data) VALUES (@ypid, @name, @star, @yp_count, @origin_data)",
                        new List<SQLiteParameter>
                        {
                            new SQLiteParameter("@ypid", dataRow["ypid"]) ,
                            new SQLiteParameter("@name", dataRow["name"]) ,
                            new SQLiteParameter("@star", 0 as object) ,
                            new SQLiteParameter("@yp_count", dataRow["yp_count"]) ,
                            new SQLiteParameter("@origin_data", dataRow["origin_data"])
                         });

                        if (i > 0)
                        {
                            //复制文件
                            if (!Directory.Exists(Path.Combine(ypHomePath, dataRow["ypid"] as string)))
                            {
                                FileUtil.CreateFolder(Path.Combine(ypHomePath, dataRow["ypid"] as string));
                            }
                            foreach (var file in files)
                            {
                                FileUtil.CopyFile(file, Path.Combine(ypHomePath, dataRow["ypid"] as string, Path.GetFileName(file)));
                            }
                            copyTotal++;
                        }
                    }
                }
            }
            Console.WriteLine("共复制" + copyTotal + "个目录");
        }

        private async static void InitDB(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                //如果没有指定数据库文件, 则使用默认
                dbPath = ConfigUtil.Instance.Load().DbFilePath;
            }
            if (!await SQLite.SetDbFilePath(dbPath))
            {
                Console.WriteLine("数据库没有正确配置");
            }
        }


        /// <summary>
        /// 检查并上传乐谱图片
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="threadCount"></param>
        private static void CheckSheetPreviewImg(string dbPath, int threadCount)
        {
            Console.WriteLine("执行上传乐谱首页任务, dbPath=" + dbPath + ", 线程数 = " + threadCount);
            InitDB(dbPath);
            var ypHomePath = ConfigUtil.Instance.Load().PianoScorePath;
            if (string.IsNullOrEmpty(ypHomePath))
            {
                Console.WriteLine("无法获取乐谱路径, 先检查一下配置文件");
                return;
            }
            ConcurrentQueue<PianoScore> sheetDirQueue = new ConcurrentQueue<PianoScore>();
            var dataSet = SQLite.SqlTable("SELECT ypid, name, yp_count FROM tan8_music WHERE ypid NOT IN (SELECT ypid FROM tan8_music_img)", null);
            var total = dataSet.Rows.Count;
            Console.WriteLine("正在添加任务队列...");
            foreach (DataRow dataRow in dataSet.Rows)
            {
                if (Directory.Exists(Path.Combine(ypHomePath, dataRow["ypid"] as string)))
                {
                    sheetDirQueue.Enqueue(new PianoScore()
                    {
                        id = Convert.ToInt32(dataRow["ypid"]),
                        Name = dataRow["name"].ToString(),
                        YpCount = Convert.ToByte(dataRow["yp_count"])
                    });
                }
            }
            Console.WriteLine("共添加" + total + "项任务");

            List<ManualResetEvent> manualEvents = new List<ManualResetEvent>();
            for (int i = 0; i < threadCount; i++)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                manualEvents.Add(mre);
                ThreadPool.QueueUserWorkItem((object obj) => 
                {
                    while (sheetDirQueue.Count > 0)
                    {
                        PianoScore pianoScore = new PianoScore();
                        var isOk = sheetDirQueue.TryDequeue(out pianoScore);
                        if (isOk)
                        {
                            var sheetDir = Path.Combine(ypHomePath, pianoScore.id.GetValueOrDefault().ToString());
                            var previewPicName = "public.png";
                            //检查目标文件夹是否已经存在已处理的图片
                            if (File.Exists(Path.Combine(ypHomePath, sheetDir, previewPicName)))
                            {
                                IImageRepo imageAPI = ImageRepoFactory.GetRandomApi();
                                //IImageRepo imageAPI = new PrntImageRepo();
                                ImageRepoUploadArg uploadArg = new ImageRepoUploadArg()
                                {
                                    FullFilePath = Path.Combine(ypHomePath, sheetDir, previewPicName),
                                    ExtraArgs = new JObject
                                    {
                                        { "uploadFileFormName", "sheet_" + pianoScore.id + ".png" }
                                    }
                                };
                                InvokeResult<ImageRepoUploadResult> invokeResult;
                                try
                                {
                                    invokeResult = imageAPI.Upload(uploadArg);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("api {0}, 引发异常, {1}", imageAPI.GetApiCode(), e.Message);
                                    continue;
                                }
                                if (!invokeResult.success)
                                {
                                    Console.WriteLine(pianoScore.Name + "上传失败, msg=" + invokeResult.message);
                                    continue;
                                }
                                SQLite.ExecuteNonQuery("INSERT INTO tan8_music_img(ypid, yp_name, img_url, api, api_channel, create_time) VALUES (@ypid, @ypName, @imgUrl, @api, @apiChannel, datetime('now', 'localtime'))",
                                    new List<SQLiteParameter>()
                                {
                                    new SQLiteParameter("@ypid", pianoScore.id.GetValueOrDefault()),
                                    new SQLiteParameter("@ypName", pianoScore.Name),
                                    new SQLiteParameter("@imgUrl", invokeResult.data.ImgUrl),
                                    new SQLiteParameter("@api", invokeResult.data.Api),
                                    new SQLiteParameter("@apiChannel", invokeResult.data.ApiChannel)
                                });
                                Console.WriteLine("剩余 : " + sheetDirQueue.Count);
                            }
                        }
                    }
                    ManualResetEvent localMre = (ManualResetEvent)obj;
                    localMre.Set();
                }, mre);
            }
            WaitHandle.WaitAll(manualEvents.ToArray());
            Console.WriteLine("乐谱图片上传完毕");
            Console.ReadKey();
        }

        /// <summary>
        /// 检查并将数据库所有曲目生成去水印封面
        /// </summary>
        /// <param name="dbPath">数据库路径</param>
        /// <param name="threadCount">最大线程数</param>
        private static void CheckWhiteBlackPreview(string dbPath, bool overwrite, int threadCount)
        {
            Console.WriteLine("执行水印任务, dbPath=" + dbPath + ", 线程数 = " + threadCount);
            InitDB(dbPath);
            var ypHomePath = ConfigUtil.Instance.Load().PianoScorePath;
            if(string.IsNullOrEmpty(ypHomePath))
            {
                Console.WriteLine("无法获取乐谱路径, 先检查一下配置文件");
                return;
            }
            ConcurrentQueue<PianoScore> sheetDirQueue = new ConcurrentQueue<PianoScore>();
            var dataSet = SQLite.SqlTable("SELECT ypid, name, yp_count FROM tan8_music", null);
            var total = dataSet.Rows.Count;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                if (Directory.Exists(Path.Combine(ypHomePath, dataRow["ypid"] as string)))
                {
                    Console.WriteLine(string.Format("正在添加 {0} 到任务队列...", dataRow["name"]));
                    sheetDirQueue.Enqueue(new PianoScore() 
                    {
                        id = Convert.ToInt32(dataRow["ypid"]),
                        Name = dataRow["name"].ToString(),
                        YpCount = Convert.ToByte(dataRow["yp_count"])
                    });
                }
            }

            for (int i = 0; i < threadCount; i++)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        PianoScore pianoScore = new PianoScore();
                        var isOk = sheetDirQueue.TryDequeue(out pianoScore);
                        if (isOk)
                        {
                            var sheetDir = Path.Combine(ypHomePath, pianoScore.id.GetValueOrDefault().ToString());
                            var previewPicName = "public.png";
                            bool doProcess = true;
                            //检查目标文件夹是否已经存在已处理的图片
                            if (File.Exists(Path.Combine(ypHomePath, sheetDir, previewPicName)))
                            {
                                doProcess = overwrite ? true : false;
                            }
                            if (!doProcess)
                            {
                                continue;
                            }
                            var sheetFiles = Directory.GetFiles(sheetDir);
                            foreach (var sheetFile in sheetFiles)
                            {
                                if (Path.GetFileName(sheetFile).Equals("page.0.png"))
                                {
                                    Console.WriteLine("剩余 : " + sheetDirQueue.Count + ", 当前 : " + Path.GetFileName(sheetDir));
                                    var sufId = "(" + pianoScore.id.GetValueOrDefault() + ")";
                                    var titleName = pianoScore.Name.EndsWith(sufId) ? pianoScore.Name.Substring(0, pianoScore.Name.Length - sufId.Length) : pianoScore.Name;
                                    Bitmap rawImg = (Bitmap)Bitmap.FromFile(sheetFile);
                                    Bitmap bmp = ImageUtil.CreateIegalTan8Sheet(rawImg, titleName, 1, pianoScore.YpCount, true);
                                    bmp.Save(Path.Combine(ypHomePath, sheetDir, previewPicName), ImageFormat.Png);
                                    bmp.Dispose();
                                }
                            }
                        }
                    }
                });
            }
            while (sheetDirQueue.Count > 0)
            {
                Thread.Sleep(1000);
            }
            //转换完毕打开目标文件夹
            Console.WriteLine("水印图片生成完毕");
        }

        private static void TestConvertImg()
        {
            var singleTestDirName = "";// 1、Dreamer's Waltz - David Lanz（Sacred Road）大卫·兰兹【神圣之路】钢琴曲集";
            try
            {
                if(string.IsNullOrEmpty(singleTestDirName))
                {
                    ConcurrentQueue<string> sheetDirQueue = new ConcurrentQueue<string>();
                    var sheetHome = Directory.GetDirectories(@"E:\\曲谱");
                    foreach (var sheetDir in sheetHome)
                    {
                        sheetDirQueue.Enqueue(sheetDir);
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        Task.Run(() =>
                        {
                            while (true)
                            {
                                var sheetDir = string.Empty;
                                var isOk = sheetDirQueue.TryDequeue(out sheetDir);
                                if (isOk)
                                {
                                    var sheetFiles = Directory.GetFiles(sheetDir);
                                    foreach (var sheetFile in sheetFiles)
                                    {
                                        if (Path.GetFileName(sheetFile).Equals("page.0.png"))
                                        {
                                            Console.WriteLine(Path.GetFileName(sheetDir));
                                            Bitmap rawImg = (Bitmap)Bitmap.FromFile(sheetFile);
                                            Bitmap bmp = ImageUtil.CreateIegalTan8Sheet(rawImg, Path.GetFileName(sheetDir), 1, 10, true);
                                            bmp.Save(Path.Combine(@"C:\Users\Administrator\Desktop\去水印", Path.GetFileName(sheetDir) + ".png"), ImageFormat.Png);
                                            bmp.Dispose();
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        });
                    }
                    Console.ReadKey();
                }
                else
                {
                    string dirName = singleTestDirName;
                    Bitmap rawImg = (Bitmap)Bitmap.FromFile(@"E:\曲谱\" + dirName + @"\page.0.png");
                    Bitmap bmp = ImageUtil.CreateIegalTan8Sheet(rawImg, dirName, 1 , 10, true);
                    bmp.Save(Path.Combine(@"C:\Users\Administrator\Desktop\去水印", dirName + ".png"), ImageFormat.Png);
                    bmp.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void CheckDirName(string dbPath)
        {
            Console.WriteLine("重命名乐谱文件夹名称");
            InitDB(dbPath);
            var ypHomePath = ConfigUtil.Instance.Load().PianoScorePath;
            if (string.IsNullOrEmpty(ypHomePath))
            {
                Console.WriteLine("无法获取乐谱路径, 先检查一下配置文件");
                return;
            }
            var dir = Directory.GetDirectories(ypHomePath);
            foreach (var d in dir)
            {
                var folderName = Path.GetFileName(d);
                var ypid = SQLite.sqlone("SELECT ypid FROM tan8_music where name = @name", new SQLiteParameter[] { new SQLiteParameter("@name", folderName) });
                if(!string.IsNullOrEmpty(ypid))
                {
                    FileUtil.RenameFolder(Path.Combine(ypHomePath, folderName), ypid);
                    Console.WriteLine("{0} -> {1}", d, ypid);
                }
            }
            Console.WriteLine("重命名完成");
            Console.ReadKey();
        }
    }
}
