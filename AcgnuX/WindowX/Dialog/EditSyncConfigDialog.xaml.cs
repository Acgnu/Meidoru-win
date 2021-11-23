using AcgnuX.Pages;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑账号的弹窗
    /// </summary>
    public partial class EditSyncConfigDialog : BaseDialog {
        //视图对象
        public SyncConfigViewModel SyncConfig { get; set; } = new SyncConfigViewModel()
        {
            Enable = 1
        };

        public EditSyncConfigDialog(SyncConfigViewModel syncConfig)
        {
            InitializeComponent();
            DataContext = this;
            if (null != syncConfig)
            {
                SyncConfig = syncConfig;
            }
        }

        /// <summary>
        /// 保存按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            int successRow;
            ManualTriggerSourceUpdate();
            if (0 == SyncConfig.Id)
            {
                successRow = SaveSyncConfig(SyncConfig);
            }
            else
            {
                successRow = ModifySyncConfig(SyncConfig);
            }
            if (successRow > 0)
            {
                DialogResult = true;
            }
            button.IsEnabled = true;
            Close();
        }

        /// <summary>
        /// 手动更新源
        /// </summary>
        private void ManualTriggerSourceUpdate()
        {
            //手动表格更新vm
            BindingExpression binding = TextBlockPcPath.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = TextBlockMobilePath.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = CheckboxEnable.GetBindingExpression(CheckBox.IsCheckedProperty);
            binding.UpdateSource();
        }

        /// <summary>
        /// 新增同步配置
        /// </summary>
        /// <param name="syncConfig"></param>
        /// <returns></returns>
        private int SaveSyncConfig(SyncConfigViewModel syncConfig)
        {
            if(string.IsNullOrEmpty(syncConfig.PcPath) || string.IsNullOrEmpty(syncConfig.MobilePath)) return 0;

            var row = SQLite.ExecuteNonQuery("INSERT INTO media_sync_config(ID, PC_PATH, MOBILE_PATH, ENABLE) VALUES ((SELECT MAX(ID)+1 FROM media_sync_config), @PcPath, @MobilePath, @Enable)",
                new List<SQLiteParameter> {
                    new SQLiteParameter("@PcPath", syncConfig.PcPath) ,
                    new SQLiteParameter("@MobilePath", syncConfig.MobilePath) ,
                    new SQLiteParameter("@Enable", syncConfig.Enable)
                });

            if (row > 0)
            {
                //查询最新添加的记录ID
                var newID = SQLite.sqlone("SELECT MAX(id) FROM media_sync_config", null);
                syncConfig.Id = Convert.ToInt32(newID);
            }
            return row;
        }

        /// <summary>
        /// 修改同步配置
        /// </summary>
        /// <param name="syncConfig"></param>
        /// <returns></returns>
        private int ModifySyncConfig(SyncConfigViewModel syncConfig)
        {
            var row = SQLite.ExecuteNonQuery("UPDATE media_sync_config SET PC_PATH = @PcPath, MOBILE_PATH = @MobilePath, ENABLE = @Enable WHERE ID = @Id",
                new List<SQLiteParameter> {
                    new SQLiteParameter("@PcPath", syncConfig.PcPath) ,
                    new SQLiteParameter("@MobilePath", syncConfig.MobilePath) ,
                    new SQLiteParameter("@Enable", syncConfig.Enable),
                    new SQLiteParameter("@Id", syncConfig.Id)
                });
            return row;
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
                TextBlockPcPath.Text = path;
            }
        }

        private void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}