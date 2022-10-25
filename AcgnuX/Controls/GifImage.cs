using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AcgnuX.Controls
{
    /// <summary>
    /// GifImage.xaml 的交互逻辑
    /// </summary>
    public class GifImage : System.Windows.Controls.Image
    {
        delegate void OnFrameChangedDelegate();
        private Bitmap m_Bitmap;
        //public string Path { get; set; }
        BitmapSource bitmapSource;

        public void AnimatedImageControl(System.Drawing.Image drawingImage)
        {
            //Path = path;
            m_Bitmap = (Bitmap) drawingImage;
            Width = m_Bitmap.Width;
            Height = m_Bitmap.Height;
            ImageAnimator.Animate(m_Bitmap, OnFrameChanged);
            bitmapSource = GetBitmapSource();
            Source = bitmapSource;
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                   new OnFrameChangedDelegate(OnFrameChangedInMainThread));
        }

        private void OnFrameChangedInMainThread()
        {
            ImageAnimator.UpdateFrames();
            if (bitmapSource != null)
                bitmapSource.Freeze();
            bitmapSource = GetBitmapSource();
            Source = bitmapSource;
            InvalidateVisual();
        }

        //private static bool loaded;
        private BitmapSource GetBitmapSource()
        {
            IntPtr inptr = m_Bitmap.GetHbitmap();
            bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                inptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(inptr);
            return bitmapSource;
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
    }
}
