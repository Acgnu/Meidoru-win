using SharedLib.Model;
using SharedLib.Utils;
using System.Data;
using System.Data.SQLite;

namespace SharedLib.Data
{
    /// <summary>
    /// 代理池数据访问
    /// </summary>
    public class ProxyAddressRepo
    {
        public int GetProxyCountFromDB()
        {
            var result = SQLite.sqlone("SELECT COUNT(1) FROM proxy_address ORDER BY addtime", null);
            if (string.IsNullOrEmpty(result)) return 0;
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// 获取IP池里的第一个代理IP地址
        /// </summary>
        /// <returns></returns>
        public string GetFirstProxy()
        {
            //if (mProxyIpPool.IsEmpty) return null;
            //return mProxyIpPool.ElementAt(0);
            return SQLite.sqlone("SELECT address FROM proxy_address ORDER BY addtime LIMIT 1", null);
        }

        /// <summary>
        /// 获取指定的代理
        /// </summary>
        /// <returns></returns>
        public ProxyAddress GetProxy(string proxyAddress)
        {
            var dbProxy = SQLite.SqlRow(string.Format(string.Format("SELECT address, rule_id FROM proxy_address WHERE address = '{0}'", proxyAddress)));
            if (null != dbProxy && dbProxy.Length > 0)
            {
                return new ProxyAddress()
                {
                    Address = dbProxy[0],
                    RuleId = Convert.ToInt32(dbProxy[1])
                };
            }
            return null;
        }


        /// <summary>
        /// 从代理池移除IP
        /// </summary>
        /// <param name="proxyAddress">需要移除的IP地址</param>
        /// <param name="requeeTime">重新放入IP池的等待时间 (毫秒), 0 = 抛弃</param>
        public void RemoveProxy(string proxyAddress, int requeeTime)
        {
            var dbProxy = GetProxy(proxyAddress);

            if (null == dbProxy) return; //可能被其他线程删掉了

            SQLite.ExecuteNonQuery("DELETE from proxy_address WHERE address = @address",
                new List<SQLiteParameter> { new SQLiteParameter("@address", proxyAddress) });

            if (requeeTime == 0) return;

            Task.Run(() =>
            {
                Thread.Sleep(requeeTime);
                SaveProxyToDB(proxyAddress, dbProxy.RuleId);
            });
        }

        /// <summary>
        /// 保存一条代理到数据库
        /// </summary>
        /// <param name="proxyAddress"></param>
        public void SaveProxyToDB(string proxyAddress, int ruleId)
        {
            SQLite.ExecuteNonQuery("INSERT OR IGNORE INTO proxy_address(address, addtime, rule_id) VALUES (@address, datetime('now', 'localtime'), @ruleId)",
                      new List<SQLiteParameter> {
                          new SQLiteParameter("@address", proxyAddress),
                          new SQLiteParameter("@ruleId", ruleId)
                      });
        }

        /// <summary>
        /// 查询数据库中所有的代理
        /// </summary>
        /// <returns></returns>
        public List<ProxyAddress> GetAllProxyFromDB()
        {
            var proxyList = new List<ProxyAddress>();
            var dataSet = SQLite.SqlTable("SELECT address, addtime FROM proxy_address ORDER BY addtime", null);
            if (null == dataSet) return proxyList;
            foreach (DataRow dataRow in dataSet.Rows)
            {
                proxyList.Add(new ProxyAddress()
                {
                    Address = Convert.ToString(dataRow["address"]),
                    Addtime = Convert.ToDateTime(dataRow["addtime"])
                });
            }
            return proxyList;
        }
    }
}
