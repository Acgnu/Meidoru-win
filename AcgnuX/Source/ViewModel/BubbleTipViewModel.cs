using AcgnuX.Source.Bussiness.Constants;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 气泡提示ViewModel
    /// </summary>
    public class BubbleTipViewModel : ObservableRecipient
    {
        //警示级别
        public AlertLevel AlertLevel { get; set; }
        //文本
        public string Text { get; set;}
        //是否展示
        private bool isShow;
        public bool IsShow { get => isShow; set => SetProperty(ref isShow, value); }
        //淡出动画结束
        public Action AnimationEndAction { get; set; }
    }
}
