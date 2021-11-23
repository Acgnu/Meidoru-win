using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.Constants
{
    class WindowsMessage
    {
        //Windows消息 - 串口变动
        public const int WM_DEVICECHANGE = 0x219;
        //Win 消息 - 设备插入
        public const int DBT_DEVICEARRIVAL = 0x8000;
        //Win 消息 - 设备卸载
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    }
}
