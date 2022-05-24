using AcgnuX.Source.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// flash 播放工具
    /// </summary>
    class Tan8PlayUtil
    {
        /// <summary>
        /// FlashPlayer.exe 执行时信任的swf配置目录
        /// 修改过的flash请求localhost执行会受到flashplayer的安全限制
        /// 此处创建白名单文件解除限制
        /// </summary>
        private static readonly string mFlashSecurityPath = @"\Macromedia\Flash Player\#Security\FlashPlayerTrust\";
        /// <summary>
        /// 针对此应用程序的配置文件
        /// </summary>
        private static readonly string mFlashTrustFileName = "acgnux_flash.cfg";
        /// <summary>
        /// flash exe线程
        /// </summary>
        private static Process mPlayerProcess;

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
            if(version == 1)
            {
                //检查信任文件是否存在
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!File.Exists(appDataPath + mFlashSecurityPath + mFlashTrustFileName))
                {
                    WriteTrustFile();
                }
                mPlayerProcess = Process.Start(new ProcessStartInfo()
                {
                    FileName = Environment.CurrentDirectory + @"\Assets\flash\flashplayer.exe",
                    WindowStyle = isHide ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,//关键代码
                    Arguments = Environment.CurrentDirectory + @"\Assets\flash\fuckTan8\Main.swf?id=" + ypid,
                });
            } 
            else if (version == 2)
            {
                mPlayerProcess = Process.Start(new ProcessStartInfo()
                {
                    FileName = Environment.CurrentDirectory + @"\Assets\exe\77player.exe",
                    WindowStyle = isHide ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,//关键代码
                    Arguments = string.Format("file=http://www.fucktan8.com/open_yp.php?ypid={0}&uid=999999999", ypid)
                });
            }
            else
            {
                throw new Exception("不支持的播放器版本");
            }
        }

        /// <summary>
        /// 退出flash
        /// </summary>
        public static void Exit()
        {
            //检查进程是否存在
            if (null != mPlayerProcess && !mPlayerProcess.HasExited)
            {
                mPlayerProcess.CloseMainWindow();
                //mFlashProcess.Close();
                if (!mPlayerProcess.HasExited)
                {
                    mPlayerProcess.Kill();
                }
            }
        }

        /// <summary>
        /// 重启Flash播放器
        /// </summary>
        /// <param name="ypid"></param>
        /// <param name="ver"></param>
        /// <param name="isHide"></param>
        public static void Restart(int? ypid, int ver, bool isHide)
        {
            Exit();
            ExePlayById(ypid.GetValueOrDefault(), ver, isHide);
        }

        /// <summary>
        /// 检查信任文件存在, 不存在则创建
        /// </summary>
        public static void WriteTrustFile()
        {
            //检查信任文件是否存在
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            FileUtil.SaveStringToFile(Environment.CurrentDirectory + @"\Assets\flash", appDataPath + mFlashSecurityPath, mFlashTrustFileName);
        }
    }
}