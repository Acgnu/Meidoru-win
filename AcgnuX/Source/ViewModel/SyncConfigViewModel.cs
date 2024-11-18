using CommunityToolkit.Mvvm.ComponentModel;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 同步配置视图模型
    /// </summary>
    public class SyncConfigViewModel : ObservableObject
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// PC路径
        /// </summary>
        private string _PcPath;
        public string PcPath { get => _PcPath; set => SetProperty(ref _PcPath, value); }
        /// <summary>
        /// 移动端路径
        /// </summary>
        private string _MobilePath;
        public string MobilePath { get => _MobilePath; set => SetProperty(ref _MobilePath, value); }
        /// <summary>
        /// 是否启用 1 是 0 否
        /// </summary>
        private bool _Enable;
        public bool Enable { get => _Enable; set => SetProperty(ref _Enable, value); }
    }
}
