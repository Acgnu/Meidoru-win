using AcgnuX.Pages;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AcgnuX.Source.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainWindowViewModel : CommonWindowViewModel
    {
        //菜单集合
        public ObservableCollection<NavMenu> navMenus { get; set; } = null;

        public MainWindowViewModel() : base()
        {
            //初始化菜单
            InitializeMenus();
            //主窗口退出则强制退出所有
            CloseCommand = new RelayCommand<Window>((win) => Task.Run(() => {
                //清空所有通知
                ToastNotificationManagerCompat.Uninstall();
                //退出Tan8播放器
                Tan8PlayUtil.Exit();
                //退出程序
                Environment.Exit(0);
            }));
        }

        /// <summary>
        /// 初始化菜单
        /// </summary>
        private void InitializeMenus()
        {
            navMenus = new ObservableCollection<NavMenu>()
            {
                new NavMenu() { name = "疼逊云解析", pageType = typeof(DnsManage), icon=(Geometry)Application.Current.FindResource("Icon_Paperclip") },
                new NavMenu() { name = "WEB服务",pageType = typeof (WebServer), icon=(Geometry)Application.Current.FindResource("Icon_Server") },
                new NavMenu() { name = "密码库",pageType = typeof( PwdRepositroy), icon=(Geometry)Application.Current.FindResource("Icon_PasswordBox") },
                new NavMenu() { name = "谱库",pageType = typeof( Tan8SheetReponsitory), icon=(Geometry)Application.Current.FindResource("Icon_Book") },
                new NavMenu() { name = "爪机同步",pageType =typeof ( MobileDeviceControl), icon=(Geometry)Application.Current.FindResource("Icon_Cloud") },
                new NavMenu() { name = "联系人",pageType =typeof ( ContactManage), icon=(Geometry)Application.Current.FindResource("Icon_ContactBoox") },
                new NavMenu() { name = "设置",pageType = typeof (AppSettings), icon=(Geometry)Application.Current.FindResource("Icon_Setting") }
            };

            //DataContext = navMenus;
        }
    }
}