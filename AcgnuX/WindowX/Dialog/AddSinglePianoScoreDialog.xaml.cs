using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 添加弹琴吧曲谱窗口
    /// </summary>
    public partial class AddSinglePianoScoreDialog : BaseDialog
    {
        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        public Action<Tan8SheetCrawlArg> ConfirmAction;

        #region binding
        //乐谱ID
        public string Ypid { get; set; }
        //是否自动下载
        public bool AutoDownload { get; set; }
        //是否使用代理
        public bool UseProxy { get; set; } = true;
        //乐谱名称
        public string SheetName { get; set; }
        //确认按钮点击命令
        public ICommand OnConfirmCommand { get; set; }
        #endregion

        public AddSinglePianoScoreDialog()
        {
            InitializeComponent();
            DataContext = this;

            OnConfirmCommand = new RelayCommand(OnConfirmClick);
        }

        /// <summary>
        /// 点击确定事件
        /// </summary>
        private void OnConfirmClick()
        {
            //执行任务
            var arg = new Tan8SheetCrawlArg
            {
                Name = SheetName,
                AutoDownload = AutoDownload,
                UseProxy = UseProxy,
                Ver = 2
            };
            if (!string.IsNullOrEmpty(Ypid) && DataUtil.IsNum(Ypid))
            {
                arg.Ypid = Convert.ToInt32(Ypid);
            }
            ConfirmAction?.Invoke(arg);
            DialogResult = true;
            Close();
        }
    }
}