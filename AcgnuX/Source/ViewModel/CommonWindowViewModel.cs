using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 一般弹出窗口的数据容器
    /// </summary>
    public class CommonWindowViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        //属性变更通知
        //public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        //最小化命令
        public ICommand MinimizeCommand { get; private set; }
        //最大化命令
        public ICommand MaximizeCommand { get; private set; }
        //关闭命令
        public ICommand CloseCommand { get; set; }

        public CommonWindowViewModel()
        {
            //初始化命令
            MinimizeCommand = new RelayCommand<Window>((window) => window.WindowState = WindowState.Minimized);
            MaximizeCommand = new RelayCommand<Window>((window) => window.WindowState ^= WindowState.Maximized);
            CloseCommand = new RelayCommand<Window>((window) => {
           
            });

            //注册事件
            //mWindow.StateChanged += (sender, e) =>
            //{

            //};
        }
    }
}
