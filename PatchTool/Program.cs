using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;

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
            var command = "?";
            var autoDel = false;
            var autoCopy = false;
            var savePath = string.Empty;
            var oldHomePath = string.Empty;
            var dbPath = string.Empty;
            var maxYpid = 0;
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
            }
            Console.ReadKey();
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
                if (!Directory.Exists(Path.Combine(ypHomePath, dataRow["name"] as string)))
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
                var name = Path.GetFileName(d);
                var num = SQLite.sqlone("select count(1) num from tan8_music where name = @name", new SQLiteParameter[] { new SQLiteParameter("@name", name) });
                var num32 = Convert.ToInt32(num);
                if(num32 == 0)
                {
                    total++;
                    allFolderNames.Append(name).Append("\r\n");
                    Console.WriteLine(name);
                    if (autoDel) FileUtil.DeleteDirWithName(ypHomePath, name);
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
                    FileUtil.DeleteDirWithName(ypHomePath, dataRow["name"] as string);
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
                if (!File.Exists(Path.Combine(ypHomePath, Convert.ToString(dataRow["name"]), "play.ypdx")))
                {
                    fixNum++;
                    var tan8Music = DataUtil.ParseToModel(Convert.ToString(dataRow["origin_data"]));
                    var downResult = new FileDownloader().DownloadFile(tan8Music.ypad_url2, Path.Combine(ypHomePath, Convert.ToString(dataRow["name"]), "play.ypdx"));
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
            var yp0Names = SQLite.sqlcolumn("SELECT name FROM tan8_music WHERE yp_count = 0", null);
            foreach(var name in yp0Names)
            {
                var files = Directory.GetFiles(Path.Combine(ypHomePath, name));
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
                    Console.WriteLine(name);
                    if (autoDel)
                    {
                        FileUtil.DeleteDirWithName(ypHomePath, name);
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
                            if (!Directory.Exists(Path.Combine(ypHomePath, dataRow["name"] as string)))
                            {
                                FileUtil.CreateFolder(Path.Combine(ypHomePath, dataRow["name"] as string));
                            }
                            foreach (var file in files)
                            {
                                FileUtil.CopyFile(file, Path.Combine(ypHomePath, dataRow["name"] as string, Path.GetFileName(file)));
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
                //如果没有指定数据库文件, 则使用默认
                dbPath = ConfigUtil.Instance.Load().DbFilePath;
            }
            if (!SQLite.SetDbFilePath(dbPath))
            {
                Console.WriteLine("数据库没有正确配置");
            }
        }
    }
}
