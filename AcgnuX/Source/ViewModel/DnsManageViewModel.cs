using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Utils;
using AlidnsLib;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// DNS管理界面VM
    /// </summary>
    public class DnsManageViewModel : ObservableObject
    {
        //DNS表格数据内容
        public ObservableCollection<DnsItemViewModel> GridData { get; set; } = new ObservableCollection<DnsItemViewModel>();
        //添加命令
        public ICommand OnAddCommand { get; set; }
        //腾讯dns调用对象
        public AlidnsClient _AlidnsClient { get; set; }
        //秘钥repo
        private readonly AppSecretKeyRepo _appSecretKeyRepo;
        //过滤文本
        private string filterText;
        public string FilterText { get => filterText; set { filterText = value; OnFilterTextChanged(); } }
        public ICollectionView _CollectionView;
        //是否忙
        private bool isBusy = false;
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value); }

        public DnsManageViewModel(AppSecretKeyRepo appSecretKeyRepo)
        {
            this._appSecretKeyRepo = appSecretKeyRepo;
            var appSecretKey = _appSecretKeyRepo.FindByPlatform("ali");
            if (null != appSecretKey)
            {
                _AlidnsClient = new AlidnsClient(appSecretKey.SecretId, appSecretKey.SecretKey, appSecretKey.PrivDomain, true);
            }
        }

        /// <summary>
        /// 重新读取数据
        /// </summary>
        public async void Load(bool reload)
        {
            if (null != _CollectionView)
            {
                if (!reload) return;

                GridData.Clear();
                _CollectionView.Refresh();
            }

            if (null == _AlidnsClient) return;
            IsBusy = true;

            var response = await _AlidnsClient.QueryRecordsAsync();
            if (null != response)
            {
                foreach (var item in response.DomainRecords.Record)
                {
                    GridData.Add(new DnsItemViewModel()
                    {
                        Id = item.RecordId,
                        Ttl = item.TTL,
                        Value = item.Value,
                        Enabled = item.Status.Equals("ENABLE") ? 1 : 0,
                        Updated_on = DateTimeOffset.FromUnixTimeMilliseconds(item.UpdateTimestamp).ToString("F"),
                        Name = item.RR,
                        Line = item.Line,
                        Type = item.Type,
                        _AlidnsClient = _AlidnsClient
                    });
                }
                _CollectionView = CollectionViewSource.GetDefaultView(GridData);
            }
            IsBusy = false;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        private void OnFilterTextChanged()
        {
            if (null == _CollectionView)
            {
                return;
            }
            if (string.IsNullOrEmpty(FilterText) && _CollectionView.Filter != null)
            {
                _CollectionView.Filter = null;
            }
            _CollectionView.Filter = new Predicate<object>((item) =>
            {
                var itemVm = item as DnsItemViewModel;
                return itemVm.Name.Contains(FilterText) || itemVm.Value.Contains(FilterText);
            });
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="selected"></param>
        internal async void DeleteItem(DnsItemViewModel selected)
        {
            IsBusy = true;
            var delResult = await _AlidnsClient.DeleteRecordAsync(selected.Id);
            if (delResult.Code != null)
            {
                WindowUtil.ShowBubbleError(delResult.Message);
                IsBusy = false;
                return;
            }
            GridData.Remove(selected);
            IsBusy = false;
        }
    }
}
