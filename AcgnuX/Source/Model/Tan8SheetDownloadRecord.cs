namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 曲谱下载记录
    /// </summary>
    public class Tan8SheetDownloadRecord
    {
        //记录id
        public int Id { get; set; }
        //下载的乐谱id
        public int Ypid { get; set; }
        //乐谱名称
        public string Name { get; set; }
        //记录时间
        public string Create_time { get; set; }
        //下载结果
        public string Result { get; set; }
    }
}
