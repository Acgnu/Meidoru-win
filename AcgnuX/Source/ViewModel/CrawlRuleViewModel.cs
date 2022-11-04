using AcgnuX.Source.Model;
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
    public class CrawlRuleViewModel : CrawlRule
    {
        public byte EnableView
        {
            get { return Enable; }
            set 
            {
                Enable = value;
                RaisePropertyChanged();
            }
        }
        public string NameView
        {
            get { return Name; }
            set
            {
                Name = value;
                RaisePropertyChanged();
            }
        }
        public string UrlView
        {
            get { return Url; }
            set
            {
                Url = value;
                RaisePropertyChanged();
            }
        }
        public string ParttenView
        {
            get { return Partten; }
            set
            {
                Partten = value;
                RaisePropertyChanged();
            }
        }
        public int MaxPageView
        {
            get { return MaxPage; }
            set
            {
                MaxPage = value;
                RaisePropertyChanged();
            }
        }
    }
}
