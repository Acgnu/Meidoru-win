using AcgnuX.Pages;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        public int TitleHeightGridLength { get; set; } = 38;
        //�˵�����
        public ObservableCollection<NavMenu> navMenus { get; set; } = null;
        //�˵��������
        public ICommand OnNavMenuItemClickCommand { get; set; }
        //����ҳ��
        public ICommand OnSettingCommand { get; set; }
        //����ҳ��
        private AppSettings appSettingsPage;
        //������
        private Object mainContent;
        public Object MainContent
        {
            get { return mainContent; }
            set
            {
                mainContent = value;
                RaisePropertyChanged();
            }
        }

        public MainWindowViewModel() : base()
        {
            //��ʼ���˵�
            InitializeMenus();
            //�������˳���ǿ���˳�����
            CloseCommand = new RelayCommand<Window>((win) => Task.Run(() => {
                //�������֪ͨ
                ToastNotificationManagerCompat.Uninstall();
                //�˳�Tan8������
                Tan8PlayUtil.Exit();
                //�˳�����
                Environment.Exit(0);
            }));
            OnNavMenuItemClickCommand = new RelayCommand<NavMenu>(NavMenuItemClick);
            OnSettingCommand = new RelayCommand<Window>(OnSettingIconClick);
        }

        private void OnSettingIconClick(Window window)
        {
            if (null == appSettingsPage)
            {
                appSettingsPage = new AppSettings((MainWindow)window);
            }
            MainContent = appSettingsPage;
        }

        /// <summary>
        /// �˵��������ʵ��
        /// </summary>
        /// <param name="navMenuItem"></param>
        private void NavMenuItemClick(NavMenu navMenuItem)
        {
            var item = navMenuItem;
            if (null == item.instance)
            {
                object[] parameters = new object[1];
                parameters[0] = Application.Current.MainWindow;
                dynamic page = Activator.CreateInstance(item.pageType, parameters);
                item.instance = page;
            }
            MainContent = item.instance;
        }

        /// <summary>
        /// ��ʼ���˵�
        /// </summary>
        private void InitializeMenus()
        {
            navMenus = new ObservableCollection<NavMenu>()
            {
                new NavMenu() { name = "��ѷ�ƽ���", pageType = typeof(DnsManage), icon=(Geometry)Application.Current.FindResource("Icon_Paperclip") },
                //new NavMenu() { name = "WEB����",pageType = typeof (WebServer), icon=(Geometry)Application.Current.FindResource("Icon_Server") },
                new NavMenu() { name = "�����",pageType = typeof( PwdRepositroy), icon=(Geometry)Application.Current.FindResource("Icon_PasswordBox") },
                new NavMenu() { name = "�׿�",pageType = typeof( Tan8SheetReponsitory), icon=(Geometry)Application.Current.FindResource("Icon_Book") },
                new NavMenu() { name = "צ��ͬ��",pageType =typeof ( MobileDeviceControl), icon=(Geometry)Application.Current.FindResource("Icon_Cloud") },
                new NavMenu() { name = "��ϵ��",pageType =typeof ( ContactManage), icon=(Geometry)Application.Current.FindResource("Icon_ContactBoox") },
                //new NavMenu() { name = "����",pageType = typeof (AppSettings), icon=(Geometry)Application.Current.FindResource("Icon_Setting") }
            };
        }
    }
}