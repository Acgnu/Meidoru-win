using System.ComponentModel;
using System.Runtime.Serialization;

namespace AcgnuX.Source.Bussiness.Constants
{
    /// <summary>
    /// tan8曲谱下载结果
    /// </summary>
    public enum Tan8SheetDownloadResult
    {
        [EnumMember]
        [Description("下载成功")]
        SUCCESS = 0,

        [EnumMember]
        [Description("没有返回乐谱信息")]
        PLAYER_NO_RESPONSE = 1,

        [EnumMember]
        [Description("该乐谱不存在")]
        PIANO_SCORE_NOT_EXSITS = 2,

        [EnumMember]
        [Description("访问过于频繁")]
        TOO_MANY_VISIT = 3,

        [EnumMember]
        [Description("封面下载失败")]
        COVER_DOWNLOAD_FAIL = 4,

        [EnumMember]
        [Description("谱页下载失败")]
        PIANO_SCORE_DOWNLOAD_FAIL = 5,

        [EnumMember]
        [Description("播放文件下载失败")]
        PLAY_FILE_DOWNLOAD_FAIL = 6,

        [EnumMember]
        [Description("访问次数达到当天限制")]
        VISTI_REACH_LIMIT = 7,

        [EnumMember]
        [Description("网络连接出错")]
        NETWORK_ERROR = 8,

        [EnumMember]
        [Description("验证错误")]
        VALID_ERROR = 9,

        [EnumMember]
        [Description("没有乐谱页")]
        NO_SHEETS = 10
    }
}