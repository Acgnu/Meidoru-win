using System;
using System.Windows.Media;

namespace AcgnuX.Source.Model
{
    /// <summary>
    /// 主导航菜单
    /// </summary>
    public class NavMenu
    {
        //菜单名称
        public string name { get; set; }
        //启动的page类型
        public Type pageType { get; set; }
        //图标
        public Geometry icon { get; set; }
        //暂存的page实例
        public Object instance { get; set; }
    }
}
