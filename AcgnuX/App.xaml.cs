using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace AcgnuX
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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
