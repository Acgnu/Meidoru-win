using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AcgnuX.Source.Taskx
{
    /// <summary>
    /// 代理IP抓取工厂
    /// 抓取代理参考正则
    /// (\d+\.\d+\.\d+\.\d+)</td>\s+<td>(\d+)</td>
    /// </summary>
    class ProxyFactory
    {
        //IP池数量变化事件
        public static event Bussiness.Common.ProxyPoolCountChangeHandler mProxyPoolCountChangeHandler;
        //IP池存储列表
        //private static ConcurrentQueue<string> mProxyIpPool = new ConcurrentQueue<string>();
        //当前代理IP数量
        private static int ProxyCount = 0;
        public static int GetProxyCount => ProxyCount;
        //用于代理连接检测的地址
        private static readonly string PROXY_TEST_URL = @"http://www.77music.com/flash_get_yp_info.php";
        //测试的期望响应结果
        private static readonly string PROXY_TEST_RESPONSE = "验证错误！";
        //抓取IP事件间隔
        private static readonly int mCrawlIPPeriod = 1000 * 60 * 60 * 3;
        //测试代理事件间隔
        private static readonly int mTestProxyPeriod = 1000 * 60 * 30;
        //抓取IP的定时任务
        private static Timer mCrawlIPTask;
        //测试代理IP有效性的定时任务
        private static Timer mCheckProxyTask;
        //标识最新抓取IP任务的ID, ID不一致则终止旧线程
        private static int mTaskId = 0;

        /// <summary>
        /// 读取爬取规则
        /// </summary>
        private static List<CrawlRule> GetCrawlRules()
        {
            var crawlRules = new List<CrawlRule>();
            var dataSet = SQLite.SqlTable("SELECT url, partten, name, max_page FROM crawl_rules WHERE enable = 1");
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
        /// 初始化 IP代理任务
        /// </summary>
        public static void InitProxyFactoryTask()
        {
            ProxyCount = GetAllProxyFromDB().Count();
            //抓取IP定时任务
            mCrawlIPTask = new Timer(new TimerCallback(StartCrawlIP), null, 0, mCrawlIPPeriod);
            //检测IP定时任务
            mCheckProxyTask = new Timer(new TimerCallback(CheckProxy), null, 0, mTestProxyPeriod);
        }

        /// <summary>
        /// 重新执行IP代理抓取任务
        /// </summary>
        public static void RestartCrawlIPTask()
        {
            mCrawlIPTask.Change(0, mCrawlIPPeriod);
            mCheckProxyTask.Change(0, mTestProxyPeriod);
        }

        /// <summary>
        /// 爬取IP
        /// </summary>
        private static void StartCrawlIP(object state)
        {
            mTaskId++;
            var taskName = "\n[ 抓取IP任务 ]";
            var crawRules = GetCrawlRules();
            //遍历所有规则
            crawRules.ForEach(item =>
            {
                //对每一条规则单开一个线程
                Task.Run(() => {
                    int threadTaskId = mTaskId;
                    //设置爬取的页数, 以第一页为当前页
                    for (var currentPage = 1; currentPage <= item.MaxPage; currentPage++)
                    {
                        if (threadTaskId != mTaskId) return;
                        var curTaskDesc = string.Format("{0} 规则 -> {1} ", taskName, item.Name);
                        //请求目标地址, 获取目标地址HTML
                        var crawlResult = RequestUtil.CrawlContentFromWebsit(string.Format(item.Url, currentPage), null);
                        if (!crawlResult.success)
                        {
                            Console.WriteLine(curTaskDesc + "页面抓取失败");
                            continue;
                        }
                        //设置匹配规则
                        Match mstr = Regex.Match(crawlResult.data, item.Partten);
                        Console.WriteLine(curTaskDesc + (mstr.Success ? "匹配成功" : "匹配失败"));
                        //开始逐行爬取IP
                        while (mstr.Success)
                        {
                            if (threadTaskId != mTaskId) return;
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
        /// 校验代理地址是否有效
        /// </summary>
        /// <param name="proxyAddress"></param>
        /// <returns></returns>
        private static bool IsProxyValid(string proxyAddress)
        {
            var crawlResult = RequestUtil.CrawlContentFromWebsit(PROXY_TEST_URL, proxyAddress, 5000);
            return PROXY_TEST_RESPONSE.Equals(crawlResult.data) ? true : false;
        }

        /// <summary>
        /// 每隔一段时间检测所有代理有效性
        /// </summary>
        private static void CheckProxy(object state)
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
        /// 从代理IP池中随机获取一个IP地址
        /// </summary>
        /// <returns></returns>
        //public static string GetRandProxy()
        //{
        //    if (mProxyIpPool.IsEmpty) return null;
        //    var index = mProxyIpPool.Count - 1 >= 0 ? mProxyIpPool.Count - 1 : 0;
        //    return mProxyIpPool.ElementAt(new Random().Next(index));
        //}

        /// <summary>
        /// 获取IP池里的第一个代理IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetFirstProxy()
        {
            //if (mProxyIpPool.IsEmpty) return null;
            //return mProxyIpPool.ElementAt(0);
            return SQLite.sqlone("SELECT address FROM proxy_address ORDER BY addtime LIMIT 1");
        }

        /// <summary>
        /// 从代理池移除IP
        /// </summary>
        /// <param name="proxyAddress">需要移除的IP地址</param>
        /// <param name="requeeTime">重新放入IP池的等待时间 (毫秒), 0 = 抛弃</param>
        public static void RemoveProxy(string proxyAddress, int requeeTime)
        {
            if (SQLite.ExecuteNonQuery("DELETE from proxy_address WHERE address = @address",
                new SQLiteParameter[] { new SQLiteParameter("@address", proxyAddress)}) > 0)
            {
                Interlocked.Decrement(ref ProxyCount);
                mProxyPoolCountChangeHandler?.Invoke(ProxyCount);
            }
            if (requeeTime ==  0)
            {
                return;
            }
            Task.Run(() =>
            {
                Thread.Sleep(requeeTime);
                //AddToPoolNotExsits(proxyAddress);
                SaveProxyToDB(proxyAddress);
            });
        }

        /// <summary>
        /// 保存一条代理到数据库
        /// </summary>
        /// <param name="proxyAddress"></param>
        private static void SaveProxyToDB(string proxyAddress)
        {
            if (SQLite.ExecuteNonQuery("INSERT OR IGNORE INTO proxy_address(address, addtime) VALUES (@address, datetime('now', 'localtime'))",
                new SQLiteParameter[] { new SQLiteParameter("@address", proxyAddress) }) > 0)
            {
                Interlocked.Increment(ref ProxyCount);
                mProxyPoolCountChangeHandler?.Invoke(ProxyCount);
            }
        }

        /// <summary>
        /// 如果代理池中没有, 则添加
        /// </summary>
        /// <param name="proxyAddress"></param>
        //private static void AddToPoolNotExsits(string proxyAddress)
        //{
        //    if(!mProxyIpPool.Contains(proxyAddress))
        //    {
        //        mProxyIpPool.Enqueue(proxyAddress);
        //        mProxyPoolCountChangeHandler?.Invoke(mProxyIpPool.Count);
        //    }
        //}

        /// <summary>
        /// 查询数据库中所有的代理
        /// </summary>
        /// <returns></returns>
        private static List<ProxyAddress> GetAllProxyFromDB()
        {
            var proxyList = new List<ProxyAddress>();
            var dataSet = SQLite.SqlTable("SELECT address, addtime FROM proxy_address ORDER BY addtime");
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
    }
}
