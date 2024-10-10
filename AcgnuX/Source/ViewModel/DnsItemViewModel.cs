using AcgnuX.Source.Utils;
using AlidnsLib;
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
        public string Id { get; set; }
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

 
        public AlidnsClient _AlidnsClient { set; get; }


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
                result = await _AlidnsClient.CreateRecordAsync(unCommitName, unCommitType, Line, unCommitValue);
            }
            else
            {
                //修改
                result = await _AlidnsClient.ModifyRecordAsync(Id, unCommitName, unCommitType, Line, unCommitValue);
            }

            if (result.Code != null)
            {
                Console.WriteLine(result.Message);
                WindowUtil.ShowBubbleError(result.Message);
                return false;
            }
            return true;
        }
    }
}
