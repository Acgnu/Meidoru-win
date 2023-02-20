using AcgnuX.Source.Model;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 抓取规则视图模型
    /// </summary>
    public class CrawlRuleViewModel : ViewModelBase
    {
        public int Id { get; set; }
        private string _name;
        public string Name { get => _name; set { _name = value; RaisePropertyChanged(); } }
        //目标地址
        private string _url;
        public string Url { get => _url; set { _url = value; RaisePropertyChanged(); } }
        //IP匹配表达式
        private string _partten;
        public string Partten { get => _partten; set { _partten = value; RaisePropertyChanged(); } }
        //最大爬取页
        private int _maxPage;
        public int MaxPage { get => _maxPage; set { _maxPage = value; RaisePropertyChanged(); } }
        //抓取的错误码
        private string _exceptionDesc;
        public string ExceptionDesc { get => _exceptionDesc; set  { _exceptionDesc = value; RaisePropertyChanged(); } }
        private byte _enable;
        public byte Enable { get => _enable; set { _enable = value; RaisePropertyChanged(); } }
    }
}
