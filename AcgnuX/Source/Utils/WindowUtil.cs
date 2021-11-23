using AcgnuX.Source.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// 窗口工具类
    /// </summary>
    public static class WindowUtil
    {
        /// <summary>
        /// 设置打开的对话框基本属性
        /// </summary>
        /// <param name="window">将要打开的对话框</param>
        /// <param name="owner">对话框的父窗口</param>
        /// <returns></returns>
        public static T SetDialogProperty<T>(T window, Window owner) where T : Window
        {
            //设置所属窗口
            //window.Owner = owner;
            //在父窗口中心打开
            //window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return window;
        }

        /// <summary>
        /// 发送进度到主信息栏
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="message"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MainWindowStatusNotify CalcProgress(MainWindowStatusNotify notify, string message, double value)
        {
            notify.message = message;
            notify.oldProgress = notify.nowProgress;
            notify.nowProgress = value;
            return notify;
        }
    }
}
