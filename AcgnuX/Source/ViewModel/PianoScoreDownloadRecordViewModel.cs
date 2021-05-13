using AcgnuX.Source.Bussiness.Constants;
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
        public ObservableCollection<PianoScoreDownloadRecord> DownloadRecordList { get; set; } = new ObservableCollection<PianoScoreDownloadRecord>();
        //筛选框对象
        public ObservableCollection<PianoScoreDownloadRecordFilterBox> FilterBoxList { get; set; } = new ObservableCollection<PianoScoreDownloadRecordFilterBox>()
        {
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), PianoScoreDownloadResult.SUCCESS.ToString()), Convert.ToByte(PianoScoreDownloadResult.SUCCESS)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), PianoScoreDownloadResult.PLAYER_NO_RESPONSE.ToString()), Convert.ToByte(PianoScoreDownloadResult.PLAYER_NO_RESPONSE)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), PianoScoreDownloadResult.PIANO_SCORE_NOT_EXSITS.ToString()), Convert.ToByte(PianoScoreDownloadResult.PIANO_SCORE_NOT_EXSITS)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), PianoScoreDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL.ToString()), Convert.ToByte(PianoScoreDownloadResult.PIANO_SCORE_DOWNLOAD_FAIL)),
            new PianoScoreDownloadRecordFilterBox(EnumLoader.GetEnumDesc(typeof(PianoScoreDownloadResult), PianoScoreDownloadResult.PLAY_FILE_DOWNLOAD_FAIL.ToString()), Convert.ToByte(PianoScoreDownloadResult.PLAY_FILE_DOWNLOAD_FAIL))
        };

        public PianoScoreDownloadRecordViewModel(BaseDialog window) : base(window)
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

    public class PianoScoreDownloadRecord
    {
        //记录id
        public int Id { get; set; }
        //下载的乐谱id
        public int Ypid { get; set; }
        //乐谱名称
        public string Name { get; set; }
        //记录时间
        public string Create_time { get; set; }
        //下载结果
        public string Result { get; set; }
    }
}
