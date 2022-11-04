using GalaSoft.MvvmLight;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 网页IP池爬取规则
    /// </summary>
    public class CrawlRule : ViewModelBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //目标地址
        public string Url { get; set; }
        //IP匹配表达式
        public string Partten { get; set; }
        //最大爬取页
        public int MaxPage { get; set; }
        public byte Enable { get; set; }
    }
}
