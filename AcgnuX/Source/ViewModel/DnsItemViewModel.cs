using AcgnuX.Bussiness.Ten.Dns;
using AcgnuX.Source.Model.Ten.Dns;
using AcgnuX.Source.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// DNS管理的DNS条目VM
    /// </summary>
    public class DnsItemViewModel : ObservableObject
    {
        public int? Id { get; set; }
        public int Ttl { get; set; }
        public int Enabled { get; set; }
        public string Status { get; set; }
        public string Updated_on { get; set; }

        private string _value;
        public string Value { get => _value; set => SetProperty(ref _value, value); }

        private string name;
        public string Name { get => name; set => SetProperty(ref name, value); }

        private string line = "默认";
        public string Line { get => line; set => SetProperty(ref line, value); }

        private string type = "A";
        public string Type { get => type; set => SetProperty(ref type, value); }

 
        public TenCloudDns _TenDnsClient { set; get; }


        /// <summary>
        /// 保存或修改DNS记录
        /// </summary>
        /// <param name="unCommitName"></param>
        /// <param name="unCommitValue"></param>
        /// <param name="unCommitType"></param>
        /// <returns></returns>
        internal async Task<bool> Save(string unCommitName, string unCommitValue, string unCommitType)
        {
            DnsOperatorResult result;
            if (Id == null)
            {
                //新增
                result = await _TenDnsClient.CreateRecordAsync(unCommitName, unCommitType, Line, unCommitValue);
            }
            else
            {
                //修改
                result = await _TenDnsClient.ModifyRecordAsync(Id.GetValueOrDefault(), unCommitName, unCommitType, Line, unCommitValue);
            }

            if (result.code != 0)
            {
                WindowUtil.ShowBubbleError(result.message);
                return false;
            }
            return true;
        }
    }
}
