using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.ViewModel;
using AcgnuX.WindowX.Dialog;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AcgnuX.Source.Utils;

namespace AcgnuX.Pages
{
    /// <summary>
    /// PwdRepositroy.xaml 的交互逻辑
    /// </summary>
    public partial class ContactManage
    {
        //内容vm
        public ContactManageViewModel ViewModel { get; }

        public ContactManage()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetService<ContactManageViewModel>();
            DataContext = ViewModel;
        }


        /// <summary>
        /// 加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Load();
        }

        /// <summary>
        /// 添加按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnAddClick(object sender, RoutedEventArgs e)
        {
            if(ViewModel.SafeMode)
            {
                return;
            }
            if (string.IsNullOrEmpty(Settings.Default.DBFilePath))
            {
                WindowUtil.ShowBubbleError("答应我, 先去配置数据库");
                return;
            }
            var newItem = new ContactItemViewModel();
            if(new EditContactDialog(newItem).ShowDialog().GetValueOrDefault())
            {
                ViewModel.ContactItems.Insert(0, newItem);
            }
        }
    }
}
