using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
                ChangeContrast(bmp, o.secondPath);
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
            {
                //Console.WriteLine(MeanSquareError(o.firstPath, o.secondPath));
                Console.WriteLine(MeanSquareErrorAsync(o.firstPath, o.secondPath));
            }
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
            if (p1 + p2 > 255)
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

            for (int i = 0; i < bytes; i++)
            {
                rgbValues[i] = Truncate(rgbValues[i], changeValue);
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(bmpData);

            ih.saveImage(bmp, savePath);
        }
        public static void NegativeImage(Bitmap image, string savePath)
        {
            Bitmap bmp = new Bitmap(image);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            IntPtr ptr = bmpData.Scan0;

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int i = 0; i < bytes; i++)
            {
                rgbValues[i] = (byte)(255 - rgbValues[i]);
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(bmpData);

            ih.saveImage(bmp, savePath);
        }
        public static void BilinearResizing(Bitmap image, string savePath, int nWidth, int nHeight)
        {
            double nXFactor = (double)image.Width / (double)nWidth;
            double nYFactor = (double)image.Height / (double)nHeight;
            double fraction_x, fraction_y, one_minus_x, one_minus_y;
            int ceil_x, ceil_y, floor_x, floor_y;
            byte red, green, blue;
            byte b1, b2;
            int bytesPerPixel = 3;

            Bitmap resultBitmap = new Bitmap(nWidth, nHeight);
            BitmapData resultBitmapData = resultBitmap.LockBits(new Rectangle(0, 0, nWidth, nHeight), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb); ;

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int allBytes = bmpData.Stride * bmpData.Height;
            byte[] originalRGB = new byte[allBytes];
            byte[] resultRGB = new byte[resultBitmapData.Stride * nHeight];
            Marshal.Copy(bmpData.Scan0, originalRGB, 0, bmpData.Height * bmpData.Stride);

            for (int x = 0; x < nWidth; ++x)
            {
                for (int y = 0; y < nHeight; ++y)
                {
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

                    byte c1B = originalRGB[floor_y * bmpData.Stride + floor_x * bytesPerPixel];
                    byte c1G = originalRGB[floor_y * bmpData.Stride + floor_x * bytesPerPixel + 1];
                    byte c1R = originalRGB[floor_y * bmpData.Stride + floor_x * bytesPerPixel + 2];

                    byte c2B = originalRGB[floor_y * bmpData.Stride + ceil_x * bytesPerPixel];
                    byte c2G = originalRGB[floor_y * bmpData.Stride + ceil_x * bytesPerPixel + 1];
                    byte c2R = originalRGB[floor_y * bmpData.Stride + ceil_x * bytesPerPixel + 2];

                    byte c3B = originalRGB[ceil_y * bmpData.Stride + floor_x * bytesPerPixel];
                    byte c3G = originalRGB[ceil_y * bmpData.Stride + floor_x * bytesPerPixel + 1];
                    byte c3R = originalRGB[ceil_y * bmpData.Stride + floor_x * bytesPerPixel + 2];

                    byte c4B = originalRGB[ceil_y * bmpData.Stride + ceil_x * bytesPerPixel];
                    byte c4G = originalRGB[ceil_y * bmpData.Stride + ceil_x * bytesPerPixel + 1];
                    byte c4R = originalRGB[ceil_y * bmpData.Stride + ceil_x * bytesPerPixel + 2];
                                 
                    // Blue
                    b1 = (byte)(one_minus_x * c1B + fraction_x * c2B);
                    b2 = (byte)(one_minus_x * c3B + fraction_x * c4B);
                    blue = (byte)(one_minus_y * (double)(b1) + fraction_y * (double)(b2));

                    // Green
                    b1 = (byte)(one_minus_x * c1G + fraction_x * c2G);
                    b2 = (byte)(one_minus_x * c3G + fraction_x * c4G);
                    green = (byte)(one_minus_y * (double)(b1) + fraction_y * (double)(b2));

                    // Red
                    b1 = (byte)(one_minus_x * c1R + fraction_x * c2R);
                    b2 = (byte)(one_minus_x * c3R + fraction_x * c4R);
                    red = (byte)(one_minus_y * (double)(b1) + fraction_y * (double)(b2));

                    resultRGB[y * resultBitmapData.Stride + x * bytesPerPixel] = blue;
                    resultRGB[y * resultBitmapData.Stride + x * bytesPerPixel + 1] = green;
                    resultRGB[y * resultBitmapData.Stride + x * bytesPerPixel + 2] = red;
                }
            }
            Marshal.Copy(resultRGB, 0, resultBitmapData.Scan0, resultBitmapData.Stride * nHeight);
            resultBitmap.UnlockBits(resultBitmapData);
            ih.saveImage(resultBitmap, savePath);
        }

        public static void HorizontalFlip(Bitmap image, string savePath)
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

            for (int i = 0; i < image.Width * image.Height; i++)
            {
                bytesB[i] = rgbValues[k];
                bytesG[i] = rgbValues[k + 1];
                bytesR[i] = rgbValues[k + 2];
                k += 3;
            }

            int val;
            int p = 0;

            for (int y = 0; y < bmpData.Height; y++)
            {
                for (int x = 0; x < bmpData.Stride; x += 3)
                {
                    val = bmpData.Stride + y * bmpData.Stride - x - 1;
                    newBmp[val] = bytesR[p];
                    newBmp[val - 1] = bytesG[p];
                    newBmp[val - 2] = bytesB[p];
                    p++;
                }
            }

            Marshal.Copy(newBmp, 0, ptr, bmpData.Stride * bmpData.Width);
            image.UnlockBits(bmpData);

            ih.saveImage(image, savePath);
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

            for (int i = 0; i < image.Width * image.Height; i++)
            {
                bytesB[i] = rgbValues[k];
                bytesG[i] = rgbValues[k + 1];
                bytesR[i] = rgbValues[k + 2];
                k += 3;
            }

            int val;
            int p = 0;

            for (int i = 0; i < image.Height; i++)
            {
                for (int z = 0; z < bmpData.Stride; z += 3)
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

            for (int i = 0; i < image.Width * image.Height; i++)
            {
                bytesB[i] = rgbValues[k];
                bytesG[i] = rgbValues[k + 1];
                bytesR[i] = rgbValues[k + 2];
                k += 3;
            }

            int val;
            int p = 0;

            for (int y = image.Height - 2; y > 0; y--)
            {
                for (int x = 0; x < bmpData.Stride; x += 3)
                {
                    val = bmpData.Stride + y * bmpData.Stride - x - 1;
                    newBmp[val] = bytesR[p];
                    newBmp[val - 1] = bytesG[p];
                    newBmp[val - 2] = bytesB[p];
                    p++;
                }
            }
            Marshal.Copy(newBmp, 0, ptr, bmpData.Stride * bmpData.Width);
            image.UnlockBits(bmpData);

            ih.saveImage(image, savePath);
        }
        public static void ChangeContrast(Bitmap image, string savePath)
        {
            int width = image.Width;
            int height = image.Height;

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];
            byte[] newBmp = new byte[bytes];

            double[] probabilityR = new double[256];
            double[] probabilityG = new double[256];
            double[] probabilityB = new double[256];

            byte[,] r = new byte[width, height];
            byte[,] g = new byte[width, height];
            byte[,] b = new byte[width, height];

            IntPtr ptr = bmpData.Scan0;

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, image.Height * bmpData.Stride);

            int k = 0;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    b[j, i] = rgbValues[k];
                    g[j, i] = rgbValues[k + 1];
                    r[j, i] = rgbValues[k + 2];
                    k += 3;
                }
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    probabilityR[r[i, j]]++;
                    probabilityG[g[i, j]]++;
                    probabilityB[b[i, j]]++;
                }
            }

            for (int i = 0; i < 256; i++)
            {
                probabilityR[i] /= (width * height);
                probabilityG[i] /= (width * height);
                probabilityB[i] /= (width * height);
            }
            int p = 0;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    double sumR = 0;
                    double sumG = 0;
                    double sumB = 0;

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
                    newBmp[p++] = (byte)(Math.Floor(255 * sumB));
                    newBmp[p++] = (byte)(Math.Floor(255 * sumG));
                    newBmp[p++] = (byte)(Math.Floor(255 * sumR));
                }
            }
            Marshal.Copy(newBmp, 0, ptr, bytes);
            image.UnlockBits(bmpData);

            ih.saveImage(image, savePath);
        }
        public static Bitmap ExtendBitmapByOne(Bitmap image)
        {
            Bitmap res = new Bitmap(image.Width + 2, image.Height + 2);

            for (int i = 1; i <= image.Height; i++)
            {
                for (int j = 1; j <= image.Width; j++)
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

            for (int i = 1; i < buffer.Height - 1; i++)
            {
                for (int j = 1; j < buffer.Width - 1; j++)
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

                    for (int x = 1; x < maskR.Length; x++)
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

        public static int MeanSquareErrorAsync(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);

            int height = bmp1.Height;
            int width = bmp1.Width;

            BitmapData bmpData1 = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmpData2 = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] pixels1 = new byte[height * bmpData1.Stride];
            byte[] pixels2 = new byte[height * bmpData1.Stride];

            Marshal.Copy(bmpData1.Scan0, pixels1, 0, height * bmpData1.Stride);
            Marshal.Copy(bmpData2.Scan0, pixels2, 0, height * bmpData2.Stride);

            int mse;

            Task<double> taskR = Task<double>.Factory.StartNew(() =>
            {
                double sumOfSquaresR = 0;
                for (int x = 0; x < height * bmpData1.Stride - 2; x += 3)
                {
                    byte pixel1R = pixels1[x];
                    byte pixel2R = pixels2[x];

                    sumOfSquaresR += Math.Pow(pixel1R - pixel2R, 2);
                }
                return sumOfSquaresR;
            });

            Task<double> taskG = Task<double>.Factory.StartNew(() =>
            {
                double sumOfSquaresG = 0;
                for (int x = 0; x < height * bmpData1.Stride - 2; x += 3)
                {
                    byte pixel1G = pixels1[x + 1];
                    byte pixel2G = pixels2[x + 1];

                    sumOfSquaresG += Math.Pow(pixel1G - pixel2G, 2);
                }
                return sumOfSquaresG;
            });

            Task<double> taskB = Task<double>.Factory.StartNew(() =>
            {
                double sumOfSquaresB = 0;
                for (int x = 0; x < height * bmpData1.Stride - 2; x += 3)
                {
                    byte pixel1B = pixels1[x + 2];
                    byte pixel2B = pixels2[x + 2];
                    sumOfSquaresB += Math.Pow(pixel1B - pixel2B, 2);
                }
                return sumOfSquaresB;
            });

            mse = (int)((taskR.Result + taskG.Result + taskB.Result) / (3 * height * width));
            return mse;
        }
        public static int MeanSquareError(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);

            int height = bmp1.Height;
            int width = bmp1.Width;

            BitmapData bmpData1 = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmpData2 = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] pixels1 = new byte[height * bmpData1.Stride];
            byte[] pixels2 = new byte[height * bmpData1.Stride];

            Marshal.Copy(bmpData1.Scan0, pixels1, 0, height * bmpData1.Stride);
            Marshal.Copy(bmpData2.Scan0, pixels2, 0, height * bmpData2.Stride);

            double sumOfSquaresR = 0;
            double sumOfSquaresG = 0;
            double sumOfSquaresB = 0;
            int mse;

            for (int x = 0; x < bmpData1.Height * bmpData1.Stride - 2; x += 3)
            {
                byte pixel1R = pixels1[x];
                byte pixel1G = pixels1[x + 1];
                byte pixel1B = pixels1[x + 2];
                byte pixel2R = pixels2[x];
                byte pixel2G = pixels2[x + 1];
                byte pixel2B = pixels2[x + 2];

                sumOfSquaresR += Math.Pow(pixel1R - pixel2R, 2);
                sumOfSquaresG += Math.Pow(pixel1G - pixel2G, 2);
                sumOfSquaresB += Math.Pow(pixel1B - pixel2B, 2);
            }

            mse = (int)((sumOfSquaresR + sumOfSquaresG + sumOfSquaresB) / (3 * height * width));
            return mse;
        }
        public static double PeakMeanSquareError(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);

            int height = bmp1.Height;
            int width = bmp1.Width;

            BitmapData bmpData1 = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmpData2 = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] pixels1 = new byte[height * bmpData1.Stride];
            byte[] pixels2 = new byte[height * bmpData1.Stride];

            Marshal.Copy(bmpData1.Scan0, pixels1, 0, height * bmpData1.Stride);
            Marshal.Copy(bmpData2.Scan0, pixels2, 0, height * bmpData2.Stride);

            double sumOfSquaresR = 0;
            double sumOfSquaresG = 0;
            double sumOfSquaresB = 0;
            double maxR = 0;
            double maxG = 0;
            double maxB = 0;

            double pmse;

            for (int x = 0; x < height * bmpData1.Stride - 2; x += 3)
            {

                byte pixel1R = pixels1[x];
                byte pixel1G = pixels1[x + 1];
                byte pixel1B = pixels1[x + 2];
                byte pixel2R = pixels2[x];
                byte pixel2G = pixels2[x + 1];
                byte pixel2B = pixels2[x + 2];

                if (pixel1R > maxR)
                    maxR = pixel1R;
                if (pixel1G > maxG)
                    maxG = pixel1G;
                if (pixel1B > maxB)
                    maxB = pixel1B;

                sumOfSquaresR += Math.Pow(pixel1R - pixel2R, 2);
                sumOfSquaresG += Math.Pow(pixel1G - pixel2G, 2);
                sumOfSquaresB += Math.Pow(pixel1B - pixel2B, 2);
            }

            pmse = (sumOfSquaresR + sumOfSquaresG + sumOfSquaresB) / (3 * (height * width) * Math.Pow((maxR + maxG + maxB) / 3, 2));

            return pmse;
        }

        public static int MaximumDifference(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);
            int height = bmp1.Height;
            int width = bmp1.Width;

            BitmapData bmpData1 = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmpData2 = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] pixels1 = new byte[height * bmpData1.Stride];
            byte[] pixels2 = new byte[height * bmpData1.Stride];

            Marshal.Copy(bmpData1.Scan0, pixels1, 0, height * bmpData1.Stride);
            Marshal.Copy(bmpData2.Scan0, pixels2, 0, height * bmpData2.Stride);

            double maxDiff = 0;

            for (int x = 0; x < height * bmpData1.Stride - 2; x += 3)
            {

                byte pixel1R = pixels1[x];
                byte pixel1G = pixels1[x + 1];
                byte pixel1B = pixels1[x + 2];
                byte pixel2R = pixels2[x];
                byte pixel2G = pixels2[x + 1];
                byte pixel2B = pixels2[x + 2];

                int redDiff = Math.Abs(pixel1R - pixel2R);
                int greenDiff = Math.Abs(pixel1G - pixel2G);
                int blueDiff = Math.Abs(pixel1B - pixel2B);

                double sumDiff = (redDiff + greenDiff + blueDiff) / 3;

                if (sumDiff > maxDiff)
                    maxDiff = sumDiff;
            }

            return (int)maxDiff;
        }

        public static double SignalToNoiseRatio(string firstImage, string secondImage)
        {
            Bitmap bmp1 = new Bitmap(firstImage);
            Bitmap bmp2 = new Bitmap(secondImage);
            int height = bmp1.Height;
            int width = bmp1.Width;

            BitmapData bmpData1 = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmpData2 = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] pixels1 = new byte[height * bmpData1.Stride];
            byte[] pixels2 = new byte[height * bmpData1.Stride];

            Marshal.Copy(bmpData1.Scan0, pixels1, 0, height * bmpData1.Stride);
            Marshal.Copy(bmpData2.Scan0, pixels2, 0, height * bmpData2.Stride);

            double sumOfSquaresR = 0;
            double sumOfSquaresG = 0;
            double sumOfSquaresB = 0;

            double sumOfSquarePixelR = 0;
            double sumOfSquarePixelG = 0;
            double sumOfSquarePixelB = 0;

            double snr;

            for (int x = 0; x < height * bmpData1.Stride - 2; x += 3)
            {
                byte pixel1R = pixels1[x];
                byte pixel1G = pixels1[x + 1];
                byte pixel1B = pixels1[x + 2];
                byte pixel2R = pixels2[x];
                byte pixel2G = pixels2[x + 1];
                byte pixel2B = pixels2[x + 2];

                sumOfSquaresR += Math.Pow(pixel1R - pixel2R, 2);
                sumOfSquaresG += Math.Pow(pixel1G - pixel2G, 2);
                sumOfSquaresB += Math.Pow(pixel1B - pixel2B, 2);

                sumOfSquarePixelR += Math.Pow(pixel1R, 2);
                sumOfSquarePixelG += Math.Pow(pixel1G, 2);
                sumOfSquarePixelB += Math.Pow(pixel1B, 2);
            }
            snr = 10 * Math.Log10((sumOfSquarePixelR + sumOfSquarePixelG + sumOfSquarePixelB) / (sumOfSquaresR + sumOfSquaresG + sumOfSquaresB));

            return snr;
        }

        public static double PeakSignalToNoiseRatio(string firstImage, string secondImage)
        {

            return 20 * Math.Log10(255 + 255 + 255) - 10 * Math.Log10(MeanSquareError(firstImage, secondImage));
        }
    }
}
