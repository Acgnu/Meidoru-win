using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// 对话框的viewModel
    /// </summary>
    public class EditDnsRecordDialogViewModel : ViewModelBase
    {
        //类型选框
        public List<ListBoxItem> TypeCombos { get; set; } = new List<ListBoxItem>()
            {
                new ListBoxItem() {Content="A"},
                new ListBoxItem() {Content="CNAME"},
                new ListBoxItem() {Content="TXT"},
                new ListBoxItem() {Content="MX"},
                new ListBoxItem() {Content="NS"}
            };
        //当前选中的类型
        private ListBoxItem _SelectedType = null;
        public ListBoxItem SelectedType
        {
            get { return _SelectedType; }
            set { _SelectedType = value; RaisePropertyChanged(); }
        }
        //是否忙
        private bool _IsBusy = false;
        public bool IsBusy
        {
            get { return !_IsBusy; }
            set { _IsBusy = value; RaisePropertyChanged(); }
        }
        //DNS解析记录
        public DnsItemViewModel DnsItem { get; set; }
        private readonly Action CloseAction;
        private readonly Action LoadAction;
        //保存命令
        public ICommand OnSaveComand { get; set; }

        public EditDnsRecordDialogViewModel(DnsItemViewModel dnsItem, Action closeAction, Action loadAction)
        {
            DnsItem = dnsItem;
            OnSaveComand = new RelayCommand(OnItemSave);
            CloseAction = closeAction;
            LoadAction = loadAction;
            //选中正在编辑的dns类型
            var preSelected = TypeCombos.Where((i) => i.Content.Equals(DnsItem.Type)).FirstOrDefault();
            SelectedType = preSelected ?? TypeCombos[0];
        }

        /// <summary>
        /// 保存事件
        /// </summary>
        private async void OnItemSave()
        {
            IsBusy = true;
            DnsItem.Type = SelectedType.Content as string;
            var result = await DnsItem.SaveOrModify();
            IsBusy = false;
            if (result.code != 0)
            {
                //错误
                Console.WriteLine(result.message);
                return;
            }
            LoadAction.Invoke();
            CloseAction.Invoke();
        }
    }
}
