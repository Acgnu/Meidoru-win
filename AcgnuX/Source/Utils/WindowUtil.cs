using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// 窗口工具类
    /// </summary>
    public static class WindowUtil
    {
        /// <summary>
        /// 打开文件对话框, 返回所选文件完整路径
        /// </summary>
        /// <param name="initialPath">初始化路径</param>
        /// <param name="filter">文件过滤</param>
        /// <returns></returns>
        public static string OpenFileDialogForPath(string initialPath, string filter)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = initialPath,
                Filter = filter,
                RestoreDirectory = true,
                FilterIndex = 1
            };
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return "";
        }

        /// <summary>
        /// 打开文件对话框, 返回所选文件夹完整路径
        /// </summary>
        /// <param name="initialPath">初始化路径</param>
        /// <param name="filter">文件过滤</param>
        /// <returns></returns>
        public static string OpenFolderDialogForPath(string initialPath)
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "请选择文件路径"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            return "";
        }

        /// <summary>
        /// 显示信息气泡
        /// </summary>
        /// <param name="message"></param>
        public static void ShowBubbleInfo(string message) => ShowBubbleMessage(message, AlertLevel.INFO);

        /// <summary>
        /// 显示错误气泡
        /// </summary>
        /// <param name="message"></param>
        public static void ShowBubbleError(string message) => ShowBubbleMessage(message, AlertLevel.ERROR);

        /// <summary>
        /// 显示警告气泡
        /// </summary>
        /// <param name="message"></param>
        public static void ShowBubbleWarn(string message) => ShowBubbleMessage(message, AlertLevel.WARN);

        /// <summary>
        /// 显示消息气泡
        /// </summary>
        /// <param name="message"></param>
        /// <param name="alertLevel"></param>
        public static void ShowBubbleMessage(string message, AlertLevel alertLevel)
        {
            WeakReferenceMessenger.Default.Send(new BubbleTipViewModel
            {
                AlertLevel = alertLevel,
                Text = message
            });
        }
    }
}
