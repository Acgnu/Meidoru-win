using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 文件列表对比器视图模型
    /// </summary>
    public class FileItemsCompareViewModel : ObservableObject
    {
        //电脑端目录路径
        public string PcFolderPath { get; set; }
        //移动端目录路径
        public string MobileFolderPath { get; set; }
        //电脑端文件列表
        public FileItemsListViewModel PcFileItems { get; set; }
        //移动端文件列表
        public FileItemsListViewModel MobileFileItems { get; set; }
        //是否正在同步
        public bool isBusy;
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value); }

        //同步按钮点命令
        public ICommand OnSyncCommand { get; set; }
        public ICommand OnOpenFolderCommand { get; set; }

        //主管理
        public DeviceSyncViewModel DeviceSyncViewModel { get; set; }

        public FileItemsCompareViewModel()
        {
            OnSyncCommand = new RelayCommand(OnSyncButtonClick);
            OnOpenFolderCommand = new RelayCommand(OnOpenFolderClick);
        }


        /// <summary>
        /// 点击同步按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSyncButtonClick()
        {
            IsBusy = true;
            //var selected = (DeviceSyncListViewModel)((ListViewItem)DeviceSyncListView.ContainerFromElement(button)).Content;
            //将任务添加进队列
            DeviceSyncViewModel.AddItemToQueueAndStartTask(this);
        }

        /// <summary>
        /// 打开文件夹图标点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenFolderClick()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = PcFolderPath,
                UseShellExecute = true
            });
        }
    }
}
