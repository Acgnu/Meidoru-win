using AcgnuX.Pages;
using AcgnuX.Properties;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using CommunityToolkit.Mvvm.Input;
using SharedLib.Utils;
using System.Collections.ObjectModel;
using System.Runtime.Caching;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
        //��������ͼ����
        public ICommand OnRefreshBackgroundCommand { get; }
        //�����ڱ���ͼƬ
        private Brush _mainWindowBackgroundBrush;
        public Brush MainWindowBackgroundBrush { get => _mainWindowBackgroundBrush; set => SetProperty(ref _mainWindowBackgroundBrush, value); }
        //����ҳ��
        private AppSettings _AppSettingsPage;
        //��ҳ
        private readonly Pages.Index _IndexPage;
        //������
        private Page _MainContent;
        public Page MainContent { get => _MainContent; set => SetProperty(ref _MainContent, value); }
        //������ʾ
        public BubbleTipViwerViewModel BubbleTipViwerViewModel { get; set; } = new BubbleTipViwerViewModel();
        //ѡ�еĵ����˵���
        private NavMenu _SelectedNavItem;
        public NavMenu SelectedNavItem { get => _SelectedNavItem; set => SetProperty(ref _SelectedNavItem, value); }

        public MainWindowViewModel() : base()
        {
            //��ʼ���˵�
            InitializeMenus();
            //�������˳���ǿ���˳�����
            CloseCommand = new RelayCommand<Window>((win) =>
            {
                var storyBoardResource = win.FindResource("WindowFadeOutStoryboard") as Storyboard;
                var storyBoard = storyBoardResource.Clone();
                storyBoard.Completed += new EventHandler((e, s) =>
                {
                    Task.Run(() =>
                    {
                        //�������֪ͨ
                        //AppNotificationManager.Uninstall();
                        //�˳�Tan8������
                        Tan8PlayUtil.ExitAll();
                        //�˳�����
                        Environment.Exit(0);
                    });
                });
                storyBoard.Begin(win);
            });

            OnNavMenuItemClickCommand = new RelayCommand<NavMenu>(NavMenuItemClick);
            OnRefreshBackgroundCommand = new RelayCommand<Window>(ExecuteRefreshBackgroundCommand);
            OnSettingCommand = new RelayCommand<Window>(OnSettingIconClick);
            FaviconClickCommand = new RelayCommand(OnFaviconClick);
            _IndexPage = new Pages.Index();
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
                new NavMenu() { name = "��������", pageType = typeof(DnsManage), icon=(Geometry)Application.Current.FindResource("Icon_Paperclip") },
                new NavMenu() { name = "�����",pageType = typeof( PwdRepositroy), icon=(Geometry)Application.Current.FindResource("Icon_PasswordBox") },
                new NavMenu() { name = "�׿�",pageType = typeof( Tan8SheetReponsitory), icon=(Geometry)Application.Current.FindResource("Icon_Book") },
                new NavMenu() { name = "צ��ͬ��",pageType =typeof ( MobileDeviceControl), icon=(Geometry)Application.Current.FindResource("Icon_Cloud") },
                new NavMenu() { name = "��ϵ��",pageType =typeof ( ContactManage), icon=(Geometry)Application.Current.FindResource("Icon_ContactBoox") }
            };
        }

        /// <summary>
        /// ��ʼ�������ڱ�����ˢ
        /// </summary>
        public void InitBackgroundBrush(double windowWidth)
        {
            MainWindowBackgroundBrush = ImageUtil.LoadImageAsBrush(Settings.Default.SkinFilePath, 0, 0, (int)windowWidth);
            if (null == MainWindowBackgroundBrush) return;

            MemoryCache.Default["skinBrush"] = MainWindowBackgroundBrush;
            MainWindowBackgroundBrush?.BeginAnimation(
                Brush.OpacityProperty,
                Application.Current.FindResource("AniImageBrushFadeIn") as DoubleAnimation);
        }

        /// <summary>
        /// ִ�и�����������
        /// </summary>
        private void ExecuteRefreshBackgroundCommand(Window window)
        {
            var folderPath = Settings.Default.SkinFolderPath;
            if (string.IsNullOrEmpty(folderPath)) return;

            var skinFile = FileUtil.GetRandomSkinFile(folderPath);
            if (skinFile == null) return;

            Settings.Default.SkinFilePath = skinFile;
            Settings.Default.Save();
            InitBackgroundBrush(window.Width);
        }
    }
}