using System.Windows.Media;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// UI 展示对象
    /// </summary>
    public class PianoScore : BasePropertyChangeNotifyModel
    {
        // 对应弹琴吧的乐谱id
        public int? id { get; set; }
        // 乐谱名称
        public string Name { get; set; }
        // 喜好评级 1 - 5
        public byte Star { get; set; }
        //封面图
        public ImageSource cover { get; set; }
        //是否自动下载 (下载需要)
        public bool AutoDownload { get; set; }
        public bool UseProxy { get; set; } = true;
        //曲谱下载地址 (适用于 v2 )
        public string SheetUrl { get; set; }
        public byte YpCount { get; set; }
        //版本 1, 2
        public byte Ver { get; set; }
    }
}
