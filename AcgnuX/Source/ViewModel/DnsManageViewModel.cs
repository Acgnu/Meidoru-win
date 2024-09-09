using AcgnuX.Bussiness.Ten.Dns;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Bussiness.Ten.Dns;
using AcgnuX.Source.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
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
        public TenCloudDns TenDnsClient { get; set; }
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
            var appSecretKey = _appSecretKeyRepo.FindByPlatform("tencent");
            if (null != appSecretKey)
            {
                TenDnsClient = new TenCloudDns(appSecretKey);
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

            if (null == TenDnsClient) return;
            IsBusy = true;

            var response = await TenDnsClient.QueryRecordsAsync(null, null);
            if (null != response)
            {
                foreach (var item in response.data.records)
                {
                    GridData.Add(new DnsItemViewModel()
                    {
                        Id = item.id,
                        Ttl = item.ttl,
                        Value = item.Value,
                        Enabled = item.enabled,
                        Updated_on = item.updated_on,
                        Name = item.Name,
                        Line = item.line,
                        Type = item.type,
                        _TenDnsClient = TenDnsClient
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
            var delResult = await TenDnsClient.DeleteRecordAsync(selected.Id.GetValueOrDefault());
            if (delResult.code != 0)
            {
                WindowUtil.ShowBubbleError(delResult.message);
                IsBusy = false;
                return;
            }
            GridData.Remove(selected);
            IsBusy = false;
        }
    }
}
