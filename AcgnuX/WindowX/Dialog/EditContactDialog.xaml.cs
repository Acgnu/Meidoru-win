using AcgnuX.Pages;
using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Taskx;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.Utils;
using EnumsNET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AcgnuX.WindowX.Dialog
{
    /// <summary>
    /// 编辑账号的弹窗
    /// </summary>
    public partial class EditContactDialog : BaseDialog {
        private ContactManage mContactManage;
        /// <summary>
        /// 下拉框选项
        /// </summary>
        public List<ListBoxItem> mListBoxItems { get; set; } = new List<ListBoxItem>();

        //视图对象
        public ContactItemViewModel ContactItem { get; set; }

        public EditContactDialog(ContactItemViewModel contactItem, ContactManage contactManage)
        {
            InitializeComponent();
            var enumItems = Enums.GetMembers<ContactPlatform>();
            var selectedIndex = 0;
            for (var i = 0; i < enumItems.Count; i++)
            {
                var item = enumItems[i];
                mListBoxItems.Add(new ListBoxItem
                {
                    Content = item.Name, 
                });
                if (contactItem != null && item.Name.Equals(contactItem.Platform.ToString()))
                    selectedIndex = i;
            }
            DataContext = this;
            mContactManage = contactManage;
            if (null != contactItem)
                ContactItem = contactItem;
            else
                ContactItem = new ContactItemViewModel(contactManage);
            ListBoxPlatform.SelectedIndex = selectedIndex;
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
            ManualTriggerSourceUpdate();
            ContactItem.Platform = (ContactPlatform) EnumLoader.GetByValue(typeof(ContactPlatform), ((ListBoxItem)ListBoxPlatform.SelectedItem).Content.ToString());
            var r = mContactManage.SaveContact(ContactItem);
            button.IsEnabled = true;
            if (!r.success)
            {
                return;
            }
            Close();
        }

        /// <summary>
        /// 手动更新源
        /// </summary>
        private void ManualTriggerSourceUpdate()
        {
            //手动表格更新vm
            BindingExpression binding = TextBlockName.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = TextBlockUid.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = TextBlockPhone.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            binding = TextBlockName.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
            //binding = TextBlockPlatfrom.GetBindingExpression(ComboBox.SelectedValueProperty);
            //binding.UpdateSource();
        }

        private void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnAvatarImageClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image eventObj = sender as Image;
            var path = FileUtil.OpenFileDialogForPath("C:\\", "JPEG图片|*.jpeg|JPG图片|*.jpg");
            if (!string.IsNullOrEmpty(path))
            {
                eventObj.Source = ImageUtil.GetBitmapImage(path);
                ContactItem.Avatar = File.ReadAllBytes(path);
            }
        }
    }
}