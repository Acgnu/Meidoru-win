using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.Utils;
using AcgnuX.WindowX.Dialog;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

        /// <summary>
        /// 进入页面触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await ProxyFactoryV2.StartTrack();
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
        /// 选择数据库文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChooseDbFile(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = FileUtil.OpenFileDialogForPath("C:\\", "SQLite数据库文件|*.db");
            if (!string.IsNullOrEmpty(path))
            {
                var settingsDataContext = DataContext as SettingsViewModel;
                settingsDataContext.DbFilePathView = path;
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
            if (string.IsNullOrEmpty(ConfigUtil.Instance.DbFilePath))
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "答应我, 先去配置数据库"
                });
                return;
            }
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
                SQLite.ExecuteNonQuery("DELETE FROM crawl_rules WHERE ID = @id", new List<SQLiteParameter> { new SQLiteParameter("@id", selected.Id) });
                var vm = DataContext as SettingsViewModel;
                vm.CrawlRuls.Remove(selected);
                vm.CheckIsCheckedAll(true);
            }
        }

        /// <summary>
        /// 离开页面触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            ProxyFactoryV2.StopTrack();
        }
    }
}
