using AcgnuX.Properties;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Taskx.Http;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using SharedLib.Utils;
using System.Configuration;
using System.IO;
using System.Windows;

namespace AcgnuX
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //[DllImport("User32.dll")]
        //private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        //[DllImport("User32.dll")]
        //private static extern bool SetForegroundWindow(IntPtr hWnd);

        //必须持有此对象作为成员变量, 才能生效
        //private Mutex mMutex;

        public static new App Current => (App)Application.Current;

        //服务, 可用于IOC
        public IServiceProvider Services { get; }

        /**
        /// <summary>
        /// 显示已运行的程序
        /// </summary>
        /// <param name="instance"></param>
        public static void HandleRunningInstance(Process instance)
        {
            ShowWindowAsync(instance.MainWindowHandle, 1); //显示
            SetForegroundWindow(instance.MainWindowHandle);            //放到前端
        }
        **/

        public App()
        {
            Services = ConfigureServices();
            this.InitializeComponent();
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <returns></returns>
        private static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                //service
                .AddSingleton<AppSecretKeyRepo, AppSecretKeyRepo>()
                .AddSingleton<ContactRepo, ContactRepo>()
                .AddSingleton<MediaSyncConfigRepo, MediaSyncConfigRepo>()
                .AddSingleton<Tan8SheetCrawlRecordRepo, Tan8SheetCrawlRecordRepo>()
                .AddSingleton<Tan8SheetCrawlTaskRepo, Tan8SheetCrawlTaskRepo>()
                .AddSingleton<Tan8SheetsRepo, Tan8SheetsRepo>()

                //需要单例的组件
                .AddSingleton<HttpWebServer, HttpWebServer>()
                .AddSingleton<ProxyFactoryV2, ProxyFactoryV2>()

                //viewmodel
                .AddTransient<MainWindowViewModel>()
                .AddTransient<DnsManageViewModel>()
                .AddTransient<PwdRepositoryViewModel>()
                .AddTransient<PianoScoreDownloadRecordViewModel>()
                .AddTransient<Tan8PlayerViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<Tan8SheetReponsitoryViewModel>()
                .AddTransient<DeviceSyncViewModel>()
                .AddTransient<DeviceSyncPathConfigDialogViewModel>()
                .AddTransient<ContactManageViewModel>()
                .BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            /**
            //检测是否单例启动
            mMutex = new Mutex(true, System.Reflection.Assembly.GetEntryAssembly().GetName().Name, out bool createdNew);
            if (!createdNew)
            {
                var psArr = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                foreach (var ps in psArr) 
                {
                    if(ps.Id != Process.GetCurrentProcess().Id)
                    {
                        //已启动则展示之前启动的, 并关闭此实例
                        HandleRunningInstance(ps);
                        Shutdown();
                        return;
                    }
                }
            }
            **/

            Task.Run(() =>
            {
                //初始化设置
                string configFilePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                if (!File.Exists(configFilePath))
                {
                    ConfigUtil.TryRestoreFromPreviousVersion(Settings.Default);
                }
                var dbfilePath = Settings.Default.DBFilePath;   //ConfigUtil.Instance.Load().DbFilePath;

                //检查数据库文件是否存在
                if (!File.Exists(dbfilePath)) return;

                //设置数据库, 创建必须的表
                var initSQL = XamlUtil.GetApplicationResourceAsString(@"Assets\data\" + ApplicationConstant.DB_INIT_FILE);
                SQLite.SetDbFilePath(dbfilePath, initSQL);
            });

            new MainWindow().Show();
        }
    }
}
