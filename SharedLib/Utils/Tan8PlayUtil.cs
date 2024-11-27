using System.Diagnostics;

namespace SharedLib.Utils
{
    /// <summary>
    /// flash 播放工具
    /// </summary>
    public class Tan8PlayUtil
    {
        /// <summary>
        /// FlashPlayer.exe 执行时信任的swf配置目录
        /// 修改过的flash请求localhost执行会受到flashplayer的安全限制
        /// 此处创建白名单文件解除限制
        /// </summary>
        private static readonly string mFlashSecurityPath = Path.Combine("Macromedia", "Flash Player", "#Security", "FlashPlayerTrust");
        /// <summary>
        /// 针对此应用程序的配置文件
        /// </summary>
        private static readonly string mFlashTrustFileName = "acgnux_flash.cfg";
        /// <summary>
        /// flash exe线程
        /// key=乐谱ID, value=线程
        /// </summary>
        private static readonly Dictionary<int, Process> _PlayerProcessDictionary = new Dictionary<int, Process>();
        /// <summary>
        /// 标记信任文件是否已经检查
        /// </summary>
        private static bool _CheckedTrustFile = false;

        /// <summary>
        /// 使用外部flashplayer.exe播放swf文件
        /// </summary>
        /// <param name="command">
        /// <param name="isHide">true 最小化启动</param>
        /// 命令行
        /// </param>
        //public static void ExePlay(string command, bool isHide)
        //{
        //Exit();
        //mFlashProcess = new Process();
        //mFlashProcess.StartInfo.FileName = Environment.CurrentDirectory + @"\Assets\flash\flashplayer.exe";
        //mFlashProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory + "/maininteractive/ccv/";//程序闪退问题，要强行指定工作目录
        //mFlashProcess.StartInfo.UseShellExecute = true;//关键代码
        //mFlashProcess.StartInfo.WindowStyle = isHide ? ProcessWindowStyle.isHide : ProcessWindowStyle.Normal;//关键代码
        //mFlashProcess.StartInfo.Arguments = command;
        //mFlashProcess.Start();
        //mFlashProcess = Process.Start(Environment.CurrentDirectory + @"\Assets\flash\flashplayer.exe", command);
        //}

        /// <summary>
        /// 根据乐谱id打开弹琴吧播放器
        /// </summary>
        /// <param name="ypid">乐谱ID</param>
        /// <param name="version">1 使用 flash播放器, 2 使用v2版exe播放器</param>
        /// <param name="isHide">true 隐藏播放器窗口</param>
        public static void ExePlayById(int ypid, int version, bool isHide)
        {
            Process? playerProcess;
            if (version == 1)
            {
                //检查信任文件是否存在
                WriteTrustFile();

                playerProcess = Process.Start(new ProcessStartInfo()
                {
                    FileName = Path.Combine(Environment.CurrentDirectory, "Assets", "flash", "flashplayer.exe"),
                    WindowStyle = isHide ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,//关键代码
                    Arguments = Path.Combine(Environment.CurrentDirectory, "Assets", "flash", "fuckTan8", "Main.swf?id=" + ypid),
                });
            }
            else if (version == 2)
            {
                playerProcess = Process.Start(new ProcessStartInfo()
                {
                    FileName = Path.Combine(Environment.CurrentDirectory, "Assets", "exe", "77player.exe"),
                    WindowStyle = isHide ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,//关键代码
                    Arguments = string.Format("file=http://www.fucktan8.com/open_yp.php?ypid={0}&uid=999999999", ypid)
                });
            }
            else
            {
                throw new Exception("不支持的播放器版本");
            }
            _PlayerProcessDictionary.Add(ypid, playerProcess);
        }

        /// <summary>
        /// 退出flash线程, 并删除管理key
        /// 此处有个问题, 如果直接关闭播放器, 此处不会有通知, 考虑到占用不会太高, 暂时不处理
        /// </summary>
        public static void Exit(int ypid)
        {
            var hasKey = ExitProcess(ypid);
            if (hasKey) _PlayerProcessDictionary.Remove(ypid);
        }

        /// <summary>
        /// 退出线程
        /// </summary>
        /// <param name="ypid"></param>
        /// <returns></returns>
        private static bool ExitProcess(int ypid)
        {
            if (!_PlayerProcessDictionary.ContainsKey(ypid))
            {
                return false;
            }
            var playerProcess = _PlayerProcessDictionary[ypid];
            //检查进程是否存在
            if (null != playerProcess && !playerProcess.HasExited)
            {
                playerProcess.CloseMainWindow();
                //mFlashProcess.Close();
                if (!playerProcess.HasExited)
                {
                    playerProcess.Kill();
                }
            }
            return true;
        }

        /// <summary>
        /// 退出全部
        /// </summary>
        public static void ExitAll()
        {
            foreach (var k in _PlayerProcessDictionary)
            {
                ExitProcess(k.Key);
            }
        }

        /// <summary>
        /// 重启Flash播放器
        /// </summary>
        /// <param name="prevYpid">重启前的乐谱ID</param>
        /// <param name="ypid">新的乐谱ID</param>
        /// <param name="ver"></param>
        /// <param name="isHide"></param>
        public static void Restart(int? ypid, int ver, bool isHide)
        {
            Exit(ypid.GetValueOrDefault());
            ExePlayById(ypid.GetValueOrDefault(), ver, isHide);
        }

        /// <summary>
        /// 判断播放器是否为隐藏启动
        /// </summary>
        /// <param name="ypid"></param>
        /// <returns></returns>
        public static bool? IsHideStarted(int ypid)
        {
            if (!_PlayerProcessDictionary.ContainsKey(ypid))
            {
                return false;
            }
            return _PlayerProcessDictionary[ypid].StartInfo.WindowStyle == ProcessWindowStyle.Hidden;
        }

        /// <summary>
        /// 检查信任文件存在, 不存在则创建
        /// </summary>
        public static void WriteTrustFile()
        {
            if (_CheckedTrustFile)
            {
                return;
            }
            //检查信任文件是否存在
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var trustFolderPath = Path.Combine(appDataPath, mFlashSecurityPath);
            var trustFilePath = Path.Combine(trustFolderPath, mFlashTrustFileName);
            var trustContent = Path.Combine(Environment.CurrentDirectory, "Assets", "flash");
            if (File.Exists(trustFilePath))
            {
                var content = File.ReadAllText(trustFilePath);
                if (string.Equals(trustContent, content))
                {
                    _CheckedTrustFile = true;
                    return;
                }
            }
            FileUtil.SaveStringToFile(trustContent, trustFolderPath, mFlashTrustFileName);
            _CheckedTrustFile = true;
        }
    }
}