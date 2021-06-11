using AcgnuX.Pages;
using AcgnuX.Pages.Apis.Ten.Dns;
using AcgnuX.Source.Bussiness.Common;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Commands;
using AcgnuX.Source.Model;
using AcgnuX.Source.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AcgnuX.ViewModel
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    class MainWindowViewModel : CommonWindowViewModel
    {
        //菜单集合
        public ObservableCollection<NavMenu> navMenus { get; set; } = null;

        public MainWindowViewModel(Window window) : base(window)
        {
            //初始化菜单
            InitializeMenus();
            //主窗口退出则强制退出所有
            CloseCommand = new RelayCommand(() => Task.Run(() => Environment.Exit(0)));
        }

        /// <summary>
        /// 初始化菜单
        /// </summary>
        private void InitializeMenus()
        {
            navMenus = new ObservableCollection<NavMenu>()
            {
                new NavMenu() { name = "疼逊云解析", pageType = typeof(DnsRecords), icon=(Geometry)Application.Current.FindResource("Icon_Paperclip") },
                new NavMenu() { name = "WEB服务",pageType = typeof (WebServer), icon=(Geometry)Application.Current.FindResource("Icon_Server") },
                new NavMenu() { name = "密码库",pageType = typeof( PwdRepositroy), icon=(Geometry)Application.Current.FindResource("Icon_PasswordBox") },
                new NavMenu() { name = "谱库",pageType = typeof( MusicScoreLibrary), icon=(Geometry)Application.Current.FindResource("Icon_Book") },
                new NavMenu() { name = "爪机同步",pageType =typeof ( MobileDeviceControl), icon=(Geometry)Application.Current.FindResource("Icon_Cloud") },
                new NavMenu() { name = "设置",pageType = typeof (Settings), icon=(Geometry)Application.Current.FindResource("Icon_Setting") }
            };

            //DataContext = navMenus;
        }
    }
}
