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
        //标题栏高度
        public int TitleHeightGridLength { get; set; } = 38;
        //菜单集合
        public ObservableCollection<NavMenu> navMenus { get; set; } = null;
        //菜单点击命令
        public ICommand OnNavMenuItemClickCommand { get; set; }
        //favicon图标点击命令
        public ICommand FaviconClickCommand { get; set; }
        //进入设置页面命令
        public ICommand OnSettingCommand { get; set; }
        //更换背景图命令
        public ICommand OnRefreshBackgroundCommand { get; }
        //主窗口背景图片
        private Brush _mainWindowBackgroundBrush;
        public Brush MainWindowBackgroundBrush { get => _mainWindowBackgroundBrush; set => SetProperty(ref _mainWindowBackgroundBrush, value); }
        //设置页面
        private AppSettings _AppSettingsPage;
        //首页
        private readonly Pages.Index _IndexPage;
        //主内容
        private Page _MainContent;
        public Page MainContent { get => _MainContent; set => SetProperty(ref _MainContent, value); }
        //气泡提示
        public BubbleTipViwerViewModel BubbleTipViwerViewModel { get; set; } = new BubbleTipViwerViewModel();
        //选中的导航菜单项
        private NavMenu _SelectedNavItem;
        public NavMenu SelectedNavItem { get => _SelectedNavItem; set => SetProperty(ref _SelectedNavItem, value); }

        public MainWindowViewModel() : base()
        {
            //初始化菜单
            InitializeMenus();
            //主窗口退出则强制退出所有
            CloseCommand = new RelayCommand<Window>((win) =>
            {
                var storyBoardResource = win.FindResource("WindowFadeOutStoryboard") as Storyboard;
                var storyBoard = storyBoardResource.Clone();
                storyBoard.Completed += new EventHandler((e, s) =>
                {
                    Task.Run(() =>
                    {
                        //清空所有通知
                        //AppNotificationManager.Uninstall();
                        //退出Tan8播放器
                        Tan8PlayUtil.ExitAll();
                        //退出程序
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
        /// 设置页面
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
        /// OnFavicon 点击事件
        /// </summary>
        private void OnFaviconClick()
        {
            SelectedNavItem = null;
            MainContent = _IndexPage;
        }

        /// <summary>
        /// 菜单点击命令实现
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
        /// 初始化菜单
        /// </summary>
        private void InitializeMenus()
        {
            navMenus = new ObservableCollection<NavMenu>()
            {
                new NavMenu() { name = "域名解析", pageType = typeof(DnsManage), icon=(Geometry)Application.Current.FindResource("Icon_Paperclip") },
                new NavMenu() { name = "密码库",pageType = typeof( PwdRepositroy), icon=(Geometry)Application.Current.FindResource("Icon_PasswordBox") },
                new NavMenu() { name = "谱库",pageType = typeof( Tan8SheetReponsitory), icon=(Geometry)Application.Current.FindResource("Icon_Book") },
                new NavMenu() { name = "爪机同步",pageType =typeof ( MobileDeviceControl), icon=(Geometry)Application.Current.FindResource("Icon_Cloud") },
                new NavMenu() { name = "联系人",pageType =typeof ( ContactManage), icon=(Geometry)Application.Current.FindResource("Icon_ContactBoox") }
            };
        }

        /// <summary>
        /// 初始化主窗口背景笔刷
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
        /// 执行更换背景命令
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