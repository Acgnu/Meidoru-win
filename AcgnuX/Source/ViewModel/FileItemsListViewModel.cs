using AcgnuX.Source.Bussiness;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 文件列表视图
    /// </summary>
    public class FileItemsListViewModel : ObservableObject
    {
        //目标设备
        public SyncDeviceType SyncDeviceType { get; set; }
        //路径
        public string FolderPath { get; set; }
        //文件
        public ObservableCollection<FileItemViewModel> FileItems { get; set; } = new ObservableCollection<FileItemViewModel>();

        public FileItemsListViewModel()
        {

        }
    }
}
