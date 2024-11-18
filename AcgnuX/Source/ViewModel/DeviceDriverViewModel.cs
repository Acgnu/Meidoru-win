using CommunityToolkit.Mvvm.ComponentModel;

namespace AcgnuX.ViewModel
{
    public class DeviceDriverViewModel : ObservableObject
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
