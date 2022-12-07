using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 曲谱下载记录
    /// </summary>
    public class PianoScoreDownloadRecordViewModel : ViewModelBase
    {
        //历史纪录列表数据对象
        public ObservableCollection<Tan8SheetDownloadRecord> DownloadRecordList { get; set; } = new ObservableCollection<Tan8SheetDownloadRecord>();
        //筛选框对象
        public ObservableCollection<PianoScoreDownloadRecordFilterBox> FilterBoxList { get; set; } = new ObservableCollection<PianoScoreDownloadRecordFilterBox>()
        {
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.SUCCESS), Convert.ToByte(Tan8SheetDownloadResult.SUCCESS)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.PLAYER_NO_RESPONSE), Convert.ToByte(Tan8SheetDownloadResult.PLAYER_NO_RESPONSE)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS), Convert.ToByte(Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL), Convert.ToByte(Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetDesc(Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL), Convert.ToByte(Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL))
        };

        //删除条目
        public ICommand OnDeleteKeyCommand { get; set; }
        //复制增长id
        public ICommand OnCopyIncrIdCommand { get; set; }

        public readonly Tan8SheetCrawlRecordRepo _Tan8SheetCrawlRecordRepo = Tan8SheetCrawlRecordRepo.Instance;

        //标识条件复选框是否初始化完成
        //private bool IsCheckBoxInited = false;

        /// <summary>
        /// 启动下载事件
        /// </summary>
        public Action<Tan8SheetCrawlArg> _DownloadAction;

        public PianoScoreDownloadRecordViewModel()
        {
            OnDeleteKeyCommand = new RelayCommand<IList>(OnDelete);
            OnCopyIncrIdCommand = new RelayCommand<Tan8SheetDownloadRecord>(OnCopyIncrYpid);
            //IsCheckBoxInited = true;
        }

        /// <summary>
        /// 查询
        /// </summary>
        public void Load()
        {
            List<int> codes = new List<int>();
            foreach (var item in FilterBoxList)
            {
                if(item.IsChecked)
                    codes.Add(item.Value);
            }
            var data = _Tan8SheetCrawlRecordRepo.Find(codes);
            DownloadRecordList.Clear();
            if (!DataUtil.IsEmptyCollection(data))
            {
                data.ForEach(e => DownloadRecordList.Add(e));
            }
        }

        /// <summary>
        /// 删除乐谱下载记录
        /// </summary>
        private void OnDelete(IList selectedList)
        {
            //System.Windows.Controls.SelectedItemCollection
            List<int> selectedIds = new List<int>();
            while (selectedList.Count > 0)
            {
                var vm = selectedList[0] as Tan8SheetDownloadRecord;
                selectedIds.Add(vm.Id);
                DownloadRecordList.Remove(vm);
            }
            _Tan8SheetCrawlRecordRepo.DelByIds(selectedIds);
        }

        /// <summary>
        /// 以递增方式复制ID
        /// </summary>
        private void OnCopyIncrYpid(Tan8SheetDownloadRecord record)
        {
            //XamlUtil.SelectRow(DownloadRecordDataGrid, e);
            if (null == record) return;
            var arg = new Tan8SheetCrawlArg()
            {
                AutoDownload = true,
                Ypid = record.Ypid + 1,
                UseProxy = true,
                IsQueueTask = false,
                Ver = 2
            };
            _DownloadAction?.Invoke(arg);
        }

        /// <summary>
        /// 新的乐谱下载完成事件
        /// </summary>
        /// <param name="newItem"></param>
        public void OnSheetItemDownloadComplete(int newYpid)
        {
            var record = _Tan8SheetCrawlRecordRepo.FindByYpid(newYpid);
            DownloadRecordList.Insert(0, record);
        }
    }

    /// <summary>
    /// 筛选框View Model
    /// </summary>
    public class PianoScoreDownloadRecordFilterBox : ViewModelBase
    {
        //筛选标题
        public string Title { get; set; }
        //筛选值
        public byte Value { get; set; }
        //选中状态
        private bool _IsChecked = true;
        public bool IsChecked { get => _IsChecked; set { _IsChecked = value; RaisePropertyChanged(); } }

        public PianoScoreDownloadRecordFilterBox(string title, byte value)
        {
            Title = title;
            Value = value;
        }

        /// <summary>
        /// 过滤框点击
        /// </summary>
        //private void OnFilter(PianoScoreDownloadRecordViewModel listViewModel)
        //{
            //IsChecked = !IsChecked;
            //由于页面创建的时候就会触发选中事件, 此处手动限制, 只有在所有数据加载之后才会触发此事件
            //comment-on 2022-11-29, mvvm模式不会在初始化下触发
            //if (!IsCheckBoxInited) return;
            //listViewModel.Load();
        //}
    }
}
