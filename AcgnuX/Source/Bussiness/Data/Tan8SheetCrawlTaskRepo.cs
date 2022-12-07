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
    /// 乐谱下载任务
    /// </summary>
    public class Tan8SheetCrawlTaskRepo
    {
        public static Tan8SheetCrawlTaskRepo Instance => new Tan8SheetCrawlTaskRepo();

        private Tan8SheetCrawlTaskRepo()
        {

        }

        /// <summary>
        /// 查询所有的任务
        /// </summary>
        /// <returns></returns>
        public List<int> FindAllTaskYpid()
        {
            var taskIds = SQLite.sqlcolumn("SELECT ypid FROM tan8_music_down_task", null);
            return taskIds.Select(e => Convert.ToInt32(e)).ToList();
        }
    }
}
