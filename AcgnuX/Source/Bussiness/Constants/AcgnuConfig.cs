using AcgnuX.Source.Utils;

namespace AcgnuX.Source.Bussiness.Constants
{
    public class AcgnuConfig
    {
        private static AcgnuConfigContext context;

        private AcgnuConfig()
        {

        }

        public static AcgnuConfigContext GetContext()
        {
            return context;
        }

        /// <summary>
        /// 系统配置初始化
        /// </summary>
        public static void Init()
        {
            //读取数据库的配置文件
            var dbConfig = SQLite.SqlRow("SELECT account_file_dir, tan8_home_dir FROM pref");
            if (null != dbConfig && dbConfig.Length > 0)
            {
                context = new AcgnuConfigContext()
                {
                    accountJsonPath = dbConfig[0],
                    pianoScorePath = dbConfig[1]
                };
            }
            else
            {
                context = new AcgnuConfigContext();
                SQLite.ExecuteNonQuery(string.Format("insert into pref (account_file_dir, tan8_home_dir) values ('{0}', '{1}')", "", ""));
            }
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="context"></param>
        public static void SaveSetting(AcgnuConfigContext context)
        {
            SQLite.ExecuteNonQuery(string.Format("update pref set account_file_dir = '{0}', tan8_home_dir = '{1}'", context.accountJsonPath, context.pianoScorePath));
        }
    }
}
