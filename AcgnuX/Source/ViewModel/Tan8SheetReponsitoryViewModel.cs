using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedLib.Model;
using SharedLib.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 乐谱管理ViewModel
    /// </summary>
    public class Tan8SheetReponsitoryViewModel : ObservableObject
    {
        //乐谱列表对象
        public ObservableCollection<SheetItemViewModel> ListData { get; set; } = new ObservableCollection<SheetItemViewModel>();
        //选中的列表对象
        private SheetItemViewModel _SelectedListData;
        public SheetItemViewModel SelectedListData { get => _SelectedListData; set => SetProperty(ref _SelectedListData, value); }
        //是否忙
        private bool _IsBusy = false;
        public bool IsBusy { get => _IsBusy; set => SetProperty(ref _IsBusy, value); }
        //是否没有数据
        public bool IsEmpty { get => ListData.Count == 0; set => OnPropertyChanged(); }
        //过滤文本
        public string FilterText { get; set; }

        //刷新命令
        public ICommand OnRefreshCommand { get; }

        //乐谱库数据库
        private readonly Tan8SheetsRepo _Tan8SheetRepo;

        public Tan8SheetReponsitoryViewModel(Tan8SheetsRepo tan8SheetsRepo)
        {
            _Tan8SheetRepo = tan8SheetsRepo;
            OnRefreshCommand = new RelayCommand(Load);
        }

        /// <summary>
        /// 加载所有曲谱
        /// </summary>
        /// <param name="kw">查询关键字</param>
        public async void Load()
        {
            IsBusy = true;
            var dataList = await LoadAsync(FilterText);
            //刷新则清空所有记录后重新添加
            ListData.Clear();
            if (DataUtil.IsEmptyCollection(dataList))
            {
                IsEmpty = true;
                IsBusy = false;
                return;
            }
            dataList.ForEach(e => ListData.Add(CreateViewInstance(e)));
            IsEmpty = false;
            IsBusy = false;
        }

        /// <summary>
        /// 异步加载数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private async Task<List<Tan8Sheet>> LoadAsync(string keyword)
        {
            return await Task.Run(() =>
            {
                var pageSize = string.IsNullOrEmpty(keyword) ? 100 : 300;
                return _Tan8SheetRepo.Find(keyword, 1, pageSize);
            });
        }


        /// <summary>
        /// 根据乐谱创建view实例
        /// </summary>
        /// <param name="tan8Sheet"></param>
        /// <returns></returns>
        private SheetItemViewModel CreateViewInstance(Tan8Sheet tan8Sheet)
        {
            var yuepuPath = FileUtil.GetTan8YuepuFolder(Settings.Default.Tan8HomeDir, Convert.ToString(tan8Sheet.Ypid));
            var imgDir = Path.Combine(yuepuPath, ApplicationConstant.DEFAULT_COVER_NAME);
            return new SheetItemViewModel()
            {
                //对于不存在cover的路径使用默认图片
                Cover = File.Exists(imgDir) ? imgDir : null,
                //Cover = imgDir,
                Name = tan8Sheet.Name,
                Id = tan8Sheet.Ypid,
                Ver = tan8Sheet.Ver,
                Star = tan8Sheet.Star,
                YpCount = tan8Sheet.YpCount
            };
        }

        /// <summary>
        /// 删除条目
        /// </summary>
        /// <param name="itemVm"></param>
        public void DeleteItem(SheetItemViewModel itemVm)
        {
            //释放文件资源
            ListData.Remove(itemVm);

            //删除文件夹
            if (!string.IsNullOrEmpty(itemVm.Name))
            {
                var yuepuParent = FileUtil.GetTan8YuepuParentFolder(Settings.Default.Tan8HomeDir, itemVm.Id.ToString());
                FileUtil.DeleteDirWithName(yuepuParent, itemVm.Id.ToString());
            }

            //删除数据库数据
            _Tan8SheetRepo.DeleteById(itemVm.Id);

            WindowUtil.ShowBubbleInfo(string.Format("[{0}] 已删除", itemVm.Name));

            //如果没有曲谱了, 则展示默认按钮
            if (DataUtil.IsEmptyCollection(ListData)) IsEmpty = true;
        }
    }
}
