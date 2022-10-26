using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Processor
{
    public static class ImageProcessor
    {
        private static ImageHandler ih = new ImageHandler();
        public static ImageHandler Ih { get { return ih; } }

        public static void Process(CommandLineOptions o)
        {
            Bitmap bmp;
            if (o == null)
                // TODO: Add null handling
                return;
            if (o.firstPath == null || o.secondPath == null)
                // TODO: Add null handling
                return;
            else
                bmp = ih.loadImage(o.firstPath);
            if (o.brightness != 0)
                ChangeBrightness(bmp, o.secondPath, o.brightness);
            if (o.contrast)
                Contrast2(bmp, o.secondPath);
            if (o.negative)
                NegativeImage(bmp, o.secondPath);
            if (o.Dimensions.Count() > 0)
            {
                List<int> dimensions = new List<int>(o.Dimensions);
                BilinearResizing(bmp, o.secondPath, dimensions[0], dimensions[1]);
            }
            if (o.hflip)
                HorizontalFlip(bmp, o.secondPath);
            if (o.vflip)
                VerticalFlip(bmp, o.secondPath);
            if (o.dflip)
                DiagonalFlip(bmp, o.secondPath);
            if (o.ValuesA.Count() > 0)
            {
                List<int> values = new List<int>(o.ValuesA);
                AlphaTrimmedFilter(bmp, o.secondPath, values[0], values[1], values[2]); 
            }
            if (o.ValuesG.Count() > 0)
            {
                List<int> values = new List<int>(o.ValuesG);
                GeometricMeanFilter(bmp, o.secondPath, values[0], values[1]);
            }
            if (o.meanSquare)
                Console.WriteLine(MeanSquareError(o.firstPath, o.secondPath));
            if (o.peakMeanSquare)
                Console.WriteLine(PeakMeanSquareError(o.firstPath, o.secondPath));
            if (o.maximumDifference)
                Console.WriteLine(MaximumDifference(o.firstPath, o.secondPath));
            if (o.signalToNoiseRatio)
                Console.WriteLine(SignalToNoiseRatio(o.firstPath, o.secondPath));
            if (o.peakSignalToNoiseRatio)
                Console.WriteLine(PeakSignalToNoiseRatio(o.firstPath, o.secondPath));


        }
        private static byte Truncate(byte pixelValue1, int changeValue)
        {
            int p1 = pixelValue1;
            int p2 = changeValue;
            if(p1 + p2 > 255)
                return 255;
            else if (p1 + p2 < 0)
                return 0;
            else
                return (byte)(p1 + p2);
        }
        public static void ChangeBrightness(Bitmap image, string savePath, int changeValue)
        {
            Bitmap bmp = new Bitmap(image);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            IntPtr ptr = bmpData.Scan0;

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            for(int i = 0; i < bytes; i++)
            {
                rgbValues[i] = Truncate(rgbValues[i], changeValue);
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(bmpData);
            //for (int y = 0; y < bmp.Height; y++)
            //{
            //    for (int x = 0; x < bmp.Width; x++)
            //    {
            //        Color color = image.GetPixel(y, x);
            //        Color newColor = Color.FromArgb(Truncate(color.R + changeValue), Truncate(color.G + changeValue), Truncate(color.B + changeValue));
            //        //Console.WriteLine(color.ToString());
            //        bmp.SetPixel(y, x, newColor);
            //    }
            //}

            ih.saveImage(bmp, savePath);
        }
        public static void NegativeImage(Bitmap image, string savePath)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);

            for(int y = 0; y < bmp.Height; y++)
            {
                for(int x = 0; x < bmp.Width; x++)
                {
                    Color color = image.GetPixel(y, x);
                    Color newColor = Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
                    bmp.SetPixel(y, x, newColor);
                }
            }

            ih.saveImage(bmp, savePath);
        }
        public static void BilinearResizing(Bitmap image, string savePath, int nWidth, int nHeight)
        {
            var b = new Bitmap(nWidth, nHeight);

            double nXFactor = (double)image.Width / (double)nWidth;
            double nYFactor = (double)image.Height / (double)nHeight;

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

                    if (ceil_x >= image.Width)
                        ceil_x = floor_x;

                    ceil_y = floor_y + 1;

                    if (ceil_y >= image.Height)
                        ceil_y = floor_y;

                    fraction_x = x * nXFactor - floor_x;
                    fraction_y = y * nYFactor - floor_y;

                    one_minus_x = 1.0 - fraction_x;
                    one_minus_y = 1.0 - fraction_y;

                    c1 = image.GetPixel(floor_x, floor_y);
                    c2 = image.GetPixel(ceil_x, floor_y);
                    c3 = image.GetPixel(floor_x, ceil_y);
                    c4 = image.GetPixel(ceil_x, ceil_y);

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

            ih.saveImage(b, savePath);
        }
        public static void HorizontalFlip(Bitmap image, string savePath)
        {
            // TODO: Check if it works - doesn't for me
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            int horizontalPixel = 0;

            for (int y = 0; y < image.Height - 1; y++)
            {
                for(int x = image.Width - 1; x >= 0 ; x--)
                {
                    bmp.SetPixel(horizontalPixel++, y, image.GetPixel(x, y));
                }
                horizontalPixel = 0;
            }
            ih.saveImage(bmp, savePath);
        }
        public static void VerticalFlip(Bitmap image, string savePath)
        {

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] rgbValues = new byte[bmpData.Stride * image.Height];
            byte[] newBmp = new byte[bmpData.Stride * image.Height];

            byte[] bytesR = new byte[image.Height * image.Width];
            byte[] bytesB = new byte[image.Height * image.Width];
            byte[] bytesG = new byte[image.Height * image.Width];

            IntPtr ptr = bmpData.Scan0;

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, image.Height * bmpData.Stride);

            int k = 0;

            for(int i = 0; i < image.Width * image.Height; i++)
            {
                bytesB[i] = rgbValues[k];
                bytesG[i] = rgbValues[k+1];
                bytesR[i] = rgbValues[k+2];
                k += 3;
            }

            int val;
            int p = 0;

            for(int i = 0; i < image.Height; i++)
            {
                for(int z = 0; z < bmpData.Stride; z+=3)
                {
                    val = ((image.Height - i) * bmpData.Stride) - bmpData.Stride + z;
                    newBmp[val] = bytesB[p];
                    newBmp[val + 1] = bytesG[p];
                    newBmp[val + 2] = bytesR[p];
                    p++;
                }
            }
            Marshal.Copy(newBmp, 0, ptr, bmpData.Stride * bmpData.Width);
            image.UnlockBits(bmpData);
  
            ih.saveImage(image, savePath);
        }
        public static void DiagonalFlip(Bitmap image, string savePath)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            int pixel1 = 0;
            int pixel2 = 0;

            for (int y = image.Height - 1; y > 0; y--)
            {
                for (int x = image.Width - 1; x > 0; x--)
                {
                    bmp.SetPixel(pixel1, pixel2, image.GetPixel(x, y));
                    pixel1++;
                }
                pixel1 = 0;
                pixel2++;
            }
            ih.saveImage(bmp, savePath);
        }
        public static void Contrast2(Bitmap image, string savePath)
        {
            int width = image.Width;
            int height = image.Height;

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
                    r[i, j] = (byte)image.GetPixel(i, j).R;
                    g[i, j] = (byte)image.GetPixel(i, j).G;
                    b[i, j] = (byte)image.GetPixel(i, j).B;

                    intensity[i, j] = (byte)((r[i, j] + g[i, j] + b[i, j]) / 3);
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
            ih.saveImage(bmp, savePath);
        }
        public static void Contrast3(Bitmap image)
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
        public static Bitmap ExtendBitmapByOne(Bitmap image)
        {
            Bitmap res = new Bitmap(image.Width + 2, image.Height + 2);

            for(int i = 1; i <= image.Height; i++)
            {
                for(int j = 1; j <= image.Width; j++)
                {
                    if (i == 1)
                        res.SetPixel(j, i - 1, image.GetPixel(j - 1, i - 1));
                    else if (i == image.Height)
                        res.SetPixel(j, i + 1, image.GetPixel(j - 1, i - 1));
                    if (j == 1)
                        res.SetPixel(j - 1, i, image.GetPixel(j - 1, i - 1));
                    else if (j == image.Width)
                        res.SetPixel(j + 1, i, image.GetPixel(j - 1, i - 1));

                    res.SetPixel(i, j, image.GetPixel(i - 1, j - 1));
                }
            }
            res.SetPixel(0, 0, res.GetPixel(1, 0));
            res.SetPixel(res.Width - 1, 0, res.GetPixel(res.Width - 2, 0));
            res.SetPixel(0, res.Height - 1, res.GetPixel(0, res.Height - 2));
            res.SetPixel(res.Width - 1, res.Height - 1, res.GetPixel(res.Height - 2, res.Width - 1));
            return res;
        }
        public static void AlphaTrimmedFilter(Bitmap image, string savePath, int alpha, int maskM, int maskN)
        {
            int m = maskM;
            int n = maskN;

            if (m % 2 == 0) m++;
            if (n % 2 == 0) n++;

            int radiusN = (int)Math.Floor(maskN / 2.0);
            int radiusM = (int)Math.Floor(maskM / 2.0);

            int height = ih.Bmp.Height;
            int width = ih.Bmp.Width;

            Bitmap res = new Bitmap(image.Width, image.Height);
            Bitmap buffer = ExtendBitmapByOne(image);

            ///Run through every pixel of the original image(not buffer)
            ///
            
            for(int i = 1; i < buffer.Height - 1; i++)
            {
                for(int j = 1; j < buffer.Width - 1; j++)
                {
                    ///Put a 3x3 mask on every pixel of the image(including buffer, as we need the borders)
                    int k = 0;
                    int meanR = 0;
                    int meanG = 0;
                    int meanB = 0;

                    Color[] maskR = new Color[m * n];
                    Color[] maskG = new Color[m * n];
                    Color[] maskB = new Color[m * n];

                    for (int x = i - radiusN; x < i + radiusN + 1; x++)
                    {
                        if (x < 0) continue;
                        if (x >= buffer.Height) break;
                        for (int y = j - radiusM; y < j + radiusM + 1; y++)
                        {
                            //Console.WriteLine(k);
                            if (y < 0) continue;
                            if (y >= buffer.Width) break;
                            Color value;
                            value = buffer.GetPixel(y, x);
                            maskR[k] = value;
                            maskG[k] = value;
                            maskB[k] = value;
                            k++;
                        }
                    }

                    ///Order the mask, so we can trim the border values
                    Array.Sort(maskR, (x, y) => x.R.CompareTo(y.R));
                    Array.Sort(maskG, (x, y) => x.G.CompareTo(y.G));
                    Array.Sort(maskB, (x, y) => x.B.CompareTo(y.B));

                    List<Color> colorsR = new List<Color>(maskR);
                    List<Color> colorsG = new List<Color>(maskG);
                    List<Color> colorsB = new List<Color>(maskB);

                    ///Remove alpha elements from both sides of the sorted arrayList
                    for (int l = 0; l < alpha; l++)
                    {
                        colorsR.RemoveAt(l);
                        colorsR.RemoveAt(colorsR.Count - l - 1);

                        colorsG.RemoveAt(l);
                        colorsG.RemoveAt(colorsG.Count - l - 1);

                        colorsB.RemoveAt(l);
                        colorsB.RemoveAt(colorsB.Count - l - 1);
                    }



                    ///Calculate the mean value of the mask
                    meanR = colorsR.Sum(x => x.R) / colorsR.Count;
                    meanG = colorsG.Sum(x => x.G) / colorsG.Count;
                    meanB = colorsB.Sum(x => x.B) / colorsB.Count;

                    ///Assign the mean value to the target pixel
                    res.SetPixel(j - 1, i - 1, Color.FromArgb(meanR, meanG, meanB));
                }
            }
            ih.saveImage(res, savePath);
        }
        public static void GeometricMeanFilter(Bitmap image, string savePath, int m, int n)
        {
            int maskM = m;
            int maskN = n;

            if (m % 2 == 0) m++;
            if (n % 2 == 0) n++;

            int radiusN = (int)Math.Floor(maskN / 2.0);
            int radiusM = (int)Math.Floor(maskM / 2.0);

            Bitmap res = new Bitmap(image.Width, image.Height);
            Bitmap buffer = ExtendBitmapByOne(image);

            ///Run through every pixel of the original image(not buffer)
            for (int i = 1; i < buffer.Height - 1; i++)
            {
                for (int j = 1; j < buffer.Width - 1; j++)
                {
                    ///Put a 3x3 mask on every pixel of the image(including buffer, as we need the borders)
                    int k = 0;

                    Color[] maskR = new Color[m * n];
                    Color[] maskG = new Color[m * n];
                    Color[] maskB = new Color[m * n];

                    for (int x = i - radiusN; x < i + radiusN + 1; x++)
                    {
                        if (x < 0) continue;
                        if (x >= buffer.Height) break;
                        for (int y = j - radiusM; y < j + radiusM + 1; y++)
                        {
                            //Console.WriteLine(k);
                            if (y < 0) continue;
                            if (y >= buffer.Width) break;
                            Color value;
                            value = buffer.GetPixel(y, x);
                            maskR[k] = value;
                            maskG[k] = value;
                            maskB[k] = value;
                            k++;
                        }
                    }

                    double productR = maskR[0].R;
                    double productG = maskG[0].G;
                    double productB = maskB[0].B;

                    for(int x = 1; x < maskR.Length; x++)
                    {
                        productR *= maskR[x].R;
                        productG *= maskG[x].G;
                        productB *= maskB[x].B;
                    }

                    double divider = 1.0 / (m * n);

                    ///Assign the geometric product value to the target pixel
                    res.SetPixel(j - 1, i - 1, Color.FromArgb((int)Math.Pow(productR, divider), (int)Math.Pow(productG, divider), (int)Math.Pow(productB, divider)));
                }
            }
            ih.saveImage(res, savePath);
        }
        public static int MeanSquareError(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);

            int M = bmp1.Height;
            int N = bmp1.Width;

            Double sumOfSquaresR = 0;
            Double sumOfSquaresG = 0;
            Double sumOfSquaresB = 0;
            int mse;
           
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Color pixel1 = bmp1.GetPixel(j, i);
                    Color pixel2 = bmp2.GetPixel(j, i);

                    sumOfSquaresR += Math.Pow(pixel1.R - pixel2.R, 2);
                    sumOfSquaresG += Math.Pow(pixel1.G - pixel2.G, 2);
                    sumOfSquaresB += Math.Pow(pixel1.B - pixel2.B, 2);
                }
            }
            mse = (int)((sumOfSquaresR + sumOfSquaresG + sumOfSquaresB) / (3 * (M * N)));
            return mse;
        }
        public static Double PeakMeanSquareError(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);

            Double M = bmp1.Height;
            Double N = bmp1.Width;

            Double sumOfSquaresR = 0;
            Double sumOfSquaresG = 0;
            Double sumOfSquaresB = 0;

            Double maxR = 0;
            Double maxG = 0;
            Double maxB = 0;

            Double pmse;

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Color pixel1 = bmp1.GetPixel(j, i);
                    Color pixel2 = bmp2.GetPixel(j, i);

                    if(pixel1.R > maxR)
                        maxR = pixel1.R;
                    if(pixel1.G > maxG)
                        maxG = pixel1.G;
                    if(pixel1.B > maxB)
                        maxB = pixel1.B;

                    sumOfSquaresR += Math.Pow(pixel1.R - pixel2.R, 2);
                    sumOfSquaresG += Math.Pow(pixel1.G - pixel2.G, 2);
                    sumOfSquaresB += Math.Pow(pixel1.B - pixel2.B, 2);
                }
            }
            pmse = (sumOfSquaresR + sumOfSquaresG + sumOfSquaresB) / (3 * (M * N) * Math.Pow((maxR + maxG + maxB) / 3, 2));

            return pmse;
        }
        public static int MaximumDifference(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);

            int M = bmp1.Height;
            int N = bmp1.Width;

            Double maxDiff = 0;

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Color pixel1 = bmp1.GetPixel(j, i);
                    Color pixel2 = bmp2.GetPixel(j, i);

                    int redDiff;
                    int greenDiff;
                    int blueDiff;

                    redDiff = Math.Abs(pixel1.R - pixel2.R);
                    greenDiff = Math.Abs(pixel1.G - pixel2.G);
                    blueDiff= Math.Abs(pixel1.B - pixel2.B);

                    Double sumDiff = (redDiff + greenDiff + blueDiff) / 3;

                    if(sumDiff > maxDiff)
                        maxDiff = sumDiff;
                }
            }

            return (int)maxDiff;
        }
        public static Double SignalToNoiseRatio(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);

            int M = bmp1.Height;
            int N = bmp1.Width;

            Double sumOfSquaresR = 0;
            Double sumOfSquaresG = 0;
            Double sumOfSquaresB = 0;

            Double sumOfSquarePixelR = 0;
            double sumOfSquarePixelG = 0;
            double sumOfSquarePixelB = 0;

            double snr;

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Color pixel1 = bmp1.GetPixel(j, i);
                    Color pixel2 = bmp2.GetPixel(j, i);

                    sumOfSquaresR += Math.Pow(pixel1.R - pixel2.R, 2);
                    sumOfSquaresG += Math.Pow(pixel1.G - pixel2.G, 2);
                    sumOfSquaresB += Math.Pow(pixel1.B - pixel2.B, 2);

                    sumOfSquarePixelR += Math.Pow(pixel1.R, 2);
                    sumOfSquarePixelG += Math.Pow(pixel1.G, 2);
                    sumOfSquarePixelB += Math.Pow(pixel1.B, 2);
                }
            }
            snr = 10 * Math.Log10((sumOfSquarePixelR + sumOfSquarePixelG + sumOfSquarePixelB) / (sumOfSquaresR + sumOfSquaresG + sumOfSquaresB));

            return snr;
        }
        public static Double PeakSignalToNoiseRatio(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);

            int M = bmp1.Height;
            int N = bmp1.Width;

            Double sumOfSquaresR = 0;
            Double sumOfSquaresG = 0;
            Double sumOfSquaresB = 0;

            Double maxR = 0;
            Double maxG = 0;
            Double maxB = 0;

            double psnr;

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Color pixel1 = bmp1.GetPixel(j, i);
                    Color pixel2 = bmp2.GetPixel(j, i);

                    sumOfSquaresR += Math.Pow(pixel1.R - pixel2.R, 2);
                    sumOfSquaresG += Math.Pow(pixel1.G - pixel2.G, 2);
                    sumOfSquaresB += Math.Pow(pixel1.B - pixel2.B, 2);

                    if (pixel1.R > maxR)
                        maxR = pixel1.R;
                    if (pixel1.G > maxG)
                        maxG = pixel1.G;
                    if (pixel1.B > maxB)
                        maxB = pixel1.B;
                }
            }
            maxR = Math.Pow(maxR, 2);
            maxG = Math.Pow(maxG, 2);
            maxB = Math.Pow(maxB, 2);

            psnr = 10 * Math.Log10((maxR + maxG + maxB) / (sumOfSquaresR + sumOfSquaresG + sumOfSquaresB));

            return 20 * Math.Log10(255 + 255 + 255) - 10 * Math.Log10(MeanSquareError(firstImage, secondImage));
        }
    }
}
