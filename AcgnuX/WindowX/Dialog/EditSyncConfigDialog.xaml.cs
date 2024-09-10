using AcgnuX.Pages;
using AcgnuX.Source.Bussiness.Data;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using SharedLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑账号的弹窗
    /// </summary>
    public partial class EditSyncConfigDialog : BaseDialog {
        //视图对象
        public SyncConfigViewModel ContentViewModel { get; }

        private readonly MediaSyncConfigRepo _MediaSyncConfigRepo;

        public EditSyncConfigDialog(SyncConfigViewModel syncConfig)
        {
            InitializeComponent();
            _MediaSyncConfigRepo = App.Current.Services.GetService<MediaSyncConfigRepo>();
            ContentViewModel = syncConfig;
            DataContext = this;
            FormStackPanel.BindingGroup.BeginEdit();
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

            if (!FormStackPanel.BindingGroup.CommitEdit())
            {
                button.IsEnabled = true;
                return; 
            }

            if (0 == ContentViewModel.Id)
            {
                ContentViewModel.Id = _MediaSyncConfigRepo.Add(ContentViewModel.PcPath, ContentViewModel.MobilePath, ContentViewModel.Enable);
            }
            else
            {
                _MediaSyncConfigRepo.Update(ContentViewModel.Id, ContentViewModel.PcPath, ContentViewModel.MobilePath, ContentViewModel.Enable);
            }
            AnimateClose((s, a) => DialogResult = true);
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
                TextBlockPcPath.Text = path;
            }
        }

        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                WindowUtil.ShowBubbleError(e.Error.ErrorContent.ToString());
            }
        }
    }
}