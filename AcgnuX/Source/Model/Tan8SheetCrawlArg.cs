using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// tan8乐谱下载参数
    /// </summary>
    public class Tan8SheetCrawlArg
    {
        //乐谱ID
        public int? Ypid { get; set; }
        //乐谱名称
        public string Name { get; set; }
        //是否自动下载
        public bool AutoDownload { get; set; }
        //是否使用代理
        public bool UseProxy { get; set; }
        //v2版的乐谱下载地址
        public string SheetUrl { get; set; }
        //版本1-flash, 2-exe, 3-APP
        public byte Ver { get; set; }
        //是否为队列下载
        public bool IsQueueTask { get; set; } = false;
        //下载任务数
        public int TaskNum { get; set; }
    }
}
