using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// 图片处理工具类
    /// </summary>
    public class ImageUtil
    {
        /// <summary>
        /// 将弹琴吧原始曲谱图片去除水印
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap CreateIegalTan8Sheet(Bitmap bmp, string title, int curPage, int totalPage, bool addImgMark)
        {
            if(curPage == 1)
            {
                return CreateIegalTan8SheetForPreviewPage(bmp, title, curPage, totalPage, addImgMark);
            }
            else
            {
                return CreateIegalTan8SheetForContentPage(bmp, curPage, totalPage);
            }
        }

        /// <summary>
        /// 去除内容页水印
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="curPage"></param>
        /// <param name="totalPage"></param>
        /// <returns></returns>
        private static Bitmap CreateIegalTan8SheetForContentPage(Bitmap bmp, int curPage, int totalPage)
        {
            int Height = bmp.Height;
            int Width = bmp.Width;
            Bitmap oldBitmap = bmp;
            Bitmap newBitmap = new Bitmap(Width, Height);
            Graphics gs = Graphics.FromImage(newBitmap);
            try
            {
                var firstPixel = oldBitmap.GetPixel(0, 0);
                var firstPixelGrayValue = GetPixelBlackWhiteValue(firstPixel.R, firstPixel.G, firstPixel.B, 2);
                Color pixel;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        int Result = 0;
                        //第一条五线谱以上/尾部所有像素转为白色
                        if (y <= 41 || y > (Height - 45))
                        {
                            Result = firstPixelGrayValue;
                        }
                        else
                        {
                            pixel = oldBitmap.GetPixel(x, y);
                            int r = pixel.R, g = pixel.G, b = pixel.B;
                            Result = GetPixelBlackWhiteValue(r, g, b, 2);
                        }
                        newBitmap.SetPixel(x, y, Color.FromArgb(Result, Result, Result));
                    }
                }
                Brush br = new SolidBrush(Color.Black);
                //绘制页码
                var pageText = string.Format("第 {0} / {1} 页", curPage, totalPage);
                Font pageFont = new Font("微软雅黑", 13);
                SizeF pageSizeF = gs.MeasureString(pageText, pageFont);
                gs.DrawString(pageText, pageFont, br, Width / 2 - pageSizeF.Width / 2, Height - 50);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            } 
            finally
            {
                gs.Dispose();
            }
            return newBitmap;
        }

        /// <summary>
        /// 去除乐谱首页水印
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="title"></param>
        /// <param name="curPage"></param>
        /// <param name="totalPage"></param>
        /// <returns></returns>
        private static Bitmap CreateIegalTan8SheetForPreviewPage(Bitmap bmp, string title, int curPage, int totalPage, bool addImgMark)
        {
            int Height = bmp.Height;
            int Width = bmp.Width;
            Bitmap oldBitmap = bmp;
            Bitmap newBitmap = new Bitmap(Width, Height); ;
            Graphics gs = Graphics.FromImage(newBitmap);
            try
            {
                Color pixel;
                int pixelEachCNText = 14;     //一个文字的宽高 
                int pixelEachFilveLine = 40;  //一条五线高度
                int pixelFiveLineStart = 0; //第一行五线谱的Y轴
                int splitWidth = Width / 5;
                int ckLeftEnd = splitWidth * 2;
                int ckRightStart = splitWidth * 3 ;
                var firstPixel = oldBitmap.GetPixel(0, 0);
                var firstPixelGrayValue = GetPixelBlackWhiteValue(firstPixel.R, firstPixel.G, firstPixel.B, 2);

                //先找到第一条五线谱第一根线的y坐标
                for (int y = 100; y < Height; y++)
                {
                    int xBlackPixelNum = 0;
                    for (int x = 0; x < Width; x++)
                    {
                        pixel = oldBitmap.GetPixel(x, y);
                        int r = pixel.R, g = pixel.G, b = pixel.B, ckFiveLine = 0;
                        ckFiveLine = GetPixelBlackWhiteValue(r, g, b, 2);
                        if(ckFiveLine < 122)
                        {
                            xBlackPixelNum++;
                        }
                    }
                    //如果在一个x轴上, 黑色像素多于特定值, 则是五线谱起点
                    if (xBlackPixelNum > Width * 0.6)
                    {
                        pixelFiveLineStart = y;
                        break;
                    }
                }
                //int fillRightX1 = 0, fillRightX2 = Width, fillRightY1 = pixelFiveLineStart - 12, fillRightY2 = 0;
                ////找到右侧文字坐标范围
                //for (int x = Width - Width / 4; x < Width; x++)
                //{
                //    for (int y = pixelFiveLineStart - pixelEachFilveLine; y < pixelFiveLineStart-12; y++)
                //    {
                //        pixel = oldBitmap.GetPixel(x, y);
                //        int r = pixel.R, g = pixel.G, b = pixel.B, Result = 0;
                //        //如果此处有非白色像素, 检查此像素两个文字高度以下周边, 如果全部是白色像素, 则此处设置为白色
                //        Result = GetPixelBlackWhiteValue(r, g, b, 2);
                //        if (Result > 10)
                //        {
                //            bool convertToWhite = false;
                //            for (int ckx = x; ckx < x + pixelEachCNText * 2; ckx++)
                //            {
                //                for (int cky = y + pixelEachCNText; cky < y + pixelEachCNText * 2; cky++)
                //                {
                //                    int ckResult = GetPixelBlackWhiteValue(r, g, b, 2);
                //                    //如果发现存在黑色像素, 则不需要转换成白色
                //                    if (ckResult < 200)
                //                    {
                //                        convertToWhite = true;
                //                        break;
                //                    }
                //                }
                //            }
                //            if (convertToWhite)
                //            {
                //                Result = 255;
                //            }
                //        }
                //    }
                //}
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        int Result = 0;
                        //第一条五线谱以上/尾部所有像素转为白色
                        if (y <= pixelFiveLineStart - pixelEachFilveLine || y > (Height - 45))
                        {
                            Result = firstPixelGrayValue;
                        }
                        else if (y < pixelFiveLineStart - 12 && (x <= ckLeftEnd || x >= ckRightStart))
                        {
                            pixel = oldBitmap.GetPixel(x, y);
                            int r = pixel.R, g = pixel.G, b = pixel.B;
                            Result = GetPixelBlackWhiteValue(r, g, b, 2);
                            //如果此处有非白色像素, 检查此像素两个文字高度以下周边, 如果全部是白色像素, 则此处设置为白色
                            int tempResult = GetPixelBlackWhiteValue(r, g, b, 3);
                            if (tempResult == 0)
                            {
                                bool breakOut = false;
                                int ckWidth = x > Width / 2 ? Width - 50 : ckLeftEnd;
                                for (int ckx = x; ckx < ckWidth; ckx++)
                                {
                                    for (int cky = y + pixelEachCNText; cky < y + pixelEachCNText * 2; cky++)
                                    {
                                        var ckPixel = oldBitmap.GetPixel(ckx, cky);
                                        int ckResult = GetPixelBlackWhiteValue(ckPixel.R, ckPixel.G, ckPixel.B, 3);
                                        //如果发现存在黑色像素, 需要转换成白色
                                        if (ckResult == 0)
                                        {
                                            Result = firstPixelGrayValue;
                                            breakOut = true;
                                            break;
                                        }
                                    }
                                    if(breakOut)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            pixel = oldBitmap.GetPixel(x, y);
                            int r = pixel.R, g = pixel.G, b = pixel.B;
                            Result = GetPixelBlackWhiteValue(r, g, b, 2);
                        }
                        newBitmap.SetPixel(x, y, Color.FromArgb(Result, Result, Result));
                    }
                }
                if(title.Length > 40)
                {
                    title = title.Substring(0, 40);
                    title += "...";
                }
                Brush br = new SolidBrush(Color.Black);
                //绘制标题
                Font font = new Font("微软雅黑", 24);
                SizeF size = gs.MeasureString(title, font);
                gs.DrawString(title, font, br, Width / 2 - size.Width / 2, 50);
                //绘制页码
                var pageText = string.Format("第 {0} / {1} 页", curPage, totalPage);
                Font pageFont = new Font("微软雅黑", 13);
                SizeF pageSizeF = gs.MeasureString(pageText, pageFont);
                gs.DrawString(pageText, pageFont, br, Width / 2 - pageSizeF.Width / 2, Height - 50);
                if(addImgMark)
                {
                    //绘制喜羊羊遮挡
                    BitmapImage resourceBmp = new BitmapImage(new Uri(
                        string.Format("pack://application:,,,/{0};component/Assets/Images/happy_sheep.jpg", System.Reflection.Assembly.GetEntryAssembly().GetName().Name), 
                        UriKind.RelativeOrAbsolute));
                    Bitmap happySheep = BitmapImage2Bitmap(resourceBmp);
                    gs.DrawImage(happySheep, Width / 2 - happySheep.Width / 2, Height / 2 - happySheep.Height / 2, happySheep.Width, happySheep.Height);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                gs.Dispose();
            }
            return newBitmap;
        }

        private static int GetPixelBlackWhiteValue(int r, int g, int b, int calcType)
        {
            int Result = 0;
            switch (calcType)
            {
                case 0://平均值法
                    Result = ((r + g + b) / 3);
                    break;
                case 1://最大值法
                    Result = r > g ? r : g;
                    Result = Result > b ? Result : b;
                    break;
                case 2://加权平均值法
                    Result = ((int)(0.7 * r) + (int)(0.2 * g) + (int)(0.1 * b));
                    break;
                case 3: //
                    Result = ((r + g + b) / 3) > 0 ? 0 : 255;
                    break;
            }
            return Result;
        }

        /// <summary>
        /// 将bmpimg转换为 System.Drawing.Bitmap
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
