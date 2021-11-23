using System.ComponentModel;
using System.Runtime.Serialization;

namespace AcgnuX.Source.Bussiness.Constants
{
    /// <summary>
    /// 同步文件类型
    /// </summary>
    public enum SyncContentType
    {
        [EnumMember]
        [Description("图片")]
        IMAGE = 1,

        [EnumMember]
        [Description("视频")]
        VIDEO = 2,

        [EnumMember]
        [Description("音频")]
        AUDIO = 3,

        [EnumMember]
        [Description("其他")]
        OTHER = 20,
    }
}