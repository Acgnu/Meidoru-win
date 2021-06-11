using AcgnuX.Source.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AcgnuX.Pages.MobileSync
{
    /// <summary>
    /// SyncPictures.xaml 的交互逻辑
    /// </summary>
    public partial class SyncPictures : Page
    {
        //列表数据对象
        private ObservableCollection<DeviceSyncListViewModel> mSyncDataList = new ObservableCollection<DeviceSyncListViewModel>();
        public SyncPictures()
        {
            InitializeComponent();
        }

        private void OnPageLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ObservableCollection<DeviceSyncListImgViewItem> ImgItemList = new ObservableCollection<DeviceSyncListImgViewItem>();
            for (int i = 0; i < 10; i++)
            {
                ImgItemList.Add(
                new DeviceSyncListImgViewItem()
                {
                    ImgNameAndView = "图片" + i,
                    BitImg = new BitmapImage(new Uri("/Assets/Images/piano-cover-default.jpg", UriKind.Relative))
                });
            }


            for (int i = 0; i < 3; i++)
            {
                mSyncDataList.Add(
                new DeviceSyncListViewModel
                {
                    FolderNameAndView = "这个只是一个文件目录" + i,
                    ImgItemList = ImgItemList
                }
              );
            }

            DeviceSyncListBox.ItemsSource = mSyncDataList;
        }
    }
}
