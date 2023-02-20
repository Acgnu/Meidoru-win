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
    /// 乐谱下载历史
    /// </summary>
    public class Tan8SheetCrawlRecordRepo
    {
        public static Tan8SheetCrawlRecordRepo Instance => new Tan8SheetCrawlRecordRepo();

        private Tan8SheetCrawlRecordRepo()
        {

        }

        /// <summary>
        /// 查询最后一次下载的乐谱ID
        /// </summary>
        /// <returns>没有则返回0</returns>
        public int FindLastCrawlYpid()
        {
            int lastYpid = 0;
            var recordLastId = SQLite.sqlone("SELECT ypid FROM tan8_music_down_record WHERE code = @code1 or code = @code2 ORDER BY create_time DESC LIMIT 1",
                        new SQLiteParameter[] {
                        new SQLiteParameter("@code1", Convert.ToInt32(Tan8SheetDownloadResult.SUCCESS)),
                        new SQLiteParameter("@code2", Convert.ToInt32(Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS))});

            if (!string.IsNullOrEmpty(recordLastId))
            {
                lastYpid = Convert.ToInt32(recordLastId) + 1;
            }
            return lastYpid;
        }

        /// <summary>
        /// 保存乐谱的下载记录
        /// </summary>
        /// <param name="ypid">乐谱ID</param>
        /// <param name="isAuto">是否自动下载</param>
        /// <param name="InvokeResult">下载结果</param>
        /// <returns></returns>
        internal int Save(int ypid, bool isAuto, string resultText, int code, string message)
        {
            return SQLite.ExecuteNonQuery("INSERT INTO tan8_music_down_record(id, ypid, name, code, result, create_time, is_auto) VALUES((SELECT IFNULL(MAX(id),0)  + 1 FROM tan8_music_down_record), @ypid, @result, @code, @message, datetime('now', 'localtime'), @isAuto)",
                new List<SQLiteParameter>
                {
                    new SQLiteParameter("@ypid", ypid) ,
                    new SQLiteParameter("@result", resultText) ,
                    new SQLiteParameter("@code", code) ,
                    new SQLiteParameter("@message", message) ,
                    new SQLiteParameter("@isAuto", isAuto)
                 });
        }

        /// <summary>
        /// 查询下载历史
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        internal List<Tan8SheetDownloadRecord> Find(List<int> codes)
        {
            List<Tan8SheetDownloadRecord> result = new List<Tan8SheetDownloadRecord>();
            if (DataUtil.IsEmptyCollection(codes)) return result;
            var condition = new StringBuilder();
            foreach (var item in codes)
            {
                condition.Append(item).Append(",");
            }
            //condition.Remove(condition.Length - 1, 1);
            condition.Length -= 1;
            var dataSet = SQLite.SqlTable(string.Format("SELECT id, ypid, name, strftime('%Y-%m-%d %H:%M:%S', create_time) create_time, result FROM tan8_music_down_record WHERE code in({0}) ORDER BY id DESC LIMIT 100", condition.ToString()), null);
            if (null == dataSet) return result;
            //封装进对象
            foreach (DataRow dataRow in dataSet.Rows)
            {
                //拼接得到cover路径
                result.Add(new Tan8SheetDownloadRecord()
                {
                    Id = Convert.ToInt32(dataRow["id"]),
                    Ypid = Convert.ToInt32(dataRow["ypid"]),
                    Name = Convert.ToString(dataRow["name"]),
                    Create_time = Convert.ToString(dataRow["create_time"]),
                    Result = Convert.ToString(dataRow["result"])
                });
            }
            return result;
        }

        /// <summary>
        /// 根据乐谱ID查询下载记录
        /// </summary>
        /// <returns></returns>
        internal Tan8SheetDownloadRecord FindByYpid(int ypid)
        {
            var dataRow = SQLite.SqlRow(string.Format("SELECT id, ypid, name, strftime('%Y-%m-%d %H:%M:%S', create_time) create_time, result  FROM tan8_music_down_record WHERE ypid = {0} ORDER BY create_time DESC LIMIT 1", ypid));
            return new Tan8SheetDownloadRecord
            {
                Id = Convert.ToInt32(dataRow[0]),
                Ypid = Convert.ToInt32(dataRow[1]),
                Name = Convert.ToString(dataRow[2]),
                Create_time = Convert.ToString(dataRow[3]),
                Result = Convert.ToString(dataRow[4])
            };
        }

        /// <summary>
        /// 根据ID删除
        /// </summary>
        /// <param name="ids"></param>
        internal void DelByIds(List<int> ids)
        {
            if (DataUtil.IsEmptyCollection(ids)) return;
            var idsStr = new StringBuilder();
            ids.ForEach(e => idsStr.Append(e).Append(","));
            idsStr.Remove(idsStr.Length - 1, 1);
            SQLite.ExecuteNonQuery(string.Format("DELETE FROM tan8_music_down_record WHERE ID IN ({0})", idsStr.ToString()), null);
        }
    }
}
