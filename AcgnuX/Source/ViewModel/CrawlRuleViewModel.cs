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
                OnPropertyChanged(nameof(Enable));
            }
        }
        public string NameView
        {
            get { return Name; }
            set
            {
                Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string UrlView
        {
            get { return Url; }
            set
            {
                Url = value;
                OnPropertyChanged(nameof(Url));
            }
        }
        public string ParttenView
        {
            get { return Partten; }
            set
            {
                Partten = value;
                OnPropertyChanged(nameof(Partten));
            }
        }
        public int MaxPageView
        {
            get { return MaxPage; }
            set
            {
                MaxPage = value;
                OnPropertyChanged(nameof(MaxPage));
            }
        }
    }
}
