﻿namespace SharedLib.Model
{
    /// <summary>
    /// 网页IP池爬取规则
    /// </summary>
    public class CrawlRule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //目标地址
        public string Url { get; set; }
        //IP匹配表达式
        public string Partten { get; set; }
        //最大爬取页
        public int MaxPage { get; set; }
        //抓取的错误描述
        public string ExceptionDesc { get; set; }
        public byte Enable { get; set; }
    }
}
