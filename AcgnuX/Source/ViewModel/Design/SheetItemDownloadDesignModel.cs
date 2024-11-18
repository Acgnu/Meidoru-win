﻿namespace AcgnuX.Source.ViewModel.Design
{
    /// <summary>
    /// 乐谱下载项视图模型
    /// </summary>
    public class SheetItemDownloadDesignModel : SheetItemDownloadViewModel
    {
        public SheetItemDownloadDesignModel()
        {
            Id = 0;
            Name = "Sheet Name";
            Progress = 30;
            ProgressText = "正在下载...";
        }
    }
}
