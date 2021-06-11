using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.Constants
{
    /// <summary>
    /// 翻页枚举
    /// </summary>
    enum PageAction : short
    {
        //上一页
        PREVIOUS = -1,
        //当前页
        CURRENT = 0,
        //下一页
        NEXT = 1
    }
}
