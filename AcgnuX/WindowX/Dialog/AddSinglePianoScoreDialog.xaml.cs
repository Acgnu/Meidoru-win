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
            AnimateClose((s, a) => DialogResult = true);
        }
    }
}