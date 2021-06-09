using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace AcgnuX
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //必须持有此对象作为成员变量, 才能生效
        private Mutex mMutex;

        /// <summary>
        /// 显示已运行的程序
        /// </summary>
        /// <param name="instance"></param>
        public static void HandleRunningInstance(Process instance)
        {
            ShowWindowAsync(instance.MainWindowHandle, 1); //显示
            SetForegroundWindow(instance.MainWindowHandle);            //放到前端
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //检测是否单例启动
            mMutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out bool createdNew);
            if (!createdNew)
            {
                var psArr = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                foreach (var ps in psArr) 
                {
                    if(ps.Id != Process.GetCurrentProcess().Id)
                    {
                        //已启动则展示之前启动的, 并关闭此实例
                        HandleRunningInstance(ps);
                        Shutdown();
                        return;
                    }
                }
            }

            //检查数据库文件是否存在
            if (!File.Exists(SQLite.dbPath + SQLite.dbfile))
            {
                SQLite.CreateDBFile(SQLite.dbfile);
            }
            //创建必须的表
            var initSQL = FileUtil.GetApplicationResourceAsString(SQLite.dbPath + SQLite.initfile);
            SQLite.ExecuteNonQuery(initSQL, null);

            //初始化设置
            ConfigUtil.Instance.Load();

            //执行代理IP爬取任务
            ProxyFactory.InitProxyFactoryTask();
        }
    }
}
