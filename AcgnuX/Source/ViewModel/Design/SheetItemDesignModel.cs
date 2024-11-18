namespace AcgnuX.Source.ViewModel.Design
{
    /// <summary>
    /// 乐谱库条目视图模型
    /// </summary>
    public class SheetItemDesignModel : SheetItemViewModel
    {
        public SheetItemDesignModel()
        {
            Id = 0;
            Name = "Sheet Name";
            YpCount = 10;
            Ver = 1;
            Star = 1;
            Progress = 30;
            ProgressText = "正在下载...";
            IsWorking = true;
        }
    }
}
