using System.ComponentModel;
using System.Runtime.Serialization;

namespace AcgnuX.Source.Bussiness.Constants
{
    /// <summary>
    /// 信息提示级别
    /// </summary>
    public enum AlertLevel
    {
        //简单信息提示
        [EnumMember]
        [Description("infomation")]
        INFO = 1,

        //标识有正在执行的任务
        [EnumMember]
        [Description("running")]
        RUN = 2,

        //警告
        [EnumMember]
        [Description("warning")]
        WARN = 3,

        //错误
        [EnumMember]
        [Description("error")]
        ERROR = 4,
    }
}