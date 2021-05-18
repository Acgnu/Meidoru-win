namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 应用设置类
    /// </summary>
    public class Settings : BasePropertyChangeNotifyModel
    {
        /// <summary>
        /// 账户文件保存路径
        /// </summary>
        public string AccountJsonPath { get; set; }
        /// <summary>
        /// 乐谱文件目录
        /// </summary>
        public string PianoScorePath { get; set; }
    }
}
