using AcgnuX.Pages;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        //�������߶�
        public int TitleHeightGridLength { get; set; } = 38;
        //�˵�����
        public ObservableCollection<NavMenu> navMenus { get; set; } = null;
        //�˵��������
        public ICommand OnNavMenuItemClickCommand { get; set; }
        //faviconͼ��������
        public ICommand FaviconClickCommand { get; set; }
        //��������ҳ������
        public ICommand OnSettingCommand { get; set; }
        //����ҳ��
        private AppSettings _AppSettingsPage;
        //��ҳ
        private readonly Index _IndexPage;
        //������
        private Page _MainContent;
        public Page MainContent
        {
            get { return _MainContent; }
            set { _MainContent = value; RaisePropertyChanged(); }
        }
        //������ʾ
        public BubbleTipViwerViewModel BubbleTipViwerViewModel { get; set; } = new BubbleTipViwerViewModel();
        //ѡ�еĵ����˵���
        private NavMenu _SelectedNavItem;
        public NavMenu SelectedNavItem { get => _SelectedNavItem; set { _SelectedNavItem = value; RaisePropertyChanged(); } }

        public MainWindowViewModel() : base()
        {
            //��ʼ���˵�
            InitializeMenus();
            //�������˳���ǿ���˳�����
            CloseCommand = new RelayCommand<Window>((win) => Task.Run(() => {
                //�������֪ͨ
                ToastNotificationManagerCompat.Uninstall();
                //�˳�Tan8������
                Tan8PlayUtil.ExitAll();
                //�˳�����
                Environment.Exit(0);
            }));
            OnNavMenuItemClickCommand = new RelayCommand<NavMenu>(NavMenuItemClick);
            OnSettingCommand = new RelayCommand<Window>(OnSettingIconClick);
            FaviconClickCommand = new RelayCommand(OnFaviconClick);
            _IndexPage = new Index();
            MainContent = _IndexPage;
        }

        /// <summary>
        /// ����ҳ��
        /// </summary>
        /// <param name="window"></param>
        private void OnSettingIconClick(Window window)
        {
            if (null == _AppSettingsPage)
            {
                _AppSettingsPage = new AppSettings();
            }
            SelectedNavItem = null;
            MainContent = _AppSettingsPage;
        }

        /// <summary>
        /// OnFavicon ����¼�
        /// </summary>
        private void OnFaviconClick()
        {
            SelectedNavItem = null;
            MainContent = _IndexPage;
        }

        /// <summary>
        /// �˵��������ʵ��
        /// </summary>
        /// <param name="navMenuItem"></param>
        private void NavMenuItemClick(NavMenu navMenuItem)
        {
            if (null == navMenuItem.instance)
            {
                //object[] parameters = new object[1];
                //parameters[0] = Application.Current.MainWindow;
                //dynamic page = Activator.CreateInstance(navMenuItem.pageType, parameters);
                dynamic page = Activator.CreateInstance(navMenuItem.pageType);
                navMenuItem.instance = page;
            }
            MainContent = navMenuItem.instance;
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