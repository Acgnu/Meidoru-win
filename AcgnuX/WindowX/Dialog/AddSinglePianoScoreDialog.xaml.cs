using AcgnuX.Source.Utils;
using System.Windows;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 添加弹琴吧曲谱窗口
    /// </summary>
    public partial class AddSinglePianoScoreDialog : BaseDialog
    {
        #region binding
        //乐谱ID
        public string Ypid { get; set; }
        //是否自动下载
        public bool AutoDownload { get; set; }
        //是否使用代理
        public bool UseProxy { get; set; } = true;
        //乐谱名称
        public string SheetName { get; set; }
        //下载任务数
        public int? TaskNum { get; set; }
        #endregion

        public AddSinglePianoScoreDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 点击确定事件
        /// </summary>
        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            if (null == TaskNum) TaskNum = 5;
            AnimateClose((s, a) => DialogResult = true);
        }
    }
}