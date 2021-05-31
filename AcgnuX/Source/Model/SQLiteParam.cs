using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// sqllite参数
    /// </summary>
    public class SQLiteParam
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; set; }
    }
}
