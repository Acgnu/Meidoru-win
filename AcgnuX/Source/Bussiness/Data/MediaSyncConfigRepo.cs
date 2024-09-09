using AcgnuX.Source.Model;
using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace AcgnuX.Source.Bussiness.Data
{
    /// <summary>
    /// 路径配置数据库对象
    /// </summary>
    public class MediaSyncConfigRepo
    {
        /// <summary>
        /// 查询路径配置
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public List<MediaSyncConfig> FindConfig(bool? enable)
        {
            var confgs = new List<MediaSyncConfig>();
            var dataSet = SQLite.SqlTable(string.Format("SELECT id, pc_path, mobile_path, enable FROM media_sync_config {0}",  
                null == enable ? "" : string.Format("where enable = {0}", enable.GetValueOrDefault() ? 1 : 0)), null);
            if (null == dataSet || dataSet.Rows.Count == 0)
            {
                return confgs;
            }
            //封装进对象
            foreach (DataRow dataRow in dataSet.Rows)
            {
                confgs.Add(new MediaSyncConfig
                {
                    Id = Convert.ToInt32(dataRow["id"]),
                    PcPath = Convert.ToString(dataRow["pc_path"]),
                    MobilePath = Convert.ToString(dataRow["mobile_path"]),
                    Enable = Convert.ToBoolean(dataRow["enable"])
                });
            }
            return confgs;
        }

        /// <summary>
        /// 根据ID删除
        /// </summary>
        /// <param name="id"></param>
        internal void DeleteById(int id)
        {
            SQLite.ExecuteNonQuery("DELETE FROM media_sync_config WHERE ID = @id", new List<SQLiteParameter> { new SQLiteParameter("@id", id) });
        }

        /// <summary>
        /// 更新同步配置启用状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="enable"></param>
        internal void UpdateSyncConfigEnable(int? id, bool enable)
        {
            SQLite.ExecuteNonQuery(string.Format("UPDATE media_sync_config SET enable = {0} {1}",
                enable ? 1 : 0,
                null == id ? "" : " WHERE id = " + id), null);
        }
    }
}
