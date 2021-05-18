using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.Utils;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight.Command;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AcgnuX.Pages
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class AppSettings : BasePage
    {
        public AppSettings(MainWindow mainWin)
        {
            InitializeComponent();
            mMainWindow = mainWin;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
           
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChooseFIle(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = FileUtil.OpenFileDialogForPath("C:\\", "JSON文件|*.JSON");
            if (!string.IsNullOrEmpty(path))
            {
                var settingsDataContext = DataContext as SettingsViewModel;
                settingsDataContext.AccountJsonPathView = path;
                ConfigUtil.Instance.Save(settingsDataContext);
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
                var settingsDataContext = DataContext as SettingsViewModel;
                settingsDataContext.PianoScorePathView = path;
                ConfigUtil.Instance.Save(settingsDataContext);
            }
        }

        /// <summary>
        /// 行双击选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCrawlRulsDataGridDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (null == CrawlConfigDataGrid.SelectedItem) return;
            //打开修改对话框
            var dialog = new EditCrawlDialog(CrawlConfigDataGrid.SelectedItem as CrawlRuleViewModel);
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault() == true)
            {
                var vm = DataContext as SettingsViewModel;
                vm.CheckIsCheckedAll(true);
            }
        }

        /// <summary>
        /// 添加规则按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddCrawlClick(object sender, RoutedEventArgs e)
        {
            //打开修改对话框
            var dialog = new EditCrawlDialog(null);
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault() == true)
            {
                var vm = DataContext as SettingsViewModel;
                vm.CrawlRuls.Add(dialog.McrawlRule);
                vm.CheckIsCheckedAll(true);
            }
        }

        /// <summary>
        /// 规则删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemMouseRightClick(object sender, MouseButtonEventArgs e)
        {
            XamlUtil.SelectRow(CrawlConfigDataGrid, e);
            var selected = CrawlConfigDataGrid.SelectedItem as CrawlRuleViewModel;
            if (null == selected) return;
            //删除对话框
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format((string)Application.Current.FindResource("DeleteConfirm"), string.Format("{0}", selected.Name))).ShowDialog();
            if (result.GetValueOrDefault())
            {
                SQLite.ExecuteNonQuery(string.Format("DELETE FROM crawl_rules WHERE ID = {0}", selected.Id));
                var vm = DataContext as SettingsViewModel;
                vm.CrawlRuls.Remove(selected);
                vm.CheckIsCheckedAll(true);
            }
        }
    }
}
