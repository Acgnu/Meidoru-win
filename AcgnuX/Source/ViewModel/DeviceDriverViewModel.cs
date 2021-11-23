using AcgnuX.Source.Model;
using System.ComponentModel;

namespace AcgnuX.ViewModel
{
    class DeviceDriverViewModel : BasePropertyChangeNotifyModel
    {
        /// <summary>
        /// 驱动值
        /// </summary>
        public string ValueView { get; set; }
        /// <summary>
        /// 驱动名称
        /// </summary>
        public string NameView { get; set; }
    }
}
