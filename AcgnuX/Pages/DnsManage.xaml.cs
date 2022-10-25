using AcgnuX.Bussiness.Ten.Dns;
using AcgnuX.Model.Ten.Dns;
using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Ten.Dns;
using AcgnuX.Source.Model;
using AcgnuX.Source.Model.Ten.Dns;
using AcgnuX.Source.Utils;
using AcgnuX.WindowX.Dialog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcgnuX.Pages
{
    /// <summary>
    ///onOpenContextMenuxaml 的交互逻辑
    /// </summary>
    public partial class DnsManage : BasePage
    {
        //数据展示结果集
        private List<DnsRecord> DnsRecordList;
        //请求异步handle
        private RestRequestAsyncHandle AsyncHandle;
        //保存完成事件
        public event BackgroundFinishHandler OnSaveFinish;
        //腾讯dns调用对象
        private TenCloudDns mTenCloudDns;

        private event StatusBarNotifyHandler OnStatusBarEvent;

        public DnsManage(MainWindow mainWin)
        {
            InitializeComponent();
            mMainWindow = mainWin;
            OnStatusBarEvent += mMainWindow.SetStatustProgess;
        }

        /// <summary>
        /// 加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if(null == mTenCloudDns)
            {
                var dbSecret = SQLite.SqlRow("SELECT secret_id, secret_key, priv_domain, priv_sub_domain, Platform FROM app_secret_keys WHERE platform = 'tencent'");
                if (null != dbSecret && dbSecret.Length > 0)
                {
                    mTenCloudDns = new TenCloudDns(new TenDnsApiSecret
                    {
                        SecretId = dbSecret[0],
                        SecretKey = dbSecret[1],
                        PrivDomain = dbSecret[2],
                        PrivSubDomain = dbSecret[3],
                        Platform = dbSecret[4]
                    });
                }
            }
            if (null == DnsRecordList)
            {
                LoadDnsRecord();
            }
        }

        /// <summary>
        /// 重新读取数据
        /// </summary>
        private void LoadDnsRecord()
        {
            if (null == mTenCloudDns) return;
            BusyIndicator.IsBusy = true;
            //如果有未完成的请求, 先中止
            AsyncHandle?.Abort();

            AsyncHandle = mTenCloudDns.queryRecordList<DnsRecordResult>(null, null, response =>
            {
                Dispatcher.Invoke(() =>
                {
                    //更新表格数据
                    if(null != response.Data.data)
                    {
                        DnsRecordList = response.Data.data.records;
                        DnsRecordDataGrid.ItemsSource = DnsRecordList;
                        BusyIndicator.IsBusy = false;
                    }
                });
            });
        }

        /// <summary>
        /// 条件筛选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFilterBoxKeyDown(object sender, KeyEventArgs e)
        {
            var box = sender as TextBox;
            if (string.IsNullOrEmpty(box.Text))
            {
                DnsRecordDataGrid.ItemsSource = DnsRecordList;
            }
            else
            {
                DnsRecordDataGrid.ItemsSource = DnsRecordList.FindAll(item => item.Name.Contains(box.Text) || item.Value.Contains(box.Text));
            }
        }

        /// <summary>
        /// 添加按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAddClick(object sender, RoutedEventArgs e)
        {
            if(null == mTenCloudDns)
            {
                OnStatusBarEvent?.Invoke(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "未配置访问密钥"
                });
                //mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                //{
                //    alertLevel = AlertLevel.ERROR,
                //    message = "未配置访问密钥"
                //});
                return;
            }
            new EditDnsRecordDialog(null, this).ShowDialog();
        }

        /// <summary>
        /// 刷新按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickRefreshButton(object sender, RoutedEventArgs e)
        {
            LoadDnsRecord();
        }

        /// <summary>
        /// 右键删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickContextMenuDelete(object sender, MouseButtonEventArgs e)
        {
            XamlUtil.SelectRow(DnsRecordDataGrid, e);
            var selected = DnsRecordDataGrid.SelectedItem as DnsRecord;
            if (null == selected) return;
            //var result = new ConfirmDialog(AlertLevel.WARN, string.Format((string)Application.Current.FindResource("DeleteConfirm"), selected.Name)).ShowDialog();
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, selected.Name)).ShowDialog();
            if (result.GetValueOrDefault())
            {
                mTenCloudDns.DelDNSRecord(Convert.ToString(selected.id), response =>
                {
                    var resultData = response.Data;
                    LoadDnsRecord();
                });
            }
        }

        /// <summary>
        /// 表格内容双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGridDoubleClick(object sender, RoutedEventArgs e)
        {
            var selected = DnsRecordDataGrid.SelectedItem as DnsRecord;
            if (null == selected) return;
            new EditDnsRecordDialog(selected, this).ShowDialog();
        }

        /// <summary>
        /// 右键菜单展开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContextMenuOpen(object sender, ContextMenuEventArgs e)
        {
            //XamlUtil.SelectRow(DnsRecordDataGrid, e);
        }

        /// <summary>
        /// 保存DNS解析记录
        /// </summary>
        /// <param name="dnsRecord"></param>
        /// <param name="dnsDomain"></param>
        public void SaveDnsRecord(DnsRecord dnsRecord)
        {
            if (dnsRecord.id == null)
            {
                //新增
                var handle = mTenCloudDns.AddDNSRecord(dnsRecord, response =>
                {
                    EditFinishCallBack(response.Data);
                });
            }
            else
            {
                //修改
                var handle = mTenCloudDns.ModifyDNS(dnsRecord, response =>
                {
                    EditFinishCallBack(response.Data);
                });
            }
        }

        /// <summary>
        /// 保存DNS记录回调
        /// </summary>
        /// <param name="result"></param>
        private void EditFinishCallBack(DnsOperatorResult result)
        {
            //错误则显示错误信息
            if (result.code != 0)
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify() { 
                    alertLevel = AlertLevel.ERROR,
                    message = string.Format("[{0}]{1}", result.code, result.message)
                });;
                //win.SetStatusBarText(AlertLevel.ERROR, result.message);
            }
            else
            {
                LoadDnsRecord();
            }
            //触发完成事件
            OnSaveFinish?.Invoke(result);
        }
    }
}
