using GalaSoft.MvvmLight;
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
    public class FileItemsListViewModel : ViewModelBase
    {
        //文件
        public ObservableCollection<FileItemViewModel> FileItems { get; set; } = new ObservableCollection<FileItemViewModel>();

        public FileItemsListViewModel()
        {

        }
    }
}
