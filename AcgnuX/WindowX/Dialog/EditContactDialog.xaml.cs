using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using EnumsNET;
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
        //视图对象
        public ContactItemViewModel ViewModel { get; }

        public EditContactDialog(ContactItemViewModel viewModel)
        {
            InitializeComponent();
            InitPlatformComboItemSource();
            ViewModel = viewModel;
            DataContext = this;
            FormGrid.BindingGroup.BeginEdit();
        }

        /// <summary>
        /// 初始化平台选择框
        /// </summary>
        private void InitPlatformComboItemSource()
        {
            var enumItems = Enums.GetMembers<ContactPlatform>();
            var comboItems = new string[enumItems.Count];
            for (var i = 0; i < enumItems.Count; i++)
            {
                var item = enumItems[i];
                comboItems[i] = item.Name;
            }
            ListBoxPlatform.ItemsSource = comboItems;
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
            //参数校验, 通过则提交变更到ViewModel
            if (!FormGrid.BindingGroup.CommitEdit())
            {
                button.IsEnabled = true;
                return;
            }
            if (!ViewModel.Save())
            {
                button.IsEnabled = true;
                return;
            }
            AnimateClose((s, a) => DialogResult = true);
        }

        /// <summary>
        /// 头像点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvatarImageClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var path = WindowUtil.OpenFileDialogForPath(string.Empty, "图片文件|*.jpg;*.png;*.jpeg;*.bmp");
            if (!string.IsNullOrEmpty(path))
            {
                var drawingImage = System.Drawing.Image.FromFile(path);
                var imageBytes = ImageUtil.ImageToByteArray(drawingImage);
                ViewModel.TempAvatar = new ByteArray(imageBytes);

                var border = sender as Border;
                var borderBackgroundBrush = border.Background as ImageBrush;
                borderBackgroundBrush.ImageSource = ImageUtil.GetBitmapImage(path);
            }
        }

        /// <summary>
        /// 校验失败提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                WindowUtil.ShowBubbleError(e.Error.ErrorContent.ToString());
            }
        }
    }
}