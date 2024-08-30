using AcgnuX.Bussiness.Ten.Dns;
using AcgnuX.Model.Ten.Dns;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Bussiness.Ten.Dns;
using AcgnuX.Source.Model;
using AcgnuX.Source.Model.Ten.Dns;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// DNS管理界面VM
    /// </summary>
    public class DnsManageViewModel : ViewModelBase
    {
        //DNS表格数据内容
        public ObservableCollection<DnsItemViewModel> GridData { get; set; } = new ObservableCollection<DnsItemViewModel>();
        //刷新按钮命令
        public ICommand OnRefreshCommand { get; set; }
        //添加命令
        public ICommand OnAddCommand { get; set; }
        //腾讯dns调用对象
        public TenCloudDns TenDnsClient { get; set; }
        //秘钥repo
        private readonly AppSecretKeyRepo _appSecretKeyRepo = AppSecretKeyRepo.Instance;
        //过滤文本
        public string FilterText { get; set; }
        //是否忙
        private bool _IsBusy = true;
        public bool IsBusy 
        { 
            get { return _IsBusy; }
            set { _IsBusy = value; RaisePropertyChanged(); }
        }

        public DnsManageViewModel()
        {
            OnRefreshCommand = new RelayCommand(OnRefreshClick);

            var appSecretKey = _appSecretKeyRepo.FindByPlatform("tencent");
            TenDnsClient = new TenCloudDns(appSecretKey);
            LoadDnsRecord();
        }

        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        public void OnRefreshClick()
        {
            LoadDnsRecord();
        }

        /// <summary>
        /// 重新读取数据
        /// </summary>
        private async void LoadDnsRecord()
        {
            if (null == TenDnsClient) return;
            IsBusy = true;

            var response = await TenDnsClient.QueryRecordsAsync<DnsRecordResult>(null, null);
            if (null != response)
            {
                GridData.Clear();
                foreach (var item in response.data.records)
                {
                    if (!string.IsNullOrEmpty(FilterText) && !item.Name.Contains(FilterText) && !item.Value.Contains(FilterText))
                    {
                        //过滤
                        continue;
                    }
                    GridData.Add(
                                   new DnsItemViewModel()
                                   {
                                       Id = item.id,
                                       Ttl = item.ttl,
                                       Value = item.Value,
                                       Enabled = item.enabled,
                                       Updated_on = item.updated_on,
                                       Name = item.Name,
                                       Line = item.line,
                                       Type = item.type,
                                       TenDnsClient = TenDnsClient
                                   });
                }
            }
            IsBusy = false;
        }
    }
}
