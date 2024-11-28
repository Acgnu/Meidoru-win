using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// 一般的数据处理工具类
    /// </summary>
    public class ConfigUtil
    {
        /// <summary>
        /// 为INI文件中指定的节点取得字符串
        /// </summary>
        /// <param name="lpAppName">欲在其中查找关键字的节点名称</param>
        /// <param name="lpKeyName">欲获取的项名</param>
        /// <param name="lpDefault">指定的项没有找到时返回的默认值</param>
        /// <param name="lpReturnedString">指定一个字串缓冲区，长度至少为nSize</param>
        /// <param name="nSize">指定装载到lpReturnedString缓冲区的最大字符数量</param>
        /// <param name="lpFileName">INI文件完整路径</param>
        /// <returns>复制到lpReturnedString缓冲区的字节数量，其中不包括那些NULL中止字符</returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        /// <summary>
        /// 修改INI文件中内容
        /// </summary>
        /// <param name="lpApplicationName">欲在其中写入的节点名称</param>
        /// <param name="lpKeyName">欲设置的项名</param>
        /// <param name="lpString">要写入的新字符串</param>
        /// <param name="lpFileName">INI文件完整路径</param>
        /// <returns>非零表示成功，零表示失败</returns>
        [DllImport("kernel32")]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

        public static ConfigUtil Instance { get; private set; } = new ConfigUtil();
        //AcgnuX.ini
        private static string mConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Reflection.Assembly.GetEntryAssembly().GetName().Name + ".ini");

        //public ConfigUtil Load()
        //{
        //    //读取数据库的配置文件
        //    PianoScorePath = Read("Path", "YpHome", "", mConfPath);
        //    AccountJsonPath = Read("Path", "AccountFile", "", mConfPath);
        //    DbFilePath = Read("Path", "DbFile", "", mConfPath);
        //    return this;
        //}

        //public ConfigUtil Load(string configPath)
        //{
        //    mConfPath = configPath;
        //    return Load();
        //}

        //public async void Save(Settings settings)
        //{
        //    AccountJsonPath = settings.AccountJsonPath;
        //    PianoScorePath = settings.PianoScorePath;
        //    DbFilePath = settings.DbFilePath;

        //    Write("Path", "YpHome", PianoScorePath, mConfPath);
        //    Write("Path", "AccountFile", AccountJsonPath, mConfPath);
        //    Write("Path", "DbFile", DbFilePath, mConfPath);

        //    if (!string.IsNullOrEmpty(DbFilePath))
        //    {
        //        await SQLite.SetDbFilePath(DbFilePath);
        //    }
        //}

        /// <summary>
        /// 读取INI文件值
        /// </summary>
        /// <param name="section">节点名</param>
        /// <param name="key">键</param>
        /// <param name="def">未取到值时返回的默认值</param>
        /// <param name="filePath">INI文件完整路径</param>
        /// <returns>读取的值</returns>
        public static string Read(string section, string key, string def, string filePath)
        {
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def, sb, 1024, filePath);
            return sb.ToString();
        }

        /// <summary>
        /// 写INI文件值
        /// </summary>
        /// <param name="section">欲在其中写入的节点名称</param>
        /// <param name="key">欲设置的项名</param>
        /// <param name="value">要写入的新字符串</param>
        /// <param name="filePath">INI文件完整路径</param>
        /// <returns>非零表示成功，零表示失败</returns>
        public static int Write(string section, string key, string value, string filePath)
        {
            //CheckPath(filePath);
            return WritePrivateProfileString(section, key, value, filePath);
        }

        /// <summary>
        /// 删除节
        /// </summary>
        /// <param name="section">节点名</param>
        /// <param name="filePath">INI文件完整路径</param>
        /// <returns>非零表示成功，零表示失败</returns>
        public static int DeleteSection(string section, string filePath)
        {
            return Write(section, null, null, filePath);
        }

        /// <summary>
        /// 删除键的值
        /// </summary>
        /// <param name="section">节点名</param>
        /// <param name="key">键名</param>
        /// <param name="filePath">INI文件完整路径</param>
        /// <returns>非零表示成功，零表示失败</returns>
        public static int DeleteKey(string section, string key, string filePath)
        {
            return Write(section, key, null, filePath);
        }

        /// <summary>
        /// 获取指定应用用户最新的配置文件路径
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns>配置文件路径或空</returns>
        public static string GetLatestConfigFile(string applicationName)
        {
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDataDirectory = Path.Combine(localAppDataPath, applicationName);

            // 获取最新版本的配置文件
            DateTime? lastWriteTime = null;
            string lastConfigFile = string.Empty;
            foreach (var dir in Directory.GetDirectories(appDataDirectory))
            {
                foreach (var subDir in Directory.GetDirectories(dir))
                {
                    var configFiles = Directory.GetFiles(subDir);
                    if (configFiles.Length == 0) continue;

                    var configFile = configFiles[0];
                    var fileLastWriteTime = File.GetLastWriteTime(configFile);
                    if (null == lastWriteTime || fileLastWriteTime.CompareTo(lastWriteTime) > 0)
                    {
                        lastWriteTime = fileLastWriteTime;
                        lastConfigFile = configFile;
                        continue;
                    }
                }
            }
            return lastConfigFile;
        }
    }
}
