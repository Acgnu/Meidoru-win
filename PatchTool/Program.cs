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
using PatchTool.Properties;
using SharedLib.ImageNetRepository;
using SharedLib.Model;
using SharedLib.Utils;

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
               //SetDB(@"E:\曲谱\master.db");
               //ClearInvalidPlayFileSheet(false, @"E:\曲谱");
               //if (true)
               //{
               //    return;
               //}
            var command = "?";
            var autoDel = false;
            var autoCopy = false;
            var savePath = string.Empty;
            var oldHomePath = string.Empty;
            var ypHomePath = string.Empty;
            var dbPath = string.Empty;
            var threadCount = 5;
            var maxYpid = 0;
            var overwrite = false;
            var sourceFilePath = string.Empty;
            //switch (command)
            //{
            //    case "?": ShowTips(); break;
            //    case "ckdir": ShowFolderNotExistsDB(dbPath, savePath, autoDel); break;
            //    case "cknon": Clean0PageYuepu(dbPath, autoDel); break;
            //    case "ckrpt": CheckRepeat(dbPath, savePath, autoDel); break;
            //    case "ckold": RedownloadOldVer(dbPath, maxYpid); break;
            //    case "ckdb": ShowNameNotExistsFolder(dbPath, savePath); break;
            //    case "ckodh": CheckOldHome(dbPath, oldHomePath, autoCopy); break;
            //    case "ckwb": CheckWhiteBlackPreview(dbPath, overwrite, threadCount); break;
            //    case "ckimg" : CheckSheetPreviewImg(dbPath, threadCount); break;
            //    case "ckdirnm": CheckDirName(dbPath); break;
            //}

            Console.WriteLine("1 [-d] [-f] [-r]\t显示不存在数据库的文件夹");
            Console.WriteLine("2 [-d] [-r]\t\t查找并删除没有有效乐谱页的乐谱");
            Console.WriteLine("3 [-d] [-f] [-r]\t重新下载重名的乐谱");
            Console.WriteLine("4 [-d] [-i]\t\t重新下载旧版本的乐谱");
            Console.WriteLine("5 [-d] [-f]\t\t显示数据库中有但是文件夹里没有的乐谱");
            Console.WriteLine("6 [-d] [-h] [-c]\t从旧乐谱库直接复制到新乐谱库");
            Console.WriteLine("7 [-d] [-o] [-t]\t检查并将数据库所有曲目生成去水印封面");
            Console.WriteLine("8 [-d] [-t]\t\t检查并上传乐谱图片");
            Console.WriteLine("9 [-d]\t\t\t以乐谱ID重命名文件夹");
            Console.WriteLine("10 [-f] [-l]\t\tMD5排重乐谱");
            Console.WriteLine("11 [-d] [-l]\t\t进入按ID批量删除模式");
            Console.WriteLine("12 [-d] [-l] [-s]\t进入按文件批量删除模式");
            Console.WriteLine("13 [-r] [-l] \t\t批量删除无法播放的0页乐谱");
            Console.WriteLine("e \t\t\t退出\n");

            Console.WriteLine("命令编号 [可选参数1] [可选参数n...] 例:1 -dD:\\master.db -fD:\\autosave -r");
            Console.WriteLine("-r 自动删除");
            Console.WriteLine("-c 自动复制");
            Console.WriteLine("-f 保存路径 例: -fC:\\Desktop\a.txt");
            Console.WriteLine("-h 旧乐谱路径 例: -hC:\\Desktop");
            Console.WriteLine("-l 乐谱路径 例: -hC:\\Desktop");
            Console.WriteLine("-d 数据库路径 例: -dC:\\master.db");
            Console.WriteLine("-t 线程数量 默认5 例: -t10");
            Console.WriteLine("-i 最大乐谱ID 默认0 例: -i666");
            Console.WriteLine("-s 输入文件路径 例: -sC:\\sql.sql");
            Console.WriteLine("-o 是否覆盖");
            
            while(true)
            {
                var inputLine = Console.ReadLine();
                if(inputLine.Equals("e"))
                {
                    return;
                }
                var commandArgs = inputLine.Split(' ');

                if (commandArgs.Length > 0)
                {
                    command = commandArgs[0];
                    foreach (var arg in commandArgs)
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
                        if (arg.StartsWith("-h"))
                        {
                            oldHomePath = arg.Substring(2);
                        }
                        if (arg.StartsWith("-l"))
                        {
                            ypHomePath = arg.Substring(2);
                        }
                        if (arg.Equals("-c"))
                        {
                            autoCopy = true;
                        }
                        if (arg.StartsWith("-t"))
                        {
                            threadCount = Convert.ToInt32(arg.Substring(2));
                        }
                        if (arg.StartsWith("-s"))
                        {
                            sourceFilePath = arg.Substring(2);
                        }
                        if (arg.Equals("-o"))
                        {
                            overwrite = true;
                        }
                    }
                }

                InitDB(dbPath);

                switch (command)
                {
                    case "1": ShowFolderNotExistsDB(savePath, autoDel); break;
                    case "2": Clean0PageYuepu(autoDel); break;
                    case "3": CheckRepeat(savePath, autoDel); break;
                    case "4": RedownloadOldVer(maxYpid); break;
                    case "5": ShowNameNotExistsFolder(savePath); break;
                    case "6": CheckOldHome(oldHomePath, autoCopy); break;
                    case "7": CheckWhiteBlackPreview(overwrite, threadCount); break;
                    case "8": CheckSheetPreviewImg(threadCount); break;
                    case "10": CheckFileMD5(savePath, ypHomePath); break;
                    case "11": BatchDel(ypHomePath); break;
                    case "12": BatchDelFromInputFile(ypHomePath, sourceFilePath); break;
                    case "13": ClearInvalidPlayFileSheet(autoDel, ypHomePath); break;
                    default: Console.WriteLine("命令不正确"); break;
                }
            }
        }


        //private static void ShowTips()
        //{
        //    Console.WriteLine("太麻烦了懒得写, 去看源码");
        //}

        /// <summary>
        /// 显示数据库中有但是文件夹里没有的
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="savePath"></param>
        private static void ShowNameNotExistsFolder(string savePath)
        {
            Console.WriteLine("检查仅存在数据库中的乐谱");

            var totalNe = 0;
            var cur = 0;
            var delSQL = new StringBuilder("DELETE FROM tan8_music WHERE ypid in (");
            //找出文件夹存在, 而数据库中不存在的
            
            var ypHomePath = Settings.Default.Tan8HomeDir;
            var dataSet = SQLite.SqlTable("SELECT ypid, name FROM tan8_music", null);
            var total = dataSet.Rows.Count;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                cur++;
                var yuepuPath = FileUtil.GetTan8YuepuFolder(ypHomePath, Convert.ToString(dataRow["ypid"]));
                if (!Directory.Exists(yuepuPath))
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
        private static void ShowFolderNotExistsDB(string savePath, bool autoDel)
        {
            Console.WriteLine("检查不存在于数据的文件夹");
            //找出文件夹存在, 而数据库中不存在的
            var ypHomePath = Settings.Default.Tan8HomeDir;
            var dir = Directory.GetDirectories(ypHomePath);
            var allFolderNames = new StringBuilder();
            var total = 0;
            foreach (var d in dir)
            {
                var dir2 = Directory.GetDirectories(d);
                foreach (var d2 in dir2)
                {
                    var ypid = Path.GetFileName(d2);
                    var num = SQLite.sqlone("select count(1) num from tan8_music where ypid = @ypid", new SQLiteParameter[] { new SQLiteParameter("@ypid", ypid) });
                    var num32 = Convert.ToInt32(num);
                    if (num32 == 0)
                    {
                        total++;
                        allFolderNames.Append(ypid).Append("\r\n");
                        Console.WriteLine(ypid);
                        if (autoDel) FileUtil.DeleteDirWithName(FileUtil.GetTan8YuepuParentFolder(ypHomePath, ypid), ypid);
                    }
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
        private static void CheckRepeat(string savePath, bool autoDel)
        {
            Console.WriteLine("检查并重新下载重名的乐谱");
            var ypHomePath = Settings.Default.Tan8HomeDir;
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
                        FileUtil.DeleteDirWithName(FileUtil.GetTan8YuepuParentFolder(ypHomePath, e), e);
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
        private static void RedownloadOldVer(int maxYpid)
        {
            Console.WriteLine("检查并重新下载旧版本的乐谱播放文件");
            //找出所有旧版本的谱子
            var dataSet = SQLite.SqlTable("select ypid, name, origin_data from tan8_music where ypid <= @ypid", new List<SQLiteParameter>()
            {
                new SQLiteParameter("ypid", maxYpid)
            });
            var ypHomePath = Settings.Default.Tan8HomeDir;
            var fixNum = 0;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                var yuepuPath = FileUtil.GetTan8YuepuFolder(ypHomePath, Convert.ToString(dataRow["ypid"]));
                if (!File.Exists(Path.Combine(yuepuPath, "play.ypdx")))
                {
                    fixNum++;
                    var tan8Music = DataUtil.ParseToModel(Convert.ToInt32(dataRow["ypid"]), Convert.ToString(dataRow["origin_data"]));
                    var downResult = new FileDownloader().DownloadFile(tan8Music.ypdx_url, Path.Combine(yuepuPath, "play.ypdx"));
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
        private static void Clean0PageYuepu(bool autoDel)
        {
            var total = 0;
            var ypHomePath = Settings.Default.Tan8HomeDir;
            var yp0Ypids = SQLite.sqlcolumn("SELECT ypid FROM tan8_music WHERE yp_count = 0", null);
            foreach(var ypid in yp0Ypids)
            {
                var files = Directory.GetFiles(FileUtil.GetTan8YuepuFolder(ypHomePath, ypid));
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
                        FileUtil.DeleteDirWithName(FileUtil.GetTan8YuepuParentFolder(ypHomePath, ypid), ypid);
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
        private static void CheckOldHome(string oldHomePath, bool autoCopy)
        {
            Console.WriteLine("检查旧乐谱谱库");
            if (string.IsNullOrEmpty(oldHomePath))
            {
                Console.WriteLine("没有指定旧乐谱路径");
                return;
            }
            var ypHomePath = Settings.Default.Tan8HomeDir;
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
                            var yuepuPath = FileUtil.GetTan8YuepuFolder(ypHomePath, Convert.ToString(dataRow["ypid"]));
                            if (!Directory.Exists(yuepuPath))
                            {
                                FileUtil.CreateFolder(yuepuPath);
                            }
                            foreach (var file in files)
                            {
                                FileUtil.CopyFile(file, Path.Combine(yuepuPath, Path.GetFileName(file)));
                            }
                            copyTotal++;
                        }
                    }
                }
            }
            Console.WriteLine("共复制" + copyTotal + "个目录");
        }

        private static void InitDB(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                //var evnFolder = Environment.CurrentDirectory;
                //var svcFolder = evnFolder.Replace(System.Reflection.Assembly.GetEntryAssembly().GetName().Name, "AcgnuX");
                //var configPath = Path.Combine(svcFolder, "AcgnuX.ini");
                //如果没有指定数据库文件, 则使用默认
                dbPath = Settings.Default.DBFilePath;
            }
            Console.WriteLine("数据库路径 :" + dbPath);
            SQLite.SetDbFilePath(dbPath);
        }


        /// <summary>
        /// 检查并上传乐谱图片
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="threadCount"></param>
        private static void CheckSheetPreviewImg(int threadCount)
        {
            Console.WriteLine("执行上传乐谱首页任务, 线程数 = " + threadCount);
            var ypHomePath = Settings.Default.Tan8HomeDir;
            if (string.IsNullOrEmpty(ypHomePath))
            {
                Console.WriteLine("无法获取乐谱路径, 先检查一下配置文件");
                return;
            }
            ConcurrentQueue<Tan8Sheet> sheetDirQueue = new ConcurrentQueue<Tan8Sheet>();
            var dataSet = SQLite.SqlTable("SELECT ypid, name, yp_count FROM tan8_music WHERE ypid NOT IN (SELECT ypid FROM tan8_music_img)", null);
            var total = dataSet.Rows.Count;
            Console.WriteLine("正在添加任务队列...");
            foreach (DataRow dataRow in dataSet.Rows)
            {
                if (Directory.Exists(FileUtil.GetTan8YuepuFolder(ypHomePath, Convert.ToString(dataRow["ypid"]))))
                {
                    sheetDirQueue.Enqueue(new Tan8Sheet()
                    {
                        Ypid = Convert.ToInt32(dataRow["ypid"]),
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
                        Tan8Sheet pianoScore = new Tan8Sheet();
                        var isOk = sheetDirQueue.TryDequeue(out pianoScore);
                        if (isOk)
                        {
                            var sheetDir = FileUtil.GetTan8YuepuFolder(ypHomePath, pianoScore.Ypid.ToString());
                            var previewPicName = "public.png";
                            //检查目标文件夹是否已经存在已处理的图片
                            if (File.Exists(Path.Combine(sheetDir, previewPicName)))
                            {
                                IImageRepo imageAPI = ImageRepoFactory.GetRandomApi();
                                //IImageRepo imageAPI = new PrntImageRepo();
                                var extraArgs = new Dictionary<string, string>();
                                extraArgs.Add("uploadFileFormName", "sheet_" + pianoScore.Ypid + ".png");
                                ImageRepoUploadArg uploadArg = new ImageRepoUploadArg()
                                {
                                    FullFilePath = Path.Combine(sheetDir, previewPicName),
                                    ExtraArgs = extraArgs
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
                                    new SQLiteParameter("@ypid", pianoScore.Ypid),
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
        }

        /// <summary>
        /// 检查并将数据库所有曲目生成去水印封面
        /// </summary>
        /// <param name="dbPath">数据库路径</param>
        /// <param name="threadCount">最大线程数</param>
        private static void CheckWhiteBlackPreview(bool overwrite, int threadCount)
        {
            Console.WriteLine("执行水印任务, 线程数 = " + threadCount);
            var ypHomePath = Settings.Default.Tan8HomeDir;
            if(string.IsNullOrEmpty(ypHomePath))
            {
                Console.WriteLine("无法获取乐谱路径, 先检查一下配置文件");
                return;
            }
            ConcurrentQueue<Tan8Sheet> sheetDirQueue = new ConcurrentQueue<Tan8Sheet>();
            var dataSet = SQLite.SqlTable("SELECT ypid, name, yp_count FROM tan8_music", null);
            var total = dataSet.Rows.Count;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                if (Directory.Exists(FileUtil.GetTan8YuepuFolder(ypHomePath, Convert.ToString(dataRow["ypid"]))))
                {
                    Console.WriteLine(string.Format("正在添加 {0} 到任务队列...", dataRow["name"]));
                    sheetDirQueue.Enqueue(new Tan8Sheet() 
                    {
                        Ypid = Convert.ToInt32(dataRow["ypid"]),
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
                        Tan8Sheet pianoScore = new Tan8Sheet();
                        var isOk = sheetDirQueue.TryDequeue(out pianoScore);
                        if (isOk)
                        {
                            var sheetDir = FileUtil.GetTan8YuepuFolder(ypHomePath, pianoScore.Ypid.ToString());
                            var previewPicName = "public.png";
                            bool doProcess = true;
                            //检查目标文件夹是否已经存在已处理的图片
                            if (File.Exists(Path.Combine(sheetDir, previewPicName)))
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
                                    var sufId = "(" + pianoScore.Ypid + ")";
                                    var titleName = pianoScore.Name.EndsWith(sufId) ? pianoScore.Name.Substring(0, pianoScore.Name.Length - sufId.Length) : pianoScore.Name;
                                    Bitmap rawImg = (Bitmap)Bitmap.FromFile(sheetFile);
                                    Bitmap bmp = Tan8SheetMaskUtil.CreateIegalTan8Sheet(rawImg, titleName, 1, pianoScore.YpCount, true);
                                    bmp.Save(Path.Combine(sheetDir, previewPicName), ImageFormat.Png);
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
                                            Bitmap bmp = Tan8SheetMaskUtil.CreateIegalTan8Sheet(rawImg, Path.GetFileName(sheetDir), 1, 10, true);
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
                }
                else
                {
                    string dirName = singleTestDirName;
                    Bitmap rawImg = (Bitmap)Bitmap.FromFile(@"E:\曲谱\" + dirName + @"\page.0.png");
                    Bitmap bmp = Tan8SheetMaskUtil.CreateIegalTan8Sheet(rawImg, dirName, 1 , 10, true);
                    bmp.Save(Path.Combine(@"C:\Users\Administrator\Desktop\去水印", dirName + ".png"), ImageFormat.Png);
                    bmp.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// 对播放文件执行MD5校验, 记录重复的乐谱ID
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="savePath"></param>
        private static void CheckFileMD5(string savePath, string ypHomePath)
        {
            Console.WriteLine("MD5排重...");
            ypHomePath = string.IsNullOrEmpty(ypHomePath) ? Settings.Default.Tan8HomeDir : ypHomePath;
            if (string.IsNullOrEmpty(ypHomePath))
            {
                Console.WriteLine("无法获取乐谱路径, 先检查一下配置文件");
                return;
            }
            var dir = Directory.GetDirectories(ypHomePath);
            //key = md5, vaue = 路径
            Dictionary<string, string> md5 = new Dictionary<string, string>();
            //key = md5, value = 重复id集合
            Dictionary<string, List<string>> repeat = new Dictionary<string, List<string>>();
            for (var i = 0; i < dir.Length; i++)
            {
                var dir2 = Directory.GetDirectories(dir[i]);
                foreach (var d in dir2)
                {
                    //Console.Clear();
                    Console.WriteLine("进度 {0} / {1} / {2}", i + 1, dir.Length, dir2.Length);
                    var folderName = Path.GetFileName(d);
                    var playFileHash = string.Empty;
                    if (File.Exists(Path.Combine(d, "play.ypdx")))
                    {
                        playFileHash = FileUtil.GetMD5(Path.Combine(d, "play.ypdx"));
                    }
                    else if (File.Exists(Path.Combine(d, "play.ypa2")))
                    {
                        playFileHash = FileUtil.GetMD5(Path.Combine(d, "play.ypa2"));
                    }
                    else
                    {
                        continue;
                    }
                    if (md5.ContainsKey(playFileHash))
                    {
                        if (repeat.ContainsKey(playFileHash))
                        {
                            repeat[playFileHash].Add(folderName);
                        }
                        else
                        {
                            repeat[playFileHash] = new List<string>()
                        {
                            md5[playFileHash],
                            folderName
                        };
                        }
                    }
                    else
                    {
                        md5[playFileHash] = folderName;
                    }
                }
            }

            int repeatSheetNum = 0;
            int repeatFileNum = 0;
            StringBuilder sql = new StringBuilder();
            foreach (var item in repeat)
            {
                sql.Append("SELECT * FROM tan8_music WHERE ypid IN (");
                foreach (var ypid in item.Value)
                {
                    sql.Append(ypid).Append(",");
                    repeatFileNum++;
                }
                sql.Remove(sql.Length - 1, 1);
                sql.Append("); \n");
                repeatSheetNum++;
            }
            Console.WriteLine(sql.ToString());
            Console.WriteLine("检查完毕, 共 {0} 首乐谱重复, 共 {1} 个文件", repeatSheetNum, repeatFileNum);
            if(!string.IsNullOrEmpty(savePath))
            {
                FileUtil.SaveStringToFile(sql.ToString(), Path.GetDirectoryName(savePath), Path.GetFileName(savePath));
                Console.WriteLine("SQL文件已保存至{0}", savePath);
            }
        }

        /// <summary>
        /// 根据乐谱ID批量删除乐谱模式
        /// </summary>
        /// <param name="ypHome"></param>
        private static void BatchDel(string ypHome)
        {
            var delHome = string.IsNullOrEmpty(ypHome) ? Settings.Default.Tan8HomeDir : ypHome;
            Console.WriteLine("已进入批量删除模式, 复制ID到输入框, 回车即可删除指定乐谱, 输入 e 回车退出");
            Console.WriteLine("谱库路径: {0}", delHome);
            while (true)
            {
                var line = Console.ReadLine();
                if(string.IsNullOrEmpty(line))
                {
                    Console.WriteLine("输入有误");
                    continue;
                }
                if("e".Equals(line))
                {
                    break;
                }
                var ypid = Convert.ToInt32(line);
                //删除文件夹
                FileUtil.DeleteDirWithName(FileUtil.GetTan8YuepuParentFolder(delHome, line), line);

                //删除数据库数据
                SQLite.ExecuteNonQuery("DELETE FROM tan8_music WHERE ypid = @ypid", new List<SQLiteParameter> { new SQLiteParameter("@ypid", ypid) });
                Console.WriteLine("已删除 {0}", line);
            }
        }

        /// <summary>
        /// 从输入文件中查询的批量删除模式
        /// </summary>
        /// <param name="ypHome"></param>
        /// <param name="inputSQLFilePath"></param>
        private static void BatchDelFromInputFile(string ypHome, string inputSQLFilePath)
        {
            if(string.IsNullOrEmpty(inputSQLFilePath))
            {
                Console.WriteLine("输入文件路径有误");
                return;
            }
            if(!File.Exists(inputSQLFilePath))
            {
                Console.WriteLine("输入文件不存在");
                return;
            }
            var delHome = string.IsNullOrEmpty(ypHome) ? Settings.Default.Tan8HomeDir : ypHome;
            
            Console.WriteLine("已进入批量删除模式, 系统将列出重复的选项, 输入需要选择需要保留的选项, 回车删除其他乐谱");
            Console.WriteLine("谱库路径: {0}", delHome);

            using (FileStream fs = new FileStream(inputSQLFilePath, FileMode.Open))
            {
                using (StreamReader fileStream = new StreamReader(fs))
                {
                    string line;
                    int lastLineNo = 1;
                    StringBuilder lastDel = new StringBuilder();
                    while ((line = fileStream.ReadLine()) != null)
                    {
                        Console.Clear();
                        if(lastDel.Length > 0)
                        {
                            Console.WriteLine("已删除: {0}", lastDel.ToString());
                            lastDel.Clear();
                        }
                        var dataSet = SQLite.SqlTable(line, null);
                        var seq = 1;
                        var seqDic = new Dictionary<Int32, Int32>();
                        Console.WriteLine("序号\tID\t页数\t名称");
                        foreach (DataRow dataRow in dataSet.Rows)
                        {
                            Console.WriteLine("{0}\t{1}\t{2}\t{3}",
                                seq,
                                Convert.ToInt32(dataRow["ypid"]),
                                Convert.ToString(dataRow["yp_count"]),
                                Convert.ToString(dataRow["name"]));
                            seqDic[seq++] = Convert.ToInt32(dataRow["ypid"]);
                        }

                        Console.Write("\n行号: {0}, 输入需要保留的序号, 输入0全部删除, 输入00跳过:", lastLineNo++);

                        var skip = false;
                        do
                        {
                            var input = Console.ReadLine();
                            if ("0".Equals(input)) break;
                            if ("00".Equals(input))
                            {
                                skip = true;
                                break;
                            }
                            if (string.IsNullOrEmpty(input)
                                || !DataUtil.IsNum(input)
                                || !seqDic.ContainsKey(Convert.ToInt32(input)))
                            {
                                Console.WriteLine("输入无效");
                                continue;
                            }
                            seqDic.Remove(Convert.ToInt32(input));
                            break;
                        } while (true);
                        if (skip)
                        {
                            Console.WriteLine("已跳过");
                            continue;
                        }
                        foreach (var item in seqDic)
                        {
                            //删除文件夹
                            FileUtil.DeleteDirWithName(FileUtil.GetTan8YuepuParentFolder(delHome, Convert.ToString(item.Value)), Convert.ToString(item.Value));

                            //删除数据库数据
                            SQLite.ExecuteNonQuery("DELETE FROM tan8_music WHERE ypid = @ypid", new List<SQLiteParameter> { new SQLiteParameter("@ypid", item.Value) });

                            lastDel.Append(item.Value).Append(",");
                        }
                    }
                    Console.Clear();
                    Console.WriteLine("\n已全部处理");
                }
            }
        }

        /// <summary>
        /// 删除无法播放的乐谱
        /// </summary>
        /// <param name="autoDel"></param>
        /// <param name="ypHomePath"></param>
        private static void ClearInvalidPlayFileSheet(bool autoDel, string ypHomePath)
        {
            var homePath = string.IsNullOrEmpty(ypHomePath) ? Settings.Default.Tan8HomeDir : ypHomePath;
            Console.WriteLine("谱库路径: {0}", homePath);
            var yp0Ypids = SQLite.sqlcolumn("SELECT ypid FROM tan8_music WHERE yp_count = 0", null);
            Console.WriteLine("共 {0} 个0页乐谱", yp0Ypids.Count);
            var checkedNum = 0;
            var totalDelNum = 0;
            foreach (var ypid in yp0Ypids)
            {
                Console.WriteLine("正在检查: {0}, 剩余 {1}", ypid, yp0Ypids.Count - checkedNum++);
                Tan8PlayUtil.ExePlayById(Convert.ToInt32(ypid), 2, false);

                var timeout = true;
                for(var i = 0; i < 10; i++)
                {
                    Thread.Sleep(1000);
                    var errDialog = FindWindow(null, "pmady");
                    if (errDialog != IntPtr.Zero)
                    {
                        Console.WriteLine("{0} 无法播放", ypid);
                        if (autoDel)
                        {
                            //删除文件夹
                            FileUtil.DeleteDirWithName(FileUtil.GetTan8YuepuParentFolder(homePath, ypid), ypid);

                            //删除数据库数据
                            SQLite.ExecuteNonQuery("DELETE FROM tan8_music WHERE ypid = @ypid", new List<SQLiteParameter> { new SQLiteParameter("@ypid", ypid) });
                            totalDelNum++;
                        }
                        timeout = false;
                        break;
                    }
                }    
                if(timeout)
                {
                    Console.WriteLine("{0} OK!", ypid);
                }
            }
            Console.WriteLine("已全部处理, 共删除 {0} 个", totalDelNum);
        }


        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private extern static IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);

        private static void FindAndHit()
        {
            const int BM_CLICK = 0xF5;
            IntPtr maindHwnd = FindWindow(null, "QQ"); //获得QQ登陆框的句柄
            if (maindHwnd != IntPtr.Zero)
            {
                IntPtr childHwnd = FindWindowEx(maindHwnd, IntPtr.Zero, null, "登录");   //获得button的句柄
                if (childHwnd != IntPtr.Zero)
                {
                    SendMessage(childHwnd, BM_CLICK, 0, 0);     //发送点击button的消息
                }
                else
                {
                    Console.WriteLine("没有找到子窗体");
                }
            }
            else
            {
                Console.WriteLine("没有找到窗体");
            }
        }
    }
}
