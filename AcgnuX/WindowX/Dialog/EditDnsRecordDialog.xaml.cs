using AcgnuX.Model.Ten.Dns;
using AcgnuX.Pages.Apis.Ten.Dns;
using AcgnuX.Source.Model.Ten.Dns;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑DNS的弹窗
    /// </summary>
    public partial class EditDnsRecordDialog : BaseDialog
    {
        //DNS解析记录
        public DnsRecord MdnsRecord { get; set; }
        //父Page
        protected DnsRecords mParentPage;

        public EditDnsRecordDialog(DnsRecord dnsRecord, DnsRecords dnsRecords)
        {
            InitializeComponent();
            //初始化父页面
            mParentPage = dnsRecords;
            //当前正在修改的解析记录
            MdnsRecord = dnsRecord;
            //保存完成回调
            dnsRecords.OnSaveFinish += SaveFinishCallBack;
            DataContext = this;
            //选中正在编辑的dns类型
            if(null != dnsRecord)
            {
                var listBoxItems = DnsTypeListBox.Items;
                for (var i = 0; i < listBoxItems.Count; i++)
                {
                    if (((ListBoxItem)listBoxItems[i]).Content.Equals(dnsRecord.type))
                    {
                        DnsTypeListBox.SelectedIndex = i;
                    }
                }
            }
        }

        /// <summary>
        /// 确认点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;

            var dnsRecord = GetDnsRecordParams();
            mParentPage.SaveDnsRecord(dnsRecord);
        }

        /// <summary>
        /// 获取dns记录对象
        /// </summary>
        /// <returns></returns>
        private DnsRecord GetDnsRecordParams()
        {
            var dnsType = DnsTypeListBox.SelectedItem as ListBoxItem;
            var dnsRecord = new DnsRecord
            {
                id = MdnsRecord?.id,
                Name = TextBlockDomain.Text,
                type = null == dnsType ? "A" : Convert.ToString(dnsType.Content),
                line = "默认",
                Value = TextBlockValue.Text
            };
            return dnsRecord;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="button"></param>
        /// <param name="enable"></param>
        private void SaveFinishCallBack(DnsOperatorResult result)
        {
            //ConfirmButton.Dispatcher.Invoke(new Action<DependencyProperty, object>(ConfirmButton.SetValue), DispatcherPriority.Background, Button.IsEnabledProperty, true);
            this.Dispatcher.Invoke(() => {
                ConfirmButton.IsEnabled = true;
                this.Close();
            });
        }
    }
}