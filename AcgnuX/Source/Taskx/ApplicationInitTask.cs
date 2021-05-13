using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Taskx.Http;
using AcgnuX.Source.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Taskx
{
    /// <summary>
    /// 系统任务 , 启动时触发
    /// </summary>
    class ApplicationInitTask
    {
        public void Init()
        {
            //检查数据库文件是否存在
            if (!File.Exists(SQLite.dbPath + SQLite.dbfile))
            {
                SQLite.CreateDBFile(SQLite.dbfile);
            }
            //创建必须的表
            var initSQL = FileUtil.GetApplicationResourceAsString(SQLite.dbPath + SQLite.initfile);
            SQLite.ExecuteNonQuery(initSQL);

            //检查核心数据是否存在

            //初始化设置
            AcgnuConfig.Init();

            //执行代理IP爬取任务
            ProxyFactory.InitProxyFactoryTask();

            Task.Run(() =>
            {
            });
        }
    }
}
