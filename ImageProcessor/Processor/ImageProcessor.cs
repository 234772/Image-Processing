using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Processor
{
    public static class ImageProcessor
    {
        private static ImageHandler ih = new ImageHandler();
        public static ImageHandler Ih { get { return ih; } }

        private static int Truncate(int pixelValue)
        {
            if(pixelValue > 255)
                return 255;
            else if (pixelValue < 0)
                return 0;
            else
                return pixelValue;
        }
        public static void ChangeBrightness(int changeValue)
        {
            Bitmap bmp = new Bitmap(ih.Bmp.Width, ih.Bmp.Height);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color color = ih.Bmp.GetPixel(y, x);
                    Color newColor = Color.FromArgb(Truncate(color.R + changeValue), Truncate(color.G + changeValue), Truncate(color.B + changeValue));
                    //Console.WriteLine(color.ToString());
                    bmp.SetPixel(y, x, newColor);
                }
            }

            ih.Bmp = bmp;
        }
        public static void NegativeImage()
        {
            Bitmap bmp = new Bitmap(ih.Bmp.Width, ih.Bmp.Height);

            for(int y = 0; y < bmp.Height; y++)
            {
                for(int x = 0; x < bmp.Width; x++)
                {
                    Color color = ih.Bmp.GetPixel(y, x);
                    Color newColor = Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
                    //Console.WriteLine(color.ToString());
                    bmp.SetPixel(y, x, newColor);
                }
            }

            ih.Bmp = bmp;
        }
        public static void BilinearResizing(int nWidth, int nHeight)
        {
            var b = new Bitmap(nWidth, nHeight);

            double nXFactor = (double)Ih.Bmp.Width / (double)nWidth;
            double nYFactor = (double)Ih.Bmp.Height / (double)nHeight;

            double fraction_x, fraction_y, one_minus_x, one_minus_y;
            int ceil_x, ceil_y, floor_x, floor_y;

            Color c1 = new Color();
            Color c2 = new Color();
            Color c3 = new Color();
            Color c4 = new Color();

            byte red, green, blue;

            byte b1, b2;

            for (int x = 0; x < b.Width; ++x)
                for (int y = 0; y < b.Height; ++y)
                {
                    // Setup

                    floor_x = (int)Math.Floor(x * nXFactor);
                    floor_y = (int)Math.Floor(y * nYFactor);

                    ceil_x = floor_x + 1;

                    if (ceil_x >= Ih.Bmp.Width)
                        ceil_x = floor_x;

                    ceil_y = floor_y + 1;

                    if (ceil_y >= Ih.Bmp.Height)
                        ceil_y = floor_y;

                    fraction_x = x * nXFactor - floor_x;
                    fraction_y = y * nYFactor - floor_y;

                    one_minus_x = 1.0 - fraction_x;
                    one_minus_y = 1.0 - fraction_y;

                    c1 = Ih.Bmp.GetPixel(floor_x, floor_y);
                    c2 = Ih.Bmp.GetPixel(ceil_x, floor_y);
                    c3 = Ih.Bmp.GetPixel(floor_x, ceil_y);
                    c4 = Ih.Bmp.GetPixel(ceil_x, ceil_y);

                    // Blue
                    b1 = (byte)(one_minus_x * c1.B + fraction_x * c2.B);
                    b2 = (byte)(one_minus_x * c3.B + fraction_x * c4.B);

                    blue = (byte)(one_minus_y * (double)(b1) + fraction_y * (double)(b2));

                    // Green
                    b1 = (byte)(one_minus_x * c1.G + fraction_x * c2.G);
                    b2 = (byte)(one_minus_x * c3.G + fraction_x * c4.G);

                    green = (byte)(one_minus_y * (double)(b1) + fraction_y * (double)(b2));

                    // Red
                    b1 = (byte)(one_minus_x * c1.R + fraction_x * c2.R);
                    b2 = (byte)(one_minus_x * c3.R + fraction_x * c4.R);

                    red = (byte)(one_minus_y * (double)(b1) + fraction_y * (double)(b2));

                    b.SetPixel(x, y, Color.FromArgb(255, red, green, blue));
                }
            ih.Bmp = b;
        }
        public static void HorizontalFlip()
        {
            Bitmap bmp = new Bitmap(ih.Bmp.Width, ih.Bmp.Height);
            int horizontalPixel = 0;

            for (int y = 0; y < bmp.Height - 1; y++)
            {
                for(int x = bmp.Width - 1; x >= 0 ; x--)
                {
                    bmp.SetPixel(horizontalPixel++,y,ih.Bmp.GetPixel(x, y));
                }
                horizontalPixel = 0;
            }
            ih.Bmp = bmp;
        }
        public static void VerticalFlip()
        {
            Bitmap bmp = new Bitmap(ih.Bmp.Width, ih.Bmp.Height);
            int verticalPixel = 0;

            for (int x = 0; x < ih.Bmp.Width - 1; x++)
            {
                for (int y = ih.Bmp.Height - 1; y >=0; y--)
                {
                    bmp.SetPixel(x, verticalPixel++, ih.Bmp.GetPixel(x, y));
                }
                verticalPixel = 0;
            }
            ih.Bmp = bmp;
        }
        public static void DiagonalFlip()
        {
            Bitmap bmp = new Bitmap(ih.Bmp.Width, ih.Bmp.Height);
            int pixel1 = 0;
            int pixel2 = 0;

            for (int y = ih.Bmp.Height - 1; y > 0; y--)
            {
                for (int x = ih.Bmp.Width - 1; x > 0; x--)
                {
                    bmp.SetPixel(pixel1, pixel2, ih.Bmp.GetPixel(x, y));
                    pixel1++;
                }
                pixel1 = 0;
                pixel2++;
            }
            ih.Bmp = bmp;
        }
        public static void Contrast()
        {
            Bitmap bmp = new Bitmap(ih.Bmp.Width, ih.Bmp.Height);

            byte lowPixel = 255;
            byte highPixel = 0;

            for(int i = 0; i < ih.Bmp.Height; i++)
            {
                for(int j = 0; j < ih.Bmp.Width; j++)
                {
                    if(lowPixel > ih.Bmp.GetPixel(i, j).B)
                        lowPixel = ih.Bmp.GetPixel(i, j).B;
                    if(highPixel < ih.Bmp.GetPixel(i, j).B)
                        highPixel = ih.Bmp.GetPixel(i, j).B;
                }
            }

            for (int i = 0; i < ih.Bmp.Height; i++)
            {
                for (int j = 0; j < ih.Bmp.Width; j++)
                {
                    var contrastPixel = (ih.Bmp.GetPixel(j, i).B - lowPixel) * ((255 - 0) / (highPixel - lowPixel)) + 0;
                    Color color = Color.FromArgb(contrastPixel, contrastPixel, contrastPixel);
                    bmp.SetPixel(j, i, color);
                }
            }
            ih.Bmp = bmp;
        }
        public static void Contrast2()
        {
            int width = ih.Bmp.Width;
            int height = ih.Bmp.Height;

            Bitmap bmp = new Bitmap(width, height);

            double[] probability = new double[256];
            double[] probabilityR = new double[256];
            double[] probabilityG = new double[256];
            double[] probabilityB = new double[256];

            byte[,] intensity = new byte[width, height];
            byte[,] r = new byte[width, height];
            byte[,] g = new byte[width, height];
            byte[,] b = new byte[width, height];

            for (int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    r[i, j] = (byte)ih.Bmp.GetPixel(i, j).R;
                    g[i, j] = (byte)ih.Bmp.GetPixel(i, j).G;
                    b[i, j] = (byte)ih.Bmp.GetPixel(i, j).B;

                    intensity[i, j] = (byte)((r[i, j] + g[i, j] + b[i, j]) / 3);

                    //Console.Write(r[j, i] + " " + g[j, i] + " " + b[j, i] + " " + intensity[j, i]);
                    //Console.WriteLine();
                }
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    probability[intensity[i, j]]++;
                    probabilityR[r[i, j]]++;
                    probabilityG[g[i, j]]++;
                    probabilityB[b[i, j]]++;
                }
            }

            for(int i = 0; i < probability.Length; i++)
            {
                probability[i] /= (width * height);
                probabilityR[i] /= (width * height);
                probabilityG[i] /= (width * height);
                probabilityB[i] /= (width * height);
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double sum = 0;
                    double sumR = 0;
                    double sumG = 0;
                    double sumB = 0;

                    for (int x = 0; x < intensity[i, j]; x++)
                    {
                        sum += probability[x];
                    }
                    for (int x = 0; x < r[i, j]; x++)
                    {
                        sumR += probabilityR[x];
                    }
                    for (int x = 0; x < g[i, j]; x++)
                    {
                        sumG += probabilityG[x];
                    }
                    for (int x = 0; x < b[i, j]; x++)
                    {
                        sumB += probabilityB[x];
                    }
                    bmp.SetPixel(i, j, Color.FromArgb((byte)(Math.Floor(255 * sumR)), (byte)(Math.Floor(255 * sumG)), (byte)(Math.Floor(255 * sumB))));
                }
            }
            ih.Bmp = bmp;
        }
        public static void Contrast3()
        {
            int w = ih.Bmp.Width;
            int h = ih.Bmp.Height;
            BitmapData sd = ih.Bmp.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = sd.Stride * sd.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(sd.Scan0, buffer, 0, bytes);
            ih.Bmp.UnlockBits(sd);
            int current = 0;
            double[] pn = new double[256];
            for (int p = 0; p < bytes; p += 4)
            {
                pn[buffer[p]]++;
            }
            for (int prob = 0; prob < pn.Length; prob++)
            {
                pn[prob] /= (w * h);
            }
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    current = y * sd.Stride + x * 4;
                    double sum = 0;
                    for (int i = 0; i < buffer[current]; i++)
                    {
                        sum += pn[i];
                    }
                    for (int c = 0; c < 3; c++)
                    {
                        result[current + c] = (byte)Math.Floor(255 * sum);
                    }
                    result[current + 3] = 255;
                }
            }
            Bitmap res = new Bitmap(w, h);
            BitmapData rd = res.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, rd.Scan0, bytes);
            res.UnlockBits(rd);
            ih.Bmp = res;
        }
    }
}
