using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using System.Linq;
using System.Timers;

namespace AcgnuX.Source.Taskx
{
    /// <summary>
    /// 代理IP查询
    /// </summary>
    class ProxyFactoryV2
    {
        //IP池数量变化事件
        public static event Bussiness.Common.ProxyPoolCountChangeHandler mProxyPoolCountChangeHandler;
        //当前代理IP数量
        private static int ProxyCount = 0;
        public static int GetProxyCount => ProxyCount;
        private static Timer mProxyCounter;


        /// <summary>
        /// 初始化 IP代理任务
        /// </summary>
        public static void InitProxyFactoryV2Task()
        {
            mProxyCounter = new Timer();
            mProxyCounter.Interval = 1000; // 1 seconds
            mProxyCounter.Elapsed += new ElapsedEventHandler(OnTimer);
            mProxyCounter.Start();
        }

        /// <summary>
        /// 重新执行IP代理抓取任务
        /// </summary>
        public static void RestartCrawlIPTask()
        {
            ServiceUtil.Restart(ApplicationConstant.CRAWL_IP_SERVICE_NAME, new string[] { ConfigUtil.Instance.DbFilePath });
        }

        /// <summary>
        /// 每隔1秒查询代理数量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnTimer(object sender, ElapsedEventArgs args)
        {
            ProxyCount = ProxyFactory.GetAllProxyFromDB().Count();
            mProxyPoolCountChangeHandler?.Invoke(ProxyCount);
        }
    }
}
