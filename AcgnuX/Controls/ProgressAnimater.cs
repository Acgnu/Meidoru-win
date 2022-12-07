using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AcgnuX.Controls
{
    /// <summary>
    /// 进度条进度动画器
    /// </summary>
    public class ProgressAnimater
    {
        public static double GetAnimateValue(DependencyObject obj)
        {
            return (double)obj.GetValue(SmoothValueProperty);
        }

        public static void SetAnimateValue(DependencyObject obj, double value)
        {
            obj.SetValue(SmoothValueProperty, value);
        }

        public static readonly DependencyProperty SmoothValueProperty =
            DependencyProperty.RegisterAttached("AnimateValue", typeof(double), typeof(ProgressAnimater), new PropertyMetadata(0.0, Changing));

        private static void Changing(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Duration duration = new Duration(TimeSpan.FromMilliseconds(notify.progressDuration));
            //DoubleAnimation doubleanimation = new DoubleAnimation(notify.oldProgress, notify.nowProgress, duration);
            //MainProgressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            var anim = new DoubleAnimation((double)e.OldValue, (double)e.NewValue, new TimeSpan(0, 0, 0, 0, 100));
            (d as ProgressBar).BeginAnimation(ProgressBar.ValueProperty, anim, HandoffBehavior.Compose);
        }
    }
}
