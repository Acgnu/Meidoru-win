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
        public string EnableAndView
        {
            get { return Enable == 1 ? "已启用" : "已停用"; }
            set { }
        }
    }
}
