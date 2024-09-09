using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace AcgnuX.Source.Bussiness.Data
{
    /// <summary>
    /// 乐谱下载任务
    /// </summary>
    public class Tan8SheetCrawlTaskRepo
    {
        /// <summary>
        /// 查询所有的任务
        /// </summary>
        /// <returns></returns>
        public List<int> FindAllTaskYpid()
        {
            var taskIds = SQLite.sqlcolumn("SELECT ypid FROM tan8_music_down_task", null);
            return taskIds.Select(e => Convert.ToInt32(e)).ToList();
        }

        /// <summary>
        /// 新增任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal int AddNew(int id)
        {
            var r = SQLite.ExecuteNonQuery("INSERT INTO tan8_music_down_task (ypid) VALUES (@ypid)",
                new List<SQLiteParameter> { new SQLiteParameter("@ypid", id) });
            return r;
        }

        /// <summary>
        /// 根据乐谱ID删除
        /// </summary>
        /// <param name="ypid"></param>
        internal void DelByYpid(int ypid)
        {
            SQLite.ExecuteNonQuery("DELETE FROM tan8_music_down_task WHERE ypid = @ypid",
                new List<SQLiteParameter> { new SQLiteParameter("@ypid", ypid) });
        }
    }
}
