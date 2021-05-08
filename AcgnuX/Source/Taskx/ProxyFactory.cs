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

namespace AcgnuX.Source.Taskx
{
    /// <summary>
    /// 代理IP抓取工厂
    /// 抓取代理参考正则
    /// (\d+\.\d+\.\d+\.\d+)</td>\s+<td>(\d+)</td>
    /// </summary>
    class ProxyFactory
    {
        //IP池存储列表
        private static ConcurrentQueue<string> mProxyIpPool = new ConcurrentQueue<string>();
        //爬取规则列表
        private static List<CrawlRule> mRules = new List<CrawlRule>();
        //用于代理连接检测的地址
        private static readonly string PROXY_TEST_URL = @"http://www.77music.com/flash_get_yp_info.php";
        //测试的期望响应结果
        private static readonly string PROXY_TEST_RESPONSE = "验证错误！";

        /// <summary>
        /// 初始化爬取规则
        /// </summary>
        private static void InitCrawlRules()
        {
            if(mRules.Count == 0)
            {
                var dataSet = SQLite.SqlTable("SELECT url, partten, max_page FROM crawl_rules WHERE enable = 1");
                if (null == dataSet) return;
                foreach (DataRow dataRow in dataSet.Rows)
                {
                    mRules.Add(new CrawlRule()
                    {
                        Url = Convert.ToString(dataRow["url"]),
                        Partten = Convert.ToString(dataRow["partten"]),
                        MaxPage = Convert.ToInt32(dataRow["max_page"])
                    });
                }
            }
        }

        /// <summary>
        /// 爬取IP
        /// </summary>
        public static void StartCrawlIP()
        {
            Task.Run(() => {
                //先初始化规则
                InitCrawlRules();

                //清空数据空中过期的代理数据
                ClearProxyBeforeToday();

                //初始化代理池
                InitProxyPool();

                var taskName = "\n[ 抓取IP任务 ]";
                //遍历所有规则
                mRules.ForEach(item =>
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
                                    mProxyIpPool.Enqueue(proxyAddress);
                                    SaveProxyToDB(proxyAddress);
                                }
                            }
                        }
                    });
                });
            });
            //开启代理检测任务
            TestProxy(1000 * 60 * 30);
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
        private static void TestProxy(int delay)
        {
            Task.Run(() =>
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
                invalidProxyList.ForEach(e => RemoveProxy(e));
                Thread.Sleep(delay);
            });
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
        /// 一个代理IP在时候用需要15秒后才可继续使用
        /// </summary>
        /// <param name="proxyAddress"></param>
        /// <param name="downloadResult"></param>
        public static void RemoveTemporary(string proxyAddress, PianoScoreDownloadResult downloadResult)
        {
            RemoveProxy(proxyAddress);
            //代理到达当天访问限制则直接抛弃
            if (PianoScoreDownloadResult.VISTI_REACH_LIMIT == downloadResult) return;
            //如果代理仍然可用, 则15秒后重新添加到代理池
            Task.Run(() =>
            {
                Thread.Sleep(15 * 1000);
                //如果IP池已包含则匹配下一条
                mProxyIpPool.Enqueue(proxyAddress);
            });
        }

        /// <summary>
        /// 尝试移除队列中的IP
        /// </summary>
        /// <param name="proxy"></param>
        private static void RemoveProxy(string proxyAddress)
        {
            //从数据中移除
            SQLite.ExecuteNonQuery(string.Format("DELETE from proxy_address WHERE address = '{0}'", proxyAddress));
            //从队列中移除
            mProxyIpPool.TryDequeue(out proxyAddress);
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
        /// 清空今天之前的代理
        /// </summary>
        private static void ClearProxyBeforeToday()
        {
            SQLite.ExecuteNonQuery("DELETE FROM proxy_address WHERE addtime < date('now')");
        }

        /// <summary>
        /// 将上一次保存的有效代理池初始化
        /// </summary>
        private static void InitProxyPool()
        {
            var columnData = SQLite.sqlcolumn("SELECT address FROM proxy_address");
            if (null == columnData) return;
            columnData.ForEach(e => mProxyIpPool.Enqueue(e));
        }
    }
}
