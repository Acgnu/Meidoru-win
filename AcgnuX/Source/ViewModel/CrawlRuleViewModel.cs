using CommunityToolkit.Mvvm.ComponentModel;
using SharedLib.Model;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 抓取规则视图模型
    /// </summary>
    public class CrawlRuleViewModel : ObservableObject
    {
        //数据库对象
        public CrawlRule _CrawlRule { get; }

        public int Id { get => _CrawlRule.Id; set => SetProperty(_CrawlRule.Id, value, _CrawlRule, (rule, v) => rule.Id = v); }
        public string Name { get => _CrawlRule.Name; set => SetProperty(_CrawlRule.Name, value, _CrawlRule, (rule, v) => rule.Name = v); }
        //目标地址
        public string Url { get => _CrawlRule.Url; set => SetProperty(_CrawlRule.Url, value, _CrawlRule, (rule, v) => rule.Url = v); }
        //IP匹配表达式
        public string Partten { get => _CrawlRule.Partten; set => SetProperty(_CrawlRule.Partten, value, _CrawlRule, (rule, v) => rule.Partten = v); }
        //最大爬取页
        public int MaxPage { get => _CrawlRule.MaxPage; set => SetProperty(_CrawlRule.MaxPage, value, _CrawlRule, (rule, v) => rule.MaxPage = v); }
        //抓取的错误码
        public string ExceptionDesc { get => _CrawlRule.ExceptionDesc; set => SetProperty(_CrawlRule.ExceptionDesc, value, _CrawlRule, (rule, v) => rule.ExceptionDesc = v); }
        public byte Enable { get => _CrawlRule.Enable; set => SetProperty(_CrawlRule.Enable, value, _CrawlRule, (rule, v) => rule.Enable = v); }


        public CrawlRuleViewModel(CrawlRule crawlRule = null)
        {
            if (null == crawlRule)
            {
                crawlRule = new CrawlRule()
                {
                    MaxPage = 1,
                    Enable = 1
                };
            }
            this._CrawlRule = crawlRule;
        }
    }
}
