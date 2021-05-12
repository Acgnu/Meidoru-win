using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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
        private static ConcurrentQueue<string> mProxyIpPool = new ConcurrentQueue<string>();
        //用于代理连接检测的地址
        private static readonly string PROXY_TEST_URL = @"http://www.77music.com/flash_get_yp_info.php";
        //测试的期望响应结果
        private static readonly string PROXY_TEST_RESPONSE = "验证错误！";
        //抓取IP事件间隔
        private static readonly int mCrawlIPPeriod = 1000 * 60 * 60 * 12;
        //测试代理事件间隔
        private static readonly int mTestProxyPeriod = 1000 * 60 * 30;
        //抓取IP的定时任务
        private static readonly System.Threading.Timer mCrawlIPTask = new System.Threading.Timer(new TimerCallback(StartCrawlIP), null, 0, mCrawlIPPeriod);
        //测试代理IP有效性的定时任务
        private static readonly System.Threading.Timer mTestProxyTask = new System.Threading.Timer(new TimerCallback(TestProxy), null, 0, mTestProxyPeriod);

        /// <summary>
        /// 读取爬取规则
        /// </summary>
        private static List<CrawlRule> GetCrawlRules()
        {
            var crawlRules = new List<CrawlRule>();
            var dataSet = SQLite.SqlTable("SELECT url, partten, max_page FROM crawl_rules WHERE enable = 1");
            if (null == dataSet) return crawlRules;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                crawlRules.Add(new CrawlRule()
                {
                    Url = Convert.ToString(dataRow["url"]),
                    Partten = Convert.ToString(dataRow["partten"]),
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
            //从数据库恢复代理
            RestoreProxyFromDB();
        }

        /// <summary>
        /// 重新执行IP代理抓取任务
        /// </summary>
        public static void RestartCrawlIPTask()
        {
            mCrawlIPTask.Change(0, mCrawlIPPeriod);
            mTestProxyTask.Change(0, mTestProxyPeriod);
        }

        /// <summary>
        /// 爬取IP
        /// </summary>
        private static void StartCrawlIP(object state)
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
                        Console.WriteLine(taskName + "开始抓取IP代理, 目标 -> " + item.Url);
                        //请求目标地址, 获取目标地址HTML
                        var crawlResult = RequestUtil.CrawlContentFromWebsit(string.Format(item.Url, currentPage), null);
                        if (!crawlResult.success)
                        {
                            Console.WriteLine(taskName + "页面抓取失败" + item.Url);
                            continue;
                        }
                        //设置匹配规则
                        Match mstr = Regex.Match(crawlResult.data, item.Partten);
                        Console.WriteLine(taskName + "匹配结果 -> " + mstr.Success);
                        //开始逐行爬取IP
                        while (mstr.Success)
                        {
                            var proxyAddress = mstr.Groups[1].Value + ":" + mstr.Groups[2].Value;
                            mstr = mstr.NextMatch();
                            //如果IP池已包含则匹配下一条
                            if (mProxyIpPool.Contains(proxyAddress)) continue;
                            //只有代理有效才加入代理池
                            if (IsProxyValid(proxyAddress))
                            {
                                AddToPoolNotExsits(proxyAddress);
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
            var crawlResult = RequestUtil.CrawlContentFromWebsit(PROXY_TEST_URL, proxyAddress, 3000);
            return PROXY_TEST_RESPONSE.Equals(crawlResult.data) ? true : false;
        }

        /// <summary>
        /// 每隔一段时间检测所有代理有效性
        /// </summary>
        private static void TestProxy(object state)
        {
            //用于存储已经失效的代理
            var invalidProxyList = new List<string>();
            //爬取完成后对代理进行检测
            for (var i = 0; i < mProxyIpPool.Count; i++)
            {
                var proxyAddress = mProxyIpPool.ElementAt(i);
                //如果无法连通则放入失效列表, 检测完后删除
                if (!IsProxyValid(proxyAddress)) invalidProxyList.Add(proxyAddress);
            }
            //移除已失效的代理
            invalidProxyList.ForEach(item => RemoveProxy(item, true));
        }

        /// <summary>
        /// 从代理IP池中随机获取一个IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetRandProxy()
        {
            if (mProxyIpPool.IsEmpty) return null;
            var index = mProxyIpPool.Count - 1 >= 0 ? mProxyIpPool.Count - 1 : 0;
            return mProxyIpPool.ElementAt(new Random().Next(index));
        }

        /// <summary>
        /// 返回IP池数量
        /// </summary>
        /// <returns></returns>
        public static int GetProxyCount()
        {
            return mProxyIpPool.Count();
        }

        /// <summary>
        /// 获取IP池里的第一个代理IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetFirstProxy()
        {
            if (mProxyIpPool.IsEmpty) return null;
            return mProxyIpPool.ElementAt(0);
        }

        /// <summary>
        /// 从代理池移除IP
        /// </summary>
        /// <param name="proxyAddress">需要移除的IP地址</param>
        /// <param name="requeeTime">重新放入IP池的等待时间 (毫秒), 0 = 抛弃</param>
        public static void RemoveTemporary(string proxyAddress, int requeeTime)
        {
            RemoveProxy(proxyAddress, 0 == requeeTime);
            if(requeeTime ==  0)
            {
                return;
            }
            Task.Run(() =>
            {
                Thread.Sleep(requeeTime);
                AddToPoolNotExsits(proxyAddress);
            });
        }

        /// <summary>
        /// 尝试移除队列中的IP
        /// </summary>
        /// <param name="proxyAddress"></param>
        /// <param name="delDb">是否也从db删除</param>
        private static void RemoveProxy(string proxyAddress, bool delDb)
        {
            if(delDb)
            {
                //从数据中移除
                SQLite.ExecuteNonQuery(string.Format("DELETE from proxy_address WHERE address = '{0}'", proxyAddress));
            }
            //从队列中移除
            if(mProxyIpPool.Contains(proxyAddress))
            {
                mProxyIpPool.TryDequeue(out proxyAddress);
                mProxyPoolCountChangeHandler?.Invoke(mProxyIpPool.Count);
            }
        }

        /// <summary>
        /// 保存一条代理到数据库
        /// </summary>
        /// <param name="proxyAddress"></param>
        private static void SaveProxyToDB(string proxyAddress)
        {
            SQLite.ExecuteNonQuery(string.Format("INSERT INTO proxy_address(address, addtime) VALUES ('{0}', datetime('now', 'localtime'))", proxyAddress));
        }

        /// <summary>
        /// 如果代理池中没有, 则添加
        /// </summary>
        /// <param name="proxyAddress"></param>
        private static void AddToPoolNotExsits(string proxyAddress)
        {
            if(!mProxyIpPool.Contains(proxyAddress))
            {
                mProxyIpPool.Enqueue(proxyAddress);
                mProxyPoolCountChangeHandler?.Invoke(mProxyIpPool.Count);
            }
        }

        /// <summary>
        /// 清空今天之前的代理
        /// </summary>
        //private static void ClearProxyBeforeToday()
        //{
        //    SQLite.ExecuteNonQuery("DELETE FROM proxy_address WHERE addtime < date('now')");
        //}

        /// <summary>
        /// 将上一次保存的有效代理池初始化
        /// </summary>
        private static void RestoreProxyFromDB()
        {
            //var columnData = SQLite.sqlcolumn("SELECT address FROM proxy_address");
            var dataSet = SQLite.SqlTable("SELECT address, addtime FROM proxy_address ORDER BY addtime DESC");
            var lateProxyList = new List<string>();
            if (null == dataSet) return;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                var proxyAddTime = Convert.ToDateTime(dataRow["addtime"]);
                var proxyAddress = Convert.ToString(dataRow["address"]);
                //如果代理添加的时间超过24小时, 需要先测试能否使用
                if (TimeUtil.PassedTimeMillis(DateTime.Now, proxyAddTime) > 1000 * 60 * 60 * 24)
                {
                    lateProxyList.Add(proxyAddress);
                }
                else
                {
                    //不超过24小时的直接加入代理池
                    AddToPoolNotExsits(proxyAddress);
                }
            }
            if(lateProxyList.Count > 0)
            {
                Task.Run(() =>
                {
                    foreach (var proxyAddress in lateProxyList)
                    {
                        if (IsProxyValid(proxyAddress))
                        {
                            AddToPoolNotExsits(proxyAddress);
                        }
                        else
                        {
                            //无效直接删除
                            RemoveProxy(proxyAddress, true);
                        }
                    }
                });
            }
        }
    }
}
