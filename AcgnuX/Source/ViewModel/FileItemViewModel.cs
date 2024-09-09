using AcgnuX.Source.Bussiness;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm;
using MediaDevices;
using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 文件视图模型
    /// </summary>
    public class FileItemViewModel : ObservableObject
    {
        //文件名
        public string Name { get; set; }
        //在视图中显示的名称
        //public string ViewName 
        //{ 
        //    get
        //    {
        //        if (Name.Length > 20) return Name.Substring(0, 20);
        //        return Name;
        //    }
        //}
        /// 文件类型
        public SyncContentType ContentType;
        //预览图
        public ByteArray PreviewImg { get; set; }
        //预览图类型 0-图片字节数组, 1-默认IMAGE图标, 2-默认VIDEO图标, 3-默认AUDIO图标, 20-默认OTHER图标
        public int PreviewImgType { 
            get 
            {
                if (null != PreviewImg && null != PreviewImg.Data && PreviewImg.Data.Length > 0) return 0;
                return (int)ContentType;
            }
        }
        /// 表示文件是否正在复制, 防止重复点击
        public bool IsCopying = false;


        //项目左键点击命令
        public ICommand ItemLeftClickCommand { get; set; }
        //项目右键点击命令
        public ICommand ItemRightClickCommand { get; set; }
        //删除按下命令
        public ICommand ItemDeleteKeyCommand { get; set; }
        //项目双击事件
        //public ICommand ItemDoubleClickCommand;

        //上级文件列表
        public FileItemsListViewModel FileItemsListViewModel { get; set; }
        //设备同步主viewModel
        public DeviceSyncViewModel DeviceSyncViewModel { get; set; }

        public FileItemViewModel ()
        {
            ItemLeftClickCommand = new RelayCommand(ImgItemLeftClick);
            ItemRightClickCommand = new RelayCommand(ImgItemRightClick);
            ItemDeleteKeyCommand = new RelayCommand(SubItemOnDeleteKey);
            //ItemDoubleClickCommand = new RelayCommand(() => { });
        }

        /// <summary>
        /// 具体同步项右击事件, 移除项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgItemRightClick()
        {
            if (DeviceSyncViewModel.IsFileSyncing)
            {
                //正在同步时不允许删除项
                return;
            }
            //var subListView = XamlUtil.GetParentListView(e);
            ////根据触发按钮获取点击的行
            //var selected = ((ListViewItem)subListView.ContainerFromElement(sender as StackPanel)).Content;
            //var ImgItemList = (ObservableCollection<DeviceSyncItem>)subListView.ItemsSource;
            FileItemsListViewModel.FileItems.Remove(this);
        }

        /// <summary>
        /// 项目双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgItemLeftClick()
        {
            //var subListView = XamlUtil.GetParentListView(e);
            ////根据触发按钮获取点击的行
            //var selected = (DeviceSyncItem)((ListViewItem)subListView.ContainerFromElement(sender as StackPanel)).Content;
            if (FileItemsListViewModel.SyncDeviceType == SyncDeviceType.PC)
            {
                //如果文件存在于PC, 则打开文件
                System.Diagnostics.Process.Start(Path.Combine(FileItemsListViewModel.FolderPath, Name));
            }
            else
            {
                //不允许重复调用
                if (IsCopying) return;

                //读取系统的临时文件夹
                var winTempFolder = Path.GetTempPath();
                if (null == winTempFolder)
                {
                    WindowUtil.ShowBubbleError("无法获取临时文件夹");
                    return;
                }
                var targetFullPath = Path.Combine(winTempFolder, Name);
                //如果已经存在于临时文件夹, 则直接打开
                if (System.IO.File.Exists(targetFullPath))
                {
                    System.Diagnostics.Process.Start(targetFullPath);
                    return;
                }

                //发送进度消息
                IsCopying = true;
                WindowUtil.ShowBubbleMessage("正在复制文件", AlertLevel.RUN);

                //先复制目标文件到临时文件夹, 再打开
                using (var device = DeviceSyncViewModel.SelectedDevice)
                {
                    device.Connect(enableCache: false);
                    var filePath = Path.Combine(DeviceSyncViewModel.SelectedDriver.ValueView, FileItemsListViewModel.FolderPath, Name);
                    MediaFileInfo fileInfo = device.GetFileInfo(filePath);
                    fileInfo.CopyTo(targetFullPath);
                    device.Disconnect();
                }
                IsCopying = false;
                WindowUtil.ShowBubbleInfo(string.Format("已复制到临时文件夹 {0}", targetFullPath));
                System.Diagnostics.Process.Start(targetFullPath);
            }
        }

        /// <summary>
        /// 子列表项目键盘事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubItemOnDeleteKey()
        {
            var selectedDevice = DeviceSyncViewModel.SelectedDevice;
            var selectedDriver = DeviceSyncViewModel.SelectedDriver;
            if (FileItemsListViewModel.SyncDeviceType== SyncDeviceType.PHONE && (null == selectedDevice || null == selectedDriver))
                return;

            //var confirmDialog = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, Name));
            //确认删除
            //if (confirmDialog.ShowDialog().GetValueOrDefault())
            //{
            //从电脑删除
            if (FileItemsListViewModel.SyncDeviceType == SyncDeviceType.PC)
            {
                FileUtil.DeleteFile(Path.Combine(FileItemsListViewModel.FolderPath, Name));
                //从vm中移除对象
                FileItemsListViewModel.FileItems.Remove(this);
                WindowUtil.ShowBubbleInfo(string.Format("文件[{0}]已删除", Name));
            }
            else
            {
                //从手机删除
                try
                {
                    using (selectedDevice)
                    {
                        if (!selectedDevice.IsConnected)
                        {
                            //selectedDevice.Connect(MediaDeviceAccess.GenericRead, MediaDeviceShare.Default, false);
                            selectedDevice.Connect(enableCache: false);
                        }
                        var targetFile = Path.Combine(selectedDriver.ValueView, FileItemsListViewModel.FolderPath, Name);
                        if (selectedDevice.FileExists(targetFile))
                        {
                            selectedDevice.DeleteFile(targetFile);
                        }
                        if (selectedDevice.IsConnected)
                        {
                            selectedDevice.Disconnect();
                        }
                        //从vm中移除对象
                        FileItemsListViewModel.FileItems.Remove(this);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    WindowUtil.ShowBubbleError(string.Format("[{0}]删除失败: [{1}]", Name, ex.Message));
                }
            }
            //}
        }
    }
}
