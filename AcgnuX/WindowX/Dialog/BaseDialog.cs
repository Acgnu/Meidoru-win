using AcgnuX.Pages;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 对话框基类
    /// </summary>
    public class BaseDialog : Window
    {
        //默认的标题栏高度
        public int TitleHeightGridLength { get; set; } = 30;

        //关闭对话框命令
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// 构造函数接收一个窗口作为父窗口
        /// </summary>
        public BaseDialog()
        {
            CloseCommand = new RelayCommand(() => this.Close());
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;
        }
    }
}
