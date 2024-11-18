using AcgnuX.Properties;
using SharedLib.Utils;
using System.Net.Http;
using System.Timers;

namespace AcgnuX.Source.Taskx
{
    /// <summary>
    /// 代理IP查询
    /// </summary>
    public class ProxyFactoryV2
    {
        //IP池数量变化事件
        public event Action<int> mProxyPoolCountChangeHandler;
        //当前代理IP数量
        private int ProxyCount = 0;
        public int GetProxyCount => ProxyCount;
        private System.Timers.Timer mProxyCounter;


        /// <summary>
        /// 开始 IP池查询任务
        /// </summary>
        public async Task StartTrack()
        {
            if (null == mProxyCounter)
            {
                await Task.Run(() =>
                {
                    OnTimer(null, null);
                    mProxyCounter = new System.Timers.Timer
                    {
                        Interval = 3000 // 3 seconds
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
        public void StopTrack()
        {
            mProxyCounter.Stop();
        }

        /// <summary>
        /// 每隔1秒查询代理数量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnTimer(object sender, ElapsedEventArgs args)
        {
            var proxyAddress = Settings.Default.HttpProxyAddress;
            if (string.IsNullOrEmpty(proxyAddress)) return;

            var num = await RequestUtil.TaskFormRequestAsync(proxyAddress + "/ip-nums", null, HttpMethod.Get);
            if (!DataUtil.IsNum(num)) return;

            ProxyCount = Convert.ToInt32(num);
            mProxyPoolCountChangeHandler?.Invoke(ProxyCount);
        }
    }
}
