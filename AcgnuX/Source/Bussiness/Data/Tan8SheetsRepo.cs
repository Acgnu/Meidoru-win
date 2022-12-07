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
    public class Tan8SheetsRepo
    {
        public static Tan8SheetsRepo Instance => new Tan8SheetsRepo();

        private Tan8SheetsRepo()
        {

        }

        /// <summary>
        /// 查询数据库中的乐谱
        /// </summary>
        /// <param name="keyword">
        /// 前缀
        /// f:w w完整名称
        /// s:w 以w开始
        /// e:w 以w结束
        /// 无前缀
        /// %模糊搜索%
        /// 纯数字
        /// Id或名称搜索
        /// </param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Tan8Sheet> Find(string keyword, int pageNo, int pageSize)
        {
            //查询关键字
            var sql = new StringBuilder(" FROM tan8_music WHERE 1 = 1");
            var sqlArgs = new List<SQLiteParameter>();
            if (!string.IsNullOrEmpty(keyword))
            {
                var keywordGroup = keyword.Length > 2 ? keyword.Split(':') : new string[] { string.Empty };
                //高级搜索
                if (keywordGroup[0].Equals("f"))
                {
                    //完整名称搜索 | 完整ID搜索
                    if (DataUtil.IsNum(keywordGroup[1]))
                    {
                        sql.Append(" and ypid = @ypid");
                        sqlArgs.Add(new SQLiteParameter("@ypid", keywordGroup[1]));
                    }
                    else
                    {
                        sql.Append(" and name = @name");
                        sqlArgs.Add(new SQLiteParameter("@name", keywordGroup[1]));
                    }
                }
                else if (keywordGroup[0].Equals("s"))
                {
                    //以 .. 开头
                    sql.Append(" and name like @name");
                    sqlArgs.Add(new SQLiteParameter("@name", keywordGroup[1] + "%"));
                }
                else if (keywordGroup[0].Equals("e"))
                {
                    //以 .. 结尾
                    sql.Append(" and name like @name");
                    sqlArgs.Add(new SQLiteParameter("@name", "%" + keywordGroup[1]));
                }
                else
                {
                    sql.Append(" and name like @name");
                    sqlArgs.Add(new SQLiteParameter("@name", "%" + keyword + "%"));
                    if (DataUtil.IsNum(keyword))
                    {
                        sql.Append(" or ypid = @ypid");
                        sqlArgs.Add(new SQLiteParameter("@ypid", keyword));
                    }
                }
            }
            //封装进对象
            var dataList = new List<Tan8Sheet>();

            //查询总数
            var sqlRowResult = SQLite.sqlone("SELECT COUNT(1) total " + sql.ToString(), sqlArgs.ToArray());
            var totalRow = string.IsNullOrEmpty(sqlRowResult) ? 0 : Convert.ToInt32(sqlRowResult);
            //没有查到内容
            if (totalRow == 0)
            {
                return dataList;
            }
            //查询记录
            sql.Append(" order by star desc limit @maxRow OFFSET @maxRow * (@curPage - 1)");
            sqlArgs.Add(new SQLiteParameter("@maxRow", pageSize));
            sqlArgs.Add(new SQLiteParameter("@curPage", pageNo));

            var dataSet = SQLite.SqlTable("select ypid, name, star, yp_count, ver " + sql.ToString(), sqlArgs);

            foreach (DataRow dataRow in dataSet.Rows)
            {
                dataList.Add(new Tan8Sheet()
                {
                    Ypid = Convert.ToInt32(dataRow["ypid"]),
                    Name = Convert.ToString(dataRow["name"]),
                    Star = Convert.ToByte(dataRow["star"]),
                    YpCount = Convert.ToByte(dataRow["yp_count"]),
                    Ver = Convert.ToByte(dataRow["ver"]),
                });
            }
            return dataList;
        }

        /// <summary>
        /// 根据ID查询单个乐谱
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Tan8Sheet FindById(int id)
        {
            var dbObj = SQLite.SqlRow(string.Format("SELECT name, ver FROM tan8_music WHERE ypid = {0}", id));
            //不存在则返回null
            if (null == dbObj || dbObj.Length == 0) return null;
            //var folderName = tan8Music[0];
            return new Tan8Sheet
            {
                Name = dbObj[0],
                Ver = Convert.ToByte(dbObj[1])
            };
        }

        /// <summary>
        /// 更新收藏信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns>最新的收藏状态</returns>
        internal int UpdateStar(int id)
        {
            var dbStar = SQLite.sqlone("SELECT star FROM tan8_music WHERE ypid = @ypid",
                new SQLiteParameter[] { new SQLiteParameter("@ypid", id) });
            var nStarVal = 0;
            if (Convert.ToInt32(dbStar) == 0)
            {
                nStarVal = 1;
            }
            SQLite.ExecuteNonQuery("UPDATE tan8_music SET star = @star WHERE ypid = @ypid",
                new List<SQLiteParameter> {
                    new SQLiteParameter("@star", nStarVal), new SQLiteParameter("@ypid", id)
                });
            return nStarVal;
        }

        /// <summary>
        /// 根据ID删除
        /// </summary>
        /// <param name="id"></param>
        internal void DeleteById(int id)
        {
            SQLite.ExecuteNonQuery("DELETE FROM tan8_music WHERE ypid = @ypid", new List<SQLiteParameter> { new SQLiteParameter("@ypid", id) });
        }

        /// <summary>
        /// 更新乐谱名称
        /// </summary>
        /// <param name="ypid"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public InvokeResult<object> UpdateName(int ypid, string newName)
        {
            //获取原始名称
            var dbName = SQLite.SqlRow(string.Format("SELECT name FROM tan8_music WHERE ypid = {0}", ypid));
            if (null == dbName || dbName.Length == 0)
            {
                return new InvokeResult<object>()
                {
                    success = false,
                    message = "无法获取原始数据"
                };
            }

            //修改文件夹名称
            //FileUtil.RenameFolder(Path.Combine(ConfigUtil.Instance.PianoScorePath, dbName[0]), pianoScore.Name);
            //修改数据库名称
            SQLite.ExecuteNonQuery("UPDATE tan8_music SET name = @name WHERE ypid = @ypid", new List<SQLiteParameter>
            {
                new SQLiteParameter("@name", newName),
                new SQLiteParameter("@ypid", ypid)
            });
            return new InvokeResult<object>
            { 
                success = true
            };
        }

        /// <summary>
        /// 根据乐谱Id查询是否存在于数据库
        /// </summary>
        /// <param name="ypid"></param>
        /// <returns>true 存在</returns>
        public bool IsYpidExist(int ypid)
        {
            var sqlRowResult = SQLite.sqlone("SELECT COUNT(1) total FROM tan8_music where ypid = @ypid",
                new SQLiteParameter[] { new SQLiteParameter("@ypid", ypid) });
            if (string.IsNullOrEmpty(sqlRowResult)) return false;
            return Convert.ToInt32(sqlRowResult) > 0;
        }

        /// <summary>
        /// 保存乐谱信息到数据库
        /// </summary>
        /// <param name="ypid">弹琴吧的乐谱ID</param>
        /// <param name="name">保存的谱名</param>
        /// <param name="tan8Music">弹琴吧接口对象</param>
        /// <param name="originstr">原始接口数据</param>
        /// <returns>数据库操作成功条数</returns>
        internal int Save(int ypid, string name, Tan8music tan8Music, string originstr)
        {
            return SQLite.ExecuteNonQuery("insert or ignore into tan8_music(ypid, `name`, star, yp_count, origin_data) VALUES (@ypid, @name, @star, @yp_count, @origin_data)",
                new List<SQLiteParameter>
                {
                    new SQLiteParameter("@ypid", ypid) ,
                    new SQLiteParameter("@name", name) ,
                    new SQLiteParameter("@star", 0 as object) ,
                    new SQLiteParameter("@yp_count", tan8Music.yp_page_count) ,
                    new SQLiteParameter("@origin_data", originstr)
                 });
        }
    }
}
