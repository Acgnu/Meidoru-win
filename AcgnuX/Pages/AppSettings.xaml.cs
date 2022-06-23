using AcgnuX.Properties;
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
                Settings.Default.AccountFilePath = path;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// 选择数据库文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnChooseDbFile(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = FileUtil.OpenFileDialogForPath("C:\\", "SQLite数据库文件|*.db");
            if (!string.IsNullOrEmpty(path))
            {
                var settingsDataContext = DataContext as SettingsViewModel;
                settingsDataContext.DbFilePathView = path;
                Settings.Default.DBFilePath = path;
                Settings.Default.Save();
                await SQLite.SetDbFilePath(path);
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
                Settings.Default.Tan8HomeDir = path;
                Settings.Default.Save();
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
        /// 同步配置行双击选中事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSyncConfigDataGridDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (null == SyncPathDataGrid.SelectedItem) return;
            //打开修改对话框
            var dialog = new EditSyncConfigDialog(SyncPathDataGrid.SelectedItem as SyncConfigViewModel);
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault() == true)
            {
                var vm = DataContext as SettingsViewModel;
                vm.CheckSyncConfigIsCheckedAll(true);
            }
        }

        /// <summary>
        /// 添加规则按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddCrawlClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.DBFilePath))
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
        /// 添加规则按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddSyncConfigClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.DBFilePath))
            {
                mMainWindow.SetStatustProgess(new MainWindowStatusNotify()
                {
                    alertLevel = AlertLevel.ERROR,
                    message = "答应我, 先去配置数据库"
                });
                return;
            }
            //打开修改对话框
            var dialog = new EditSyncConfigDialog(null);
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault() == true)
            {
                var vm = DataContext as SettingsViewModel;
                //由于添加重复策略是替换, 因此新增成功之后需要检查当前vm中重复的项
                for(var i = 0;  i < vm.SyncConfigs.Count; i++)
                {
                    //如果存在PcPath重复或者MobilePath重复, 都需要删除 (Replace 策略会将任意一个重复的都删除 )
                    var item = vm.SyncConfigs[i];
                    if(item.PcPath.Equals(dialog.SyncConfig.PcPath) || item.MobilePath.Equals(dialog.SyncConfig.MobilePath))
                    {
                        vm.SyncConfigs.Remove(item);
                        i--;
                    }
                }
                vm.SyncConfigs.Add(dialog.SyncConfig);
                vm.CheckSyncConfigIsCheckedAll(true);
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
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, string.Format("{0}", selected.Name))).ShowDialog();
            if (result.GetValueOrDefault())
            {
                SQLite.ExecuteNonQuery("DELETE FROM crawl_rules WHERE ID = @id", new List<SQLiteParameter> { new SQLiteParameter("@id", selected.Id) });
                var vm = DataContext as SettingsViewModel;
                vm.CrawlRuls.Remove(selected);
                vm.CheckIsCheckedAll(true);
            }
        }

        /// <summary>
        /// 同步规则删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSyncConfigItemMouseRightClick(object sender, MouseButtonEventArgs e)
        {
            XamlUtil.SelectRow(SyncPathDataGrid, e);
            var selected = SyncPathDataGrid.SelectedItem as SyncConfigViewModel;
            if (null == selected) return;
            //删除对话框
            var result = new ConfirmDialog(AlertLevel.WARN, string.Format(Properties.Resources.S_DeleteConfirm, string.Format("{0}", selected.PcPath))).ShowDialog();
            if (result.GetValueOrDefault())
            {
                SQLite.ExecuteNonQuery("DELETE FROM media_sync_config WHERE ID = @id", new List<SQLiteParameter> { new SQLiteParameter("@id", selected.Id) });
                var vm = DataContext as SettingsViewModel;
                vm.SyncConfigs.Remove(selected);
                vm.CheckSyncConfigIsCheckedAll(true);
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
