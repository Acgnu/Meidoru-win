using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;
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
        /// 开始 IP池查询任务
        /// </summary>
        public static async Task StartTrack()
        {
            if (null == mProxyCounter)
            {
                await Task.Run(() =>
                {
                    OnTimer(null, null);
                    mProxyCounter = new Timer
                    {
                        Interval = 1000 // 1 seconds
                    };
                    mProxyCounter.Elapsed += new ElapsedEventHandler(OnTimer);
                    mProxyCounter.Start();
                });
            }
            else
            {
                mProxyCounter.Start();
            }
        }

        /// <summary>
        /// 停止IP池查询任务
        /// </summary>
        public static void StopTrack()
        {
            mProxyCounter.Stop();
        }

        /// <summary>
        /// 重新执行IP代理抓取任务
        /// </summary>
        public static void RestartCrawlIPService()
        {
            ServiceUtil.Restart(ApplicationConstant.CRAWL_IP_SERVICE_NAME, new string[] { Settings.Default.DBFilePath });
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
