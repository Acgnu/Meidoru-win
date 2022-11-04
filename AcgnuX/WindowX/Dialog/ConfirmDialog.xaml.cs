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
            DialogResult = true;
            CloseCommand.Execute(null);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            CloseCommand.Execute(null);
        }
    }
}