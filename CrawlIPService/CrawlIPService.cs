using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CrawIPService
{
    /// <summary>
    /// 后台抓取网络IP的服务
    /// </summary>
    public partial class CrawlIPService : ServiceBase
    {
        private EventLog mEventLog;
        private string mEventSourceName = "AcgnuX_CrawlIPSource";
        private string mEventLogName = "AcgnuX_CrawlIPLog";

        //抓取IP的定时任务
        private System.Threading.Timer mCrawlIPTask;
        //测试代理IP有效性的定时任务
        private System.Threading.Timer mCheckProxyTask;
        //抓取IP时间间隔
        private readonly int mCrawlIPPeriod = 1000 * 60 * 60 * 3;      //3小时
        //测试代理时间间隔
        private readonly int mTestProxyPeriod = 1000 * 60 * 30;        //30分钟
        //用于代理连接检测的地址
        private readonly string PROXY_TEST_URL = @"http://www.77music.com/flash_get_yp_info.php";
        //测试的期望响应结果
        private readonly string PROXY_TEST_RESPONSE = "验证错误！";

        public CrawlIPService()
        {
            InitializeComponent();
            //添加自定义事件日志功能 到 Windows 服务——“事件查看器”中找到 mEventLogName
            mEventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(mEventSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    mEventSourceName, mEventLogName);
            }
            mEventLog.Source = mEventSourceName;
            mEventLog.Log = mEventLogName;
        }

        #region /* 设置服务状态 */
        // 使用平台调用声明 SetServiceStatus 函数
        //[DllImport("advapi32.dll", SetLastError = true)]
        //private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        // 声明 ServiceState 值和添加你将在平台调用中使用的状态结构
        //public enum ServiceState
        //{
        //    SERVICE_STOPPED = 0x00000001,
        //    SERVICE_START_PENDING = 0x00000002,
        //    SERVICE_STOP_PENDING = 0x00000003,
        //    SERVICE_RUNNING = 0x00000004,
        //    SERVICE_CONTINUE_PENDING = 0x00000005,
        //    SERVICE_PAUSE_PENDING = 0x00000006,
        //    SERVICE_PAUSED = 0x00000007,
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //public struct ServiceStatus
        //{
        //    public int dwServiceType;
        //    public ServiceState dwCurrentState;
        //    public int dwControlsAccepted;
        //    public int dwWin32ExitCode;
        //    public int dwServiceSpecificExitCode;
        //    public int dwCheckPoint;
        //    public int dwWaitHint;
        //};
        #endregion

        protected override void OnStart(string[] args)
        {
            // 将服务状态更新为“开始挂起”。
            //ServiceStatus serviceStatus = new ServiceStatus();
            //serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            //serviceStatus.dwWaitHint = 100000;
            //SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            //# 在服务操作开始后，OnStart 方法必须返回操作系统，以便不阻止系统。
            //  所以若要设置简单的轮询机制，设置一个每分钟触发一次的Timer计时器，以此可以进行监视服务。
            //# 当然也可以使用后台工作线程来运行任务，进行轮询。

            // 将服务状态更新为正在运行。
            //serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            //SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            //读取数据库
            var dbFilePath = (null == args || args.Length == 0) ? ConfigUtil.Instance.Load().DbFilePath : args[0];
            if(string.IsNullOrEmpty(dbFilePath))
            {
                OnStop();
                return;
            }
            ConfigUtil.Instance.Save(new Settings()
            {
                DbFilePath = dbFilePath
            });
            SQLite.SetDbFilePath(dbFilePath);
            //抓取IP定时任务
            mCrawlIPTask = new System.Threading.Timer(new System.Threading.TimerCallback(StartCrawlIP), null, 0, mCrawlIPPeriod);
            //检测IP定时任务
            mCheckProxyTask = new System.Threading.Timer(new System.Threading.TimerCallback(CheckProxy), null, 0, mTestProxyPeriod);
        }

        protected override void OnStop()
        {
            // 将服务状态更新为“停止挂起”。
            //ServiceStatus serviceStatus = new ServiceStatus();
            //serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            //serviceStatus.dwWaitHint = 100000;
            //SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // 在服务停止时向事件日志添加一个条目
            //mEventLog.WriteEntry("In OnStop.");

            // 将服务状态更新为已停止。
            //serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            //SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        /// <summary>
        /// 读取爬取规则
        /// </summary>
        private List<CrawlRule> GetCrawlRules()
        {
            var crawlRules = new List<CrawlRule>();
            var dataSet = SQLite.SqlTable("SELECT url, partten, name, max_page FROM crawl_rules WHERE enable = 1", null);
            if (null == dataSet) return crawlRules;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                crawlRules.Add(new CrawlRule()
                {
                    Url = Convert.ToString(dataRow["url"]),
                    Partten = Convert.ToString(dataRow["partten"]),
                    Name = Convert.ToString(dataRow["name"]),
                    MaxPage = Convert.ToInt32(dataRow["max_page"])
                });
            }
            return crawlRules;
        }

        /// <summary>
        /// 查询数据库中所有的代理
        /// </summary>
        /// <returns></returns>
        private List<ProxyAddress> GetAllProxyFromDB()
        {
            var proxyList = new List<ProxyAddress>();
            var dataSet = SQLite.SqlTable("SELECT address, addtime FROM proxy_address ORDER BY addtime", null);
            if (null == dataSet) return proxyList;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                proxyList.Add(new ProxyAddress()
                {
                    Address = Convert.ToString(dataRow["address"]),
                    Addtime = Convert.ToDateTime(dataRow["addtime"])
                });
            }
            return proxyList;
        }

        /// <summary>
        /// 每隔一段时间检测所有代理有效性
        /// </summary>
        private void CheckProxy(object state)
        {
            //用于存储已经失效的代理
            var allProxy = GetAllProxyFromDB();
            foreach (var proxy in allProxy)
            {
                //如果无法连通则放入失效列表, 检测完后删除
                if (!IsProxyValid(proxy.Address)) RemoveProxy(proxy.Address, 0);
            }
        }

        /// <summary>
        /// 从代理池移除IP
        /// </summary>
        /// <param name="proxyAddress">需要移除的IP地址</param>
        /// <param name="requeeTime">重新放入IP池的等待时间 (毫秒), 0 = 抛弃</param>
        public void RemoveProxy(string proxyAddress, int requeeTime)
        {
            SQLite.ExecuteNonQuery("DELETE from proxy_address WHERE address = @address",
                new List<SQLiteParameter> { new SQLiteParameter("@address", proxyAddress) });
            if (requeeTime == 0)
            {
                return;
            }
            Task.Run(() =>
            {
                Thread.Sleep(requeeTime);
                SaveProxyToDB(proxyAddress);
            });
        }

        /// <summary>
        /// 保存一条代理到数据库
        /// </summary>
        /// <param name="proxyAddress"></param>
        private void SaveProxyToDB(string proxyAddress)
        {
            SQLite.ExecuteNonQuery("INSERT OR IGNORE INTO proxy_address(address, addtime) VALUES (@address, datetime('now', 'localtime'))",
                new List<SQLiteParameter> { new SQLiteParameter("@address", proxyAddress) });
        }

        /// <summary>
        /// 校验代理地址是否有效
        /// </summary>
        /// <param name="proxyAddress"></param>
        /// <returns></returns>
        private bool IsProxyValid(string proxyAddress)
        {
            if(string.IsNullOrEmpty(proxyAddress))
            {
                return false;
            }
            var crawlResult = RequestUtil.CrawlContentFromWebsit(PROXY_TEST_URL, proxyAddress, 5000);
            return PROXY_TEST_RESPONSE.Equals(crawlResult.data) ? true : false;
        }

        /// <summary>
        /// 爬取IP
        /// </summary>
        private void StartCrawlIP(object state)
        {
            var taskName = "\n[ 抓取IP任务 ]";
            var crawRules = GetCrawlRules();
            //遍历所有规则
            crawRules.ForEach(item =>
            {
                //对每一条规则单开一个线程
                Task.Run(() => {
                    //设置爬取的页数, 以第一页为当前页
                    for (var currentPage = 1; currentPage <= item.MaxPage; currentPage++)
                    {
                        var curTaskDesc = string.Format("{0} 规则 -> {1} ", taskName, item.Name);
                        //请求目标地址, 获取目标地址HTML
                        var crawlResult = RequestUtil.CrawlContentFromWebsit(string.Format(item.Url, currentPage), null);
                        if (!crawlResult.success)
                        {
                            mEventLog.WriteEntry(curTaskDesc + (curTaskDesc + "页面抓取失败"));
                            continue;
                        }
                        //设置匹配规则
                        Match mstr = Regex.Match(crawlResult.data, item.Partten);
                        mEventLog.WriteEntry(curTaskDesc + (mstr.Success ? "匹配成功" : "匹配失败"));
                        //开始逐行爬取IP
                        while (mstr.Success)
                        {
                            var proxyAddress = mstr.Groups[1].Value + ":" + mstr.Groups[2].Value;
                            mstr = mstr.NextMatch();
                            if (IsProxyValid(proxyAddress))
                            {
                                SaveProxyToDB(proxyAddress);
                            }
                        }
                    }
                });
            });
        }

        /// <summary>
        /// 定义服务的OnContinue操作
        /// </summary>
        protected override void OnContinue()
        {
            mEventLog.WriteEntry("In OnContinue.");
        }

        /// <summary>
        /// 定义服务的OnPause操作
        /// </summary>
        protected override void OnPause()
        {
            mEventLog.WriteEntry("In OnPause.");
        }

        /// <summary>
        /// 定义服务的OnShutdown操作
        /// </summary>
        protected override void OnShutdown()
        {
            mEventLog.WriteEntry("In OnShutdown .");
        }
    }
}
