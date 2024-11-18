using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using SharedLib.Utils;
using System.Windows;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class AppSettings
    {
        public SettingsViewModel ViewModel { get; }
        private ProxyFactoryV2 _ProxyFactoryV2 { get; }

        public AppSettings()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetService<SettingsViewModel>();
            _ProxyFactoryV2 = App.Current.Services.GetService<ProxyFactoryV2>();
            DataContext = ViewModel;
        }

        /// <summary>
        /// 进入页面触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await _ProxyFactoryV2.StartTrack();
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChooseFIle(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = WindowUtil.OpenFileDialogForPath("C:\\", "JSON文件|*.JSON");
            if (!string.IsNullOrEmpty(path))
            {
                ViewModel.AccountJsonPath = path;
            }
        }

        /// <summary>
        /// 选择数据库文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChooseDbFile(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = WindowUtil.OpenFileDialogForPath("C:\\", "SQLite数据库文件|*.db");
            if (!string.IsNullOrEmpty(path))
            {
                ViewModel.DbFilePath = path;
                var initSQL = XamlUtil.GetApplicationResourceAsString(@"Assets\data\" + ApplicationConstant.DB_INIT_FILE);
                SQLite.SetDbFilePath(path, initSQL);
            }
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChooseFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = WindowUtil.OpenFolderDialogForPath(null);
            if (!string.IsNullOrEmpty(path))
            {
                ViewModel.PianoScorePath = path;
            }
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChooseSkinFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = WindowUtil.OpenFolderDialogForPath(null);
            if (!string.IsNullOrEmpty(path))
            {
                ViewModel.SkinFolderPath = path;
            }
        }

        /// <summary>
        /// 离开页面触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            _ProxyFactoryV2.StopTrack();
        }
    }
}
