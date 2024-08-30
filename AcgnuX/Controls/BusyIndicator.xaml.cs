using AcgnuX.Source.Utils;
using SharedLib.Utils;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

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
        //private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return (bool)GetValue(IsBusyProperty);
            }
            set
            {
                //SetValue(IsBusyProperty, value);
                if (value)
                {
                    Visibility = Visibility.Visible;
                    var _fadeInAnimation = (DoubleAnimation) FindResource("FadeInAnimation");
                    BeginAnimation(UIElement.OpacityProperty, _fadeInAnimation);
                }
                else
                {
                    //var fadeOutStoryboard = (Storyboard) this.FindResource("FadeOutStoryboard");
                    //fadeOutStoryboard.AutoReverse = true;
                    var _fadeOutAnimation = (DoubleAnimation) FindResource("FadeOutAnimation");
                    _fadeOutAnimation.Completed += (sender, e) => Visibility = Visibility.Collapsed;
                    BeginAnimation(UIElement.OpacityProperty, _fadeOutAnimation);
                }
            }
        }

        public static readonly DependencyProperty IsBusyProperty =
         DependencyProperty.Register(
             nameof(IsBusy),
             typeof(bool),
             typeof(BusyIndicator),
             new PropertyMetadata(false, OnIsBusyPropertyChange));

        public BusyIndicator()
        {
            InitializeComponent();
            this.Visibility = Visibility.Collapsed;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                //加载GIF图片
                var steam = XamlUtil.GetApplicationResourceAsStream("../Assets/Images/loading_nekololi.gif");
                var bytes = FileUtil.Stream2Bytes(steam.Stream);
                var image = ImageUtil.ByteArrayToImage(bytes);
                AnimateImg.AnimatedImageControl(image);
            }
            //timer.Interval = new TimeSpan(200000);
            //timer.Tick += new EventHandler(timer_Tick);
            //timer.Start();
        }

        /// <summary>
        /// IsBusy 属性变化事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnIsBusyPropertyChange(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var indicator = obj as BusyIndicator;
            indicator.IsBusy = (bool) args.NewValue;
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
