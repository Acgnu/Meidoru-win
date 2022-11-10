using AcgnuX.Source.ViewModel;
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
    /// BubbleTip.xaml 的交互逻辑
    /// </summary>
    public partial class BubbleTip : UserControl
    {
        public bool IsShow 
        {
            get { return ( bool )GetValue(IsShowProperty); } 
            set { SetValue(IsShowProperty, value); }
        }

        public static readonly DependencyProperty IsShowProperty =
             DependencyProperty.Register(
                 nameof(IsShow),
                 typeof(bool),
                 typeof(BubbleTip),
                 new PropertyMetadata(false, OnIsShowPropertyChange));

        public BubbleTip()
        {
            InitializeComponent();
        }

        private static void OnIsShowPropertyChange(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var bubbleTip = obj as BubbleTip;
            var IsShow = (bool) args.NewValue;
            if (IsShow)
            {
                bubbleTip.Visibility = Visibility.Visible;
                var _fadeInAnimation = (DoubleAnimation)bubbleTip.FindResource("FadeInAnimation");
                bubbleTip.BeginAnimation(UIElement.OpacityProperty, _fadeInAnimation);
            }
            else
            {
                var _fadeOutAnimation = (DoubleAnimation)bubbleTip.FindResource("FadeOutAnimation");
                _fadeOutAnimation.Completed += (sender, e) =>
                {
                    bubbleTip.Visibility = Visibility.Collapsed;
                    var vm = bubbleTip.DataContext as BubbleTipViewModel;
                    vm.AnimationEndAction?.Invoke();
                };
                bubbleTip.BeginAnimation(UIElement.OpacityProperty, _fadeOutAnimation);
            }
        }
    }
}
