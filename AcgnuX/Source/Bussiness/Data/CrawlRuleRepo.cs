using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.Data
{
    /// <summary>
    /// 抓取规则
    /// </summary>
    public class CrawlRuleRepo
    {
        public static CrawlRuleRepo Instance => new CrawlRuleRepo();

        private CrawlRuleRepo()
        {

        }

        /// <summary>
        /// 更新错误描述
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v"></param>
        public void UpdateExceptionDesc(int id, string desc)
        {
            SQLite.ExecuteNonQuery("UPDATE crawl_rules SET exception_desc  = @desc WHERE id =@id",
                  new List<SQLiteParameter> {
                        new SQLiteParameter("@desc", desc), 
                      new SQLiteParameter("@id", id)
                  });
        }
    }
}
