using AcgnuX.Bussiness.Ten.Dns;
using AcgnuX.Source.Model.Ten.Dns;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// DNS管理的DNS条目VM
    /// </summary>
    public class DnsItemViewModel : ViewModelBase
    {
        public int? Id { get; set; }
        public int Ttl { get; set; }
        public string Value { get; set; }
        public int Enabled { get; set; }
        public string Status { get; set; }
        public string Updated_on { get; set; }
        public int Q_project_id { get; set; }
        public string Name { get; set; }
        public string Line { get; set; }
        public string Line_id { get; set; }
        public string Type { get; set; }
        public string Remark { get; set; }
        public int Mx { get; set; }
        public string Hold { get; set; }
        public TenCloudDns TenDnsClient { get; set; }

        /// <summary>
        /// 新增或更新
        /// </summary>
        public async Task<DnsOperatorResult> SaveOrModify()
        {
            DnsOperatorResult result;
            if (Id == null)
            {
                //新增
                var response = await TenDnsClient.CreateRecordAsync(Name, Type, Line, Value);
                result = response.Data;
            }
            else
            {
                //修改
                var response = await TenDnsClient.ModifyRecordAsync(Id.GetValueOrDefault(), Name, Type, Line, Value);
                result = response.Data;
            }
            return result;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public async Task<DnsOperatorResult> Delete()
        {
            var response = await TenDnsClient.DeleteRecordAsync(Convert.ToString(Id.GetValueOrDefault()));
            return response.Data;
        }
    }
}
