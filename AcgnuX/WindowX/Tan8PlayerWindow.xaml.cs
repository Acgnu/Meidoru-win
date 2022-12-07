using AcgnuX.Source.Model;
using AcgnuX.Source.ViewModel;
//using AxShockwaveFlashObjects;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms.Integration;

namespace AcgnuX.WindowX
{
    /// <summary>
    /// Tan8PlayerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Tan8PlayerWindow : Window
    {
        //private AxShockwaveFlash axShockwaveFlash;

        public Tan8PlayerWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化Flash
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageLoad(object sender, RoutedEventArgs e)
        {
            WindowsFormsHost formHost = new WindowsFormsHost();
            //axShockwaveFlash = new AxShockwaveFlash();
            //formHost.Child = axShockwaveFlash;
            //PlayerGrid.Children.Add(formHost);
        }

        /// <summary>
        /// 从flash播放器获取真实的乐谱信息URL
        /// </summary>
        /// <param name="ypid"></param>
        /// <returns></returns>
        public string GetRealTan8URL(int ypid)
        {
            //var result = axShockwaveFlash.CallFunction(
            //    string.Format("<invoke name=\"swfExtGetypURL\" returntype=\"xml\"><arguments><string>{0}</string></arguments></invoke>", ypid));
            //return result;
            return "";
        }

        /// <summary>
        /// 播放给定的曲谱
        /// </summary>
        /// <param name="crawlArg">需要播放的曲谱</param>
        public void PlaySelected(Tan8SheetCrawlArg crawlArg)
        {
            string flashPath = Environment.CurrentDirectory;
            flashPath += @"\Assets\flash\fuckTan8\Main.swf?id=" + crawlArg.Ypid;
            //axShockwaveFlash.Movie = flashPath;
        }

        /// <summary>
        /// 重写窗口关闭事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;  //标识取消close
            this.Hide();      // 隐藏, 方便再次调用show()
        }
    }
}
