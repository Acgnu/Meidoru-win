using AcgnuX.Source.Utils;
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
            new PianoScoreDownloadRecordFilterBox("成功", 0),
            new PianoScoreDownloadRecordFilterBox("解析失败", 1),
            new PianoScoreDownloadRecordFilterBox("乐谱不存在", 2),
            new PianoScoreDownloadRecordFilterBox("乐谱页下载失败", 5),
            new PianoScoreDownloadRecordFilterBox("播放文件下载失败", 7)
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
