using System.ComponentModel;
using System.Runtime.Serialization;

namespace AcgnuX.Source.Bussiness.Constants
{
    /// <summary>
    /// 
    /// </summary>
    public enum SyncTaskProgressType : byte
    {
        //子项目完成
        SUB_ITEM_FINISH = 1,
        //队列完成
        QUEUE_FINISH = 2
    }
}