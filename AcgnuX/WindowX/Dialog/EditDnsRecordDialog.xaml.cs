using AcgnuX.Source.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑DNS的弹窗
    /// </summary>
    public partial class EditDnsRecordDialog : BaseDialog
    {
        //内容vm
        public DnsItemViewModel ItemViewModel { get; }

        public EditDnsRecordDialog(DnsItemViewModel itemVm)
        {
            InitializeComponent();
            InitCommboBox();
            ItemViewModel = itemVm;
            //窗口vm
            DataContext = this;
            FormStackPanel.BindingGroup.BeginEdit();
        }

        /// <summary>
        /// 初始化下拉框
        /// </summary>
        private void InitCommboBox()
        {
            TypeListBox.ItemsSource = new string[]
            {
                "A",
                "CNAME",
                "TXT",
                "MX",
                "NS"
            };
        }

        /// <summary>
        /// 点击保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickSaveButton(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;

            var gp = FormStackPanel.BindingGroup;
            var unCommitName = gp.GetValue(ItemViewModel, "Name") as string;
            var unCommitValue = gp.GetValue(ItemViewModel, "Value") as string;
            var unCommitType = gp.GetValue(ItemViewModel, "Type") as string;

            //没有调用gp.CommitEdit() 时 ItemViewModel 内的值就还没有发生变化, 只能通过 gp.GetValue 获取编辑的值
            var isOk = await ItemViewModel.Save(unCommitName, unCommitValue, unCommitType);
            if (!isOk)
            {
                button.IsEnabled = true;
                return;
            }
            //只有保存成功之后才调用 CommitEdit(), 来变更 ItemViewModel 此时会同步更新列表中的项
            FormStackPanel.BindingGroup.CommitEdit();
            AnimateClose((s, a) => DialogResult = true);
        }
    }
}