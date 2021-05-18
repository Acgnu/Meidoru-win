using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Utils;
using AcgnuX.WindowX.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 曲谱下载记录
    /// </summary>
    public class PianoScoreDownloadRecordViewModel : CommonWindowViewModel
    {
        //历史纪录列表数据对象
        public ObservableCollection<Tan8SheetDownloadRecord> DownloadRecordList { get; set; } = new ObservableCollection<Tan8SheetDownloadRecord>();
        //筛选框对象
        public ObservableCollection<PianoScoreDownloadRecordFilterBox> FilterBoxList { get; set; } = new ObservableCollection<PianoScoreDownloadRecordFilterBox>()
        {
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(Tan8SheetDownloadResult), Tan8SheetDownloadResult.SUCCESS.ToString()), Convert.ToByte(Tan8SheetDownloadResult.SUCCESS)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(Tan8SheetDownloadResult), Tan8SheetDownloadResult.PLAYER_NO_RESPONSE.ToString()), Convert.ToByte(Tan8SheetDownloadResult.PLAYER_NO_RESPONSE)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(Tan8SheetDownloadResult), Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS.ToString()), Convert.ToByte(Tan8SheetDownloadResult.PIANO_SCORE_NOT_EXSITS)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(Tan8SheetDownloadResult), Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL.ToString()), Convert.ToByte(Tan8SheetDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(Tan8SheetDownloadResult), Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL.ToString()), Convert.ToByte(Tan8SheetDownloadResult.PLAY_FILE_DOWNLOAD_FAIL))
        };

        public PianoScoreDownloadRecordViewModel() : base()
        {
        }
    }

    public class PianoScoreDownloadRecordFilterBox
    {
        //筛选标题
        public string Title { get; set; }
        //筛选值
        public byte Value { get; set; }
        //选中状态
        public bool IsChecked { get; set; } = true;
        public PianoScoreDownloadRecordFilterBox(string title, byte value)
        {
            Title = title;
            Value = value;
        }
    }
}
