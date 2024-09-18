using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProxyTestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            //var proxyProviderUrl = string.Format(@"https://www.kuaidaili.com/free/inha/{0}/", 1);
            var proxyProviderUrl = @"http://www.ip3366.net/";
            //var partten = "(\\d+\\.\\d+\\.\\d+\\.\\d+)\",.*?\"port\":\\s\"(\\d+)\"";
            var partten = "<td>(\\d+\\.\\d+\\.\\d+\\.\\d+)</td>\\s+<td>(\\d+)</td>";
            Console.WriteLine(partten);

            var proxyPage = RequestUtil.CrawlContentFromWebsit(proxyProviderUrl, null);

            if (!proxyPage.success)
            {
                Console.WriteLine(proxyPage.message);
                Console.ReadKey();
                return;
            }

            MatchCollection matches = Regex.Matches(proxyPage.data, partten);
            if (matches.Count == 0)
            {
                Console.WriteLine("规则匹配失败");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("代理数量:{0}", matches.Count);
            foreach (Match mstr in matches)
            {
                var proxyAddress = mstr.Groups[1].Value + ":" + mstr.Groups[2].Value;
                //mstr = mstr.NextMatch();
                Console.Write("正在测试 {0}", proxyAddress);
                var crawlResult = RequestUtil.CrawlContentFromWebsit(@"http://www.77music.com/flash_get_yp_info.php", proxyAddress, 5000);
                Console.WriteLine(" 抓取结果:{0}", crawlResult.data);
            }

            Console.WriteLine("抓取结束, 按任意键退出");
            Console.ReadKey();
        }
    }
}
