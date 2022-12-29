using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.ViewModel;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;

namespace AcgnuX.Pages
{
    /// <summary>
    /// PwdRepositroy.xaml 的交互逻辑
    /// </summary>
    public partial class ContactManage : BasePage
    {
        //内容vm
        private readonly ContactManageViewModel _ViewModel;

        public ContactManage()
        {
            InitializeComponent();
            _ViewModel = (ContactManageViewModel) FramGrid.DataContext;
            DataContext = this;
        }


        /// <summary>
        /// 加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _ViewModel.Load();
        }

        /// <summary>
        /// 添加按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAddClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.DBFilePath))
            {
                Messenger.Default.Send(new BubbleTipViewModel
                {
                    AlertLevel = AlertLevel.ERROR,
                    Text = "答应我, 先去配置数据库"
                });
                return;
            }
            new EditContactDialog(null, _ViewModel).ShowDialog();
        }
    }
}
