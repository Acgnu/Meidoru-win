using AcgnuX.Source.Bussiness.Constants;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 气泡提示ViewModel
    /// </summary>
    public class BubbleTipViewModel : ViewModelBase
    {
        //警示级别
        public AlertLevel AlertLevel { get; set; }
        //文本
        public string Text { get; set;}
        //是否展示
        private bool _IsShow;
        public bool IsShow
        {
            get { return _IsShow; }
            set { _IsShow = value; RaisePropertyChanged(); }
        }
        //淡出动画结束
        public Action AnimationEndAction { get; set; }
    }
}
