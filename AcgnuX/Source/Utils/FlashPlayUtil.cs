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
    class FlashPlayUtil
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
        private static Process mFlashProcess;

        /// <summary>
        /// 使用外部flashplayer.exe播放swf文件
        /// </summary>
        /// <param name="command">
        /// <param name="isHide">true 最小化启动</param>
        /// 命令行
        /// </param>
        public static void ExePlay(string command, bool isHide)
        {
            //检查信任文件是否存在
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!File.Exists(appDataPath + mFlashSecurityPath + mFlashTrustFileName))
            {
                FileUtil.SaveStringToFile(Environment.CurrentDirectory + @"\Assets\flash", appDataPath + mFlashSecurityPath, mFlashTrustFileName);
            }
            //Exit();
            //mFlashProcess = new Process();
            //mFlashProcess.StartInfo.FileName = Environment.CurrentDirectory + @"\Assets\flash\flashplayer.exe";
            //mFlashProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory + "/maininteractive/ccv/";//程序闪退问题，要强行指定工作目录
            //mFlashProcess.StartInfo.UseShellExecute = true;//关键代码
            //mFlashProcess.StartInfo.WindowStyle = isHide ? ProcessWindowStyle.isHide : ProcessWindowStyle.Normal;//关键代码
            //mFlashProcess.StartInfo.Arguments = command;
            //mFlashProcess.Start();
            mFlashProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = Environment.CurrentDirectory + @"\Assets\flash\flashplayer.exe",
                WindowStyle = isHide ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,//关键代码
                Arguments = command,
            });
            //mFlashProcess = Process.Start(Environment.CurrentDirectory + @"\Assets\flash\flashplayer.exe", command);
        }

        /// <summary>
        /// 根据乐谱id打开弹琴吧播放器
        /// </summary>
        /// <param name="ypid"></param>
        public static void ExePlayById(int ypid, bool isHide)
        {
            ExePlay(Environment.CurrentDirectory + @"\Assets\flash\fuckTan8\Main.swf?id=" + ypid, isHide);
        }

        /// <summary>
        /// 退出flash
        /// </summary>
        public static void Exit()
        {
            //检查进程是否存在
            if (null != mFlashProcess && !mFlashProcess.HasExited)
            {
                mFlashProcess.CloseMainWindow();
                //mFlashProcess.Close();
                if (!mFlashProcess.HasExited)
                {
                    mFlashProcess.Kill();
                }
            }
        }

        /// <summary>
        /// 重启Flash播放器
        /// </summary>
        /// <param name="ypid"></param>
        /// <param name="isHide"></param>
        public static void Restart(int? ypid, bool isHide)
        {
            Exit();
            ExePlayById(ypid.GetValueOrDefault(), isHide);
        }
    }
}