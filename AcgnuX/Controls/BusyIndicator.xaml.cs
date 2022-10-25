using AcgnuX.Source.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AcgnuX.Controls
{
    /// <summary>
    /// BusyIndicator.xaml 的交互逻辑
    /// </summary>
    public partial class BusyIndicator : UserControl
    {
        //private int _counter = 0;
        //private DispatcherTimer timer = new DispatcherTimer();
        //private RotateTransform rt = new RotateTransform();
        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                if (value)
                {
                    this.Opacity = 1;
                    Visibility = Visibility.Visible;
                    //timer.Start();
                }
                else
                {
                    var fadeOutStoryboard = (Storyboard) this.FindResource("FadeOutStoryboard");
                    //首先隐藏图片，图片隐藏后隐藏掉透明的背景层。
                    fadeOutStoryboard.Completed += (sender, e) =>
                    {
                        Visibility = Visibility.Collapsed;
                    };
                    fadeOutStoryboard.Begin();
                }
            }
        }

        private void FadeOutStoryboard_Completed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public BusyIndicator()
        {
            InitializeComponent();
            this.Visibility = Visibility.Collapsed;
            var steam = FileUtil.GetApplicationResourceAsStream("../Assets/Images/loading_nekololi.gif");
            var bytes = FileUtil.Stream2Bytes(steam.Stream);
            var image = ImageUtil.ByteArrayToImage(bytes);
            AnimateImg.AnimatedImageControl(image);
            //timer.Interval = new TimeSpan(200000);
            //timer.Tick += new EventHandler(timer_Tick);
            //timer.Start();
        }

        /**
        void timer_Tick(object sender, EventArgs e)
        {
            _counter++;
            //设置旋转中心点，根据图片大小设置，值为图片尺寸/2.
            rt.CenterX = 64;
            rt.CenterY = 64;
            rt.Angle -= 10; //旋转图片，每次旋转10度，可自定义旋转方向
            LoadImg.RenderTransform = rt;
            //让Loading后面的点闪的不要太快
            if (_counter % 8 == 0)
            {
                if (txtLoading.Text.Equals("Loading..."))
                {
                    txtLoading.Text = "Loading.";
                }
                else if (txtLoading.Text.Equals("Loading."))
                {
                    txtLoading.Text = "Loading..";
                }
                else if (txtLoading.Text.Equals("Loading.."))
                {
                    txtLoading.Text = "Loading...";
                }
            }
        }
        **/
    }
}
