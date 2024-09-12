namespace SharedLib.Model
{
    /// <summary>
    /// tan8接口返回的乐谱信息
    /// </summary>
    public class Tan8music
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public long yp_create_time { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string yp_title { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public byte yp_page_count { get; set; }
        /// <summary>
        /// 图片宽度
        /// </summary>
        public short yp_page_width { get; set; }
        /// <summary>
        /// 图片高度
        /// </summary>
        public short yp_page_height { get; set; }
        /// <summary>
        /// 大调标识
        /// </summary>
        public byte yp_is_dadiao { get; set; }
        public byte yp_key_note { get; set; }
        /// <summary>
        /// 延音标识
        /// </summary>
        public byte yp_is_yanyin { get; set; }
        /// <summary>
        /// 乐谱图片url 需要手动拼接 .页码.png
        /// </summary>
        public string ypad_url { get; set; }
        /// <summary>
        /// 播放文件地址
        /// </summary>
        public string ypad_url2 { get; set; }
        /// <summary>
        /// 试听mp3文件
        /// </summary>
        public string mp3_url { get; set; }
    }
}
