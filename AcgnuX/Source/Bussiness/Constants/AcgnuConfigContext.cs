using AcgnuX.Source.Model;

namespace AcgnuX.Source.Bussiness.Constants
{
    /// <summary>
    /// 配置文件容器
    /// </summary>
    public class AcgnuConfigContext : BasePropertyChangeNotifyModel
    {
        /// <summary>
        /// 账号密码文件存储的json完整路径
        /// </summary>
        public string accountJsonPath { get; set; }

        /// <summary>
        /// 乐谱文件目录
        /// </summary>
        public string pianoScorePath { get; set; }
    }
}
