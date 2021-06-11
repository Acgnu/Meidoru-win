using AcgnuX.Source.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 一般弹出窗口的数据容器
    /// </summary>
    public class CommonWindowViewModel : INotifyPropertyChanged
    {
        //主窗口对象
        protected Window mWindow;
        //属性变更通知
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        //最小化命令
        public ICommand MinimizeCommand { get; set; }
        //最大化命令
        public ICommand MaximizeCommand { get; set; }
        //关闭命令
        public ICommand CloseCommand { get; set; }

        public CommonWindowViewModel(Window window)
        {
            //初始化参数
            mWindow = window;

            //初始化命令
            MinimizeCommand = new RelayCommand(() => mWindow.WindowState = WindowState.Minimized);
            MaximizeCommand = new RelayCommand(() => mWindow.WindowState ^= WindowState.Maximized);
            CloseCommand = new RelayCommand(() => mWindow.Close());

            //注册事件
            mWindow.StateChanged += (sender, e) =>
            {

            };
        }
    }
}
