using AcgnuX.Source.Bussiness.Constants;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 分页信息
    /// </summary>
    class Pager
    {
        //当前页
        public int CurrentPage { get; set; }
        //每页最大行
        public int MaxRow { get; set; }
        //最大页数
        public int TotalPage { get; set; }
        //-1 上一页 0 当前页刷新 1 下一页
        public PageAction Action { get; set; }

        public Pager(int currentPage, int maxRow)
        {
            CurrentPage = currentPage;
            MaxRow = maxRow;
        }
    }
}
