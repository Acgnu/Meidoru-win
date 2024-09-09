using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Utils;
using System.Windows;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 弹窗以确定
    /// </summary>
    public partial class ConfirmDialog : BaseDialog
    {
        //弹窗标题
        public string DialogTitle { get; set; }
        //弹窗内容
        public string Message { get; set; }

        public ConfirmDialog(AlertLevel alertLevel, string message)
        {
            InitializeComponent();
            DialogTitle = EnumLoader.GetDesc(alertLevel);
            Message = message;
            DataContext = this;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            //DialogResult 赋值后内部会自动调用Close()
            AnimateClose((s, a) => DialogResult = true);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            AnimateClose((s, a) => DialogResult = false);
        }
    }
}