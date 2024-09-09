using SharedLib.Model;
using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Data
{
    /// <summary>
    /// 抓取规则
    /// </summary>
    public class CrawlRuleRepo
    {

        /// <summary>
        /// 新增规则
        /// </summary>
        /// <param name="crawlRule"></param>
        /// <returns></returns>
        public int Add(CrawlRule crawlRule)
        {
            var row = SQLite.ExecuteNonQuery("INSERT INTO crawl_rules(ID, NAME, URL, PARTTEN, MAX_PAGE, ENABLE) VALUES ((SELECT MAX(ID)+1 FROM crawl_rules), @Name, @Url, @Partten, @MaxPage, @Enable)",
                new List<SQLiteParameter> {
                    new SQLiteParameter("@Name", crawlRule.Name) ,
                    new SQLiteParameter("@Url", crawlRule.Url) ,
                    new SQLiteParameter("@Partten", crawlRule.Partten) ,
                    new SQLiteParameter("@MaxPage", crawlRule.MaxPage) ,
                    new SQLiteParameter("@Enable", crawlRule.Enable)
                });

            if (row > 0)
            {
                var sLastId = SQLite.sqlone("SELECT LAST_INSERT_ROWID() FROM crawl_rules", null);
                return Convert.ToInt32(sLastId);
            }
            return 0;
        }

        /// <summary>
        /// 更新规则
        /// </summary>
        /// <param name="crawlRule"></param>
        /// <returns></returns>
        public bool Update(CrawlRule crawlRule)
        {
            var row = SQLite.ExecuteNonQuery("UPDATE crawl_rules SET NAME = @Name, URL = @Url, PARTTEN = @Partten, MAX_PAGE = @MaxPage, ENABLE = @Enable WHERE ID = @Id",
               new List<SQLiteParameter> {
                    new SQLiteParameter("@Name", crawlRule.Name) ,
                    new SQLiteParameter("@Url", crawlRule.Url) ,
                    new SQLiteParameter("@Partten", crawlRule.Partten) ,
                    new SQLiteParameter("@MaxPage", crawlRule.MaxPage) ,
                    new SQLiteParameter("@Enable", crawlRule.Enable),
                    new SQLiteParameter("@Id", crawlRule.Id)
               });
            return row > 1;
        }

        /// <summary>
        /// 查询所有规则
        /// </summary>
        /// <returns></returns>
        public async Task<List<CrawlRule>> FindCrawlRuleAsync()
        {
            //从数据库读取规则
            return await Task.Run(() =>
            {
                var resultList = new List<CrawlRule>();
                var dataSet = SQLite.SqlTable("SELECT id, name, url, partten, max_page, exception_desc, enable FROM crawl_rules", null);
                if (null == dataSet) return resultList;
                //封装进对象
                foreach (DataRow dataRow in dataSet.Rows)
                {
                    resultList.Add(new CrawlRule()
                    {
                        Id = Convert.ToInt32(dataRow["id"]),
                        Name = Convert.ToString(dataRow["name"]),
                        Url = Convert.ToString(dataRow["url"]),
                        Partten = Convert.ToString(dataRow["partten"]),
                        MaxPage = Convert.ToInt32(dataRow["max_page"]),
                        ExceptionDesc = dataRow["exception_desc"].ToString(),
                        Enable = Convert.ToByte(dataRow["enable"]),
                    });
                }
                return resultList;
            });
        }

        /// <summary>
        /// 删除规则
        /// </summary>
        /// <param name="id"></param>
        public void Delete(int id)
        {
            SQLite.ExecuteNonQuery("DELETE FROM crawl_rules WHERE ID = @id", new List<SQLiteParameter> { new SQLiteParameter("@id", id) });
        }

        /// <summary>
        /// 更新规则启用状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="enable"></param>
        public void UpdateCrawlRulesEnable(int? id, bool enable)
        {
            SQLite.ExecuteNonQuery(string.Format("UPDATE crawl_rules SET enable = {0} {1}",
                enable ? 1 : 0,
                null == id ? "" : " WHERE id = " + id), null);
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
