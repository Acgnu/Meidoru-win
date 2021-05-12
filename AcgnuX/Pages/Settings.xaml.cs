using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.Utils;
using AcgnuX.WindowX.Dialog;
using System;
using System.Windows;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : BasePage
    {
        //视图模型
        private SettingsViewModel settingsViewModel = new SettingsViewModel();
        private CrawlConfigViewModel mCrawlConfigViewModel;

        public Settings(MainWindow mainWin)
        {
            InitializeComponent();
            InitSettingData();
            mMainWindow = mainWin;
            DataContext = settingsViewModel;
            //初始化规则表格数据
            mCrawlConfigViewModel = new CrawlConfigViewModel(this);
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            mCrawlConfigViewModel.LoadRules();
        }

        /// <summary>
        /// 初始化设置
        /// </summary>
        private void InitSettingData()
        {
            var settingContext = AcgnuConfig.GetContext();
            settingsViewModel.accountJsonPath = settingContext.accountJsonPath;
            settingsViewModel.pianoScorePath = settingContext.pianoScorePath;
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickSaveButton(object sender, RoutedEventArgs e)
        {
            AcgnuConfig.SaveSetting(DataContext as AcgnuConfigContext);
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChooseFIle(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            settingsViewModel.PianoScorePathAndView = Convert.ToString(TimeUtil.CurrentMillis());
            var path = FileUtil.OpenFileDialogForPath("C:\\", "JSON文件|*.JSON");
            if (!string.IsNullOrEmpty(path))
            {
                settingsViewModel.AccountJsonPathAndView = path;
            }
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChooseFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = FileUtil.OpenFolderDialogForPath(null);
            if (!string.IsNullOrEmpty(path))
            {
                settingsViewModel.PianoScorePathAndView = path;
            }
        }
    }
}
