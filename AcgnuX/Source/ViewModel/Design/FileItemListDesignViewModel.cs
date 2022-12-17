using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.ViewModel.Design
{
    public class FileItemListDesignViewModel : FileItemsListViewModel
    {

        public FileItemListDesignViewModel()
        {
            FileItems = new System.Collections.ObjectModel.ObservableCollection<FileItemViewModel>
            {
                new FileItemViewModel
                {
                    Name = "设计文件001.jpg"
                },
                new FileItemViewModel
                {
                    Name = "设计文件002.txt"
                },
                new FileItemViewModel
                {
                    Name = "设计文件003xxxfxfxfxfxxffxxfxfxfxfxxfxfxf.mp4"
                },
                new FileItemViewModel
                {
                    Name = "设计文件004.docx"
                },
                new FileItemViewModel
                {
                    Name = "设计文件004.mp3"
                },
            };
        }
    }
}
