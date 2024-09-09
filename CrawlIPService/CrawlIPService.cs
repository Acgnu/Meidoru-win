using CrawlIPService.Properties;
using SharedLib.Data;
using SharedLib.Model;
using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        private readonly CrawlRuleRepo _CrawlRuleRepo = new CrawlRuleRepo();
        private readonly ProxyAddressRepo _ProxyAddressRepo = new ProxyAddressRepo();

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
            if (args.Length > 0)
            {
                mEventLog.WriteEntry(string.Format("启动参数 {0}", args[0]));
            }
            var dbFilePath = (null == args || args.Length == 0) ? Settings.Default.DBFilePath : args[0];
            if (string.IsNullOrEmpty(dbFilePath))
            {
                mEventLog.WriteEntry("没有指定数据库路径, 无法执行");
                OnStop();
                return;
            }
            mEventLog.WriteEntry(string.Format("执行参数: {0}", dbFilePath));
            Settings.Default.Save();
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
            var data = _CrawlRuleRepo.FindCrawlRuleAsync().Result;
            data.RemoveAll(rule => rule.Enable.Equals(Convert.ToByte(0)));
            return data;
        }

        /// <summary>
        /// 每隔一段时间检测所有代理有效性
        /// </summary>
        private void CheckProxy(object state)
        {
            //用于存储已经失效的代理
            var allProxy = _ProxyAddressRepo.GetAllProxyFromDB();
            foreach (var proxy in allProxy)
            {
                //如果无法连通则放入失效列表, 检测完后删除
                if (!IsProxyValid(proxy.Address)) _ProxyAddressRepo.RemoveProxy(proxy.Address, 0);
            }
        }

        /// <summary>
        /// 校验代理地址是否有效
        /// </summary>
        /// <param name="proxyAddress"></param>
        /// <returns></returns>
        private bool IsProxyValid(string proxyAddress)
        {
            if (string.IsNullOrEmpty(proxyAddress))
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
                Task.Run(() =>
                {
                    //设置爬取的页数, 以第一页为当前页
                    for (var currentPage = 1; currentPage <= item.MaxPage; currentPage++)
                    {
                        var curTaskDesc = string.Format("{0} 规则 -> {1} ", taskName, item.Name);
                        //请求目标地址, 获取目标地址HTML
                        var crawlResult = RequestUtil.CrawlContentFromWebsit(string.Format(item.Url, currentPage), null);
                        if (!crawlResult.success)
                        {
                            mEventLog.WriteEntry(curTaskDesc + (curTaskDesc + "页面抓取失败"));
                            _CrawlRuleRepo.UpdateExceptionDesc(item.Id, "页面抓取失败:" + crawlResult.message);
                            continue;
                        }
                        //设置匹配规则
                        Match mstr = Regex.Match(crawlResult.data, item.Partten);
                        if (!mstr.Success)
                        {
                            _CrawlRuleRepo.UpdateExceptionDesc(item.Id, "规则匹配失败");
                        }
                        mEventLog.WriteEntry(curTaskDesc + (mstr.Success ? "匹配成功" : "匹配失败"));
                        //开始逐行爬取IP
                        while (mstr.Success)
                        {
                            var proxyAddress = mstr.Groups[1].Value + ":" + mstr.Groups[2].Value;
                            mstr = mstr.NextMatch();
                            if (IsProxyValid(proxyAddress))
                            {
                                _ProxyAddressRepo.SaveProxyToDB(proxyAddress);
                            }
                        }
                        if (!"正常".Equals(item.ExceptionDesc))
                        {
                            _CrawlRuleRepo.UpdateExceptionDesc(item.Id, "正常");
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
