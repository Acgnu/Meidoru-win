using AcgnuX.Source.Model;
using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// 一般的数据处理工具类
    /// </summary>
    public class ConfigUtil : Settings
    {
        public static ConfigUtil Instance { get; private set; } = new ConfigUtil();

        public void Load()
        {
            //读取数据库的配置文件
            var dbConfig = SQLite.SqlRow("SELECT account_file_dir, tan8_home_dir FROM pref");
            if (null != dbConfig && dbConfig.Length > 0)
            {
                AccountJsonPath = dbConfig[0];
                PianoScorePath = dbConfig[1];
            }
        }

        public void Save(Settings settings)
        {
            AccountJsonPath = settings.AccountJsonPath;
            PianoScorePath = settings.PianoScorePath;
            SQLite.ExecuteNonQuery("update pref set account_file_dir = @account_file_dir, tan8_home_dir = @tan8_home_dir", new SQLiteParameter[] { 
                new SQLiteParameter("@account_file_dir", settings.AccountJsonPath) ,
                new SQLiteParameter("@tan8_home_dir", settings.PianoScorePath) 
            });
        }
    }
}
