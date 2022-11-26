using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Processor.CommandLineOptions;

namespace Processor
{
    public static class ImageProcessor
    {
        private static ImageHandler ih = new ImageHandler();
        public static ImageHandler Ih { get { return ih; } }
        /// <summary>
        /// Method that processes the arguments passed by the user through command line, and makes calls to proper functions, based un user's input.
        /// </summary>
        /// <param name="o"></param>
        public static void Process(CommandLineOptions o)
        {
            Bitmap bmp;
            if (o.firstPath == null || o.secondPath == null)
                return;
            else
                bmp = ih.loadImage(o.firstPath);
            if (o.ValuesA.Count() > 0)
            {
                List<int> values = new List<int>(o.ValuesA);
                AlphaTrimmedFilter(ih.Bmp, o.secondPath, values[0], values[1], values[2]);
            }
            if (o.ValuesG.Count() > 0)
            {
                List<int> values = new List<int>(o.ValuesG);
                GeometricMeanFilter(ih.Bmp, o.secondPath, values[0], values[1]);
            }
            if (o.brightness != 0)
                ChangeBrightness(ih.Bmp, o.secondPath, o.brightness);
            if (o.contrast !=0)
                ChangeContrast(ih.Bmp, o.secondPath,o.contrast);
            if (o.negative)
                NegativeImage(ih.Bmp, o.secondPath);
            if (o.Dimensions.Count() > 0)
            {
                //Some function calls take multiple parameters from the user,
                //so we put them into IEnumerable and separate them here.
                List<int> dimensions = new List<int>(o.Dimensions);
                BilinearResizing(ih.Bmp, o.secondPath, dimensions[0], dimensions[1]);
            }
            if (o.DimensionsS.Count() > 0)
            {
                //Some function calls take multiple parameters from the user,
                //so we put them into IEnumerable and separate them here.
                List<int> dimensions = new List<int>(o.DimensionsS);
                BilinearResizing(ih.Bmp, o.secondPath, dimensions[0], dimensions[1]);
            }
            if (o.hflip)
                HorizontalFlip(ih.Bmp, o.secondPath);
            if (o.vflip)
                VerticalFlip(ih.Bmp, o.secondPath);
            if (o.dflip)
                DiagonalFlip(ih.Bmp, o.secondPath);
            if (o.meanSquare)
                Console.WriteLine(MeanSquareErrorAsync(o.firstPath, o.secondPath));
            if (o.peakMeanSquare)
                Console.WriteLine(PeakMeanSquareError(o.firstPath, o.secondPath));
            if (o.maximumDifference)
                Console.WriteLine(MaximumDifference(o.firstPath, o.secondPath));
            if (o.signalToNoiseRatio)
                Console.WriteLine(SignalToNoiseRatio(o.firstPath, o.secondPath));
            if (o.peakSignalToNoiseRatio)
                Console.WriteLine(PeakSignalToNoiseRatio(o.firstPath, o.secondPath));
            if (o.channel == Channel.Red || o.channel == Channel.Blue || o.channel == Channel.Green)
                HistogramImage(ih.Bmp, o.secondPath, o.channel);
            if(o.gs.Any())
            {
                List<double> gs = new List<double>(o.gs);
                PowerFinalProbabilityDensityFunction(ih.Bmp, o.secondPath, gs[0], gs[1]);
            }
            if (o.mean)
                Console.WriteLine(Mean(ih.Bmp));
            if (o.variance)
                Console.WriteLine(Variance(ih.Bmp));
            if (o.deviation)
                Console.WriteLine(StandardDeviation(ih.Bmp));
            if (o.variation)
                Console.WriteLine(VariationCoefficientI(ih.Bmp));
            if (o.asymmetry)
                Console.WriteLine(AsymmetryCoefficient(ih.Bmp));
            if (o.flattening)
                Console.WriteLine(FlatteningCoefficient(ih.Bmp));
            if (o.variation2)
                Console.WriteLine(VariationCoefficientII(ih.Bmp));
            if (o.entropy)
                Console.WriteLine(InformationSourceEntropy(ih.Bmp));
            if (o.sexdeti)
                ExtractionOfDetailsI(ih.Bmp, o.secondPath);
            if (o.robertsII)
                RobertsII(ih.Bmp, o.secondPath);
            if (o.sexdetio)
                ExtractionOfDetailsIOptimized(ih.Bmp, o.secondPath);
                
        }
        /// <summary>
        /// Method used solely as a helper method in ChangeBrightnessMethod().
        /// It assures that the pixel values after changing the brightness don't go below zero, or above 255.
        /// </summary>
        /// <param name="pixelValue1"></param>
        /// <param name="changeValue"></param>
        /// <returns>Byte value between 0 and 255.</returns>
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
        /// <summary>
        /// Assures that the value is at the interval <0,255>. If value > 255 it returns 255, if values < 0 it returns 0.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Byte value between 0 and 255.</returns>
        private static byte Clamp(int value)
        {
            if (value > 255)
                return 255;
            else if (value < 0)
                return 0;
            else
                return (byte)value;
        }
        /// <summary>
        /// Method used to increase or decrease brightness of an image.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
        /// <param name="changeValue"></param>
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

        /// <summary>
        /// Method that produces version of an image, with negative of the original color values.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
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
        /// <summary>
        /// Resizes the image to dimensions given by the user.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        public static void BilinearResizing(Bitmap image, string savePath, int nWidth, int nHeight)
        {
            double nXFactor = (double)image.Width / (double)nWidth;
            double nYFactor = (double)image.Height / (double)nHeight;
            double fraction_x, fraction_y, one_minus_x, one_minus_y;
            int ceil_x, ceil_y, floor_x, floor_y;
            byte red, green, blue, b1, b2;
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
                        ceil_x--;

                    ceil_y = floor_y + 1;

                    if (ceil_y >= image.Height)
                        ceil_y--;

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
        /// <summary>
        /// Flips the image horizontally.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
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
        /// <summary>
        /// Flips the image vertically.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
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
        /// <summary>
        /// Flips the image diagonally.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
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
        /// <summary>
        /// Changes the contrast of an image to a higher value.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
        /// <param name="contrast"></param>
        public static void ChangeContrast(Bitmap image, string savePath, int contrast)
        {
            int width = image.Width;
            int height = image.Height;
            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] originalRGB = new byte[bytes];
            byte[] newBmp = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(bmpData.Scan0, originalRGB, 0, image.Height * bmpData.Stride);

            float factor = (259 * ((float)contrast + 255)) / (255 * (259 - (float)contrast));

            for (int x = 0; x < bytes - 2; x += 3)
            {
                byte red = originalRGB[x + 2];
                byte green = originalRGB[x + 1];
                byte blue = originalRGB[x];

                byte newRed = Clamp((int)(factor * (red - 128)) + 128);
                byte newGreen = Clamp((int)(factor * (green - 128)) + 128);
                byte newBlue = Clamp((int)(factor * (blue - 128)) + 128);

                newBmp[x + 2] = newRed;
                newBmp[x + 1] = newGreen;
                newBmp[x] = newBlue;
            }
            Marshal.Copy(newBmp, 0, bmpData.Scan0, bytes);
            image.UnlockBits(bmpData);

            ih.saveImage(image, savePath);
        }
        /// <summary>
        /// Extends the given bitmap by one pixel on its edges.
        /// </summary>
        /// <param name="image"></param>
        /// <returns>New bitmap with dimension increased by 2</returns>
        public static Bitmap ExtendBitmapByOne(Bitmap image)
        {
            Bitmap res = new Bitmap(image.Width + 2, image.Height + 2);

            //Copy row zero of original image, to row zero of buffer image, last row to last row, etc.
            for(int i = 1; i <= image.Height; i++)
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
            //Assign corner values by hand.
            res.SetPixel(0, 0, res.GetPixel(1, 0));
            res.SetPixel(res.Width - 1, 0, res.GetPixel(res.Width - 2, 0));
            res.SetPixel(0, res.Height - 1, res.GetPixel(0, res.Height - 2));
            res.SetPixel(res.Width - 1, res.Height - 1, res.GetPixel(res.Height - 2, res.Width - 1));
            return res;
        }
        /// <summary>
        /// Applies alpha-trimmed mean filter on an image to denoise it.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
        /// <param name="alpha"></param>
        /// <param name="maskM"></param>
        /// <param name="maskN"></param>
        public static void AlphaTrimmedFilter(Bitmap image, string savePath, int alpha, int maskM, int maskN)
        {
            int m = maskM;
            int n = maskN;

            //We want our mask dimensions to be odd, so we can avoid situations where we can't take
            //equally as many pixels from around the center pxiel.
            if (m % 2 == 0) m++;
            if (n % 2 == 0) n++;

            if(alpha > (m * n) / 2)
            {
                Console.WriteLine("Alpha cannot be bigger than m*n/2");
                return;
            }

            int radiusN = (int)Math.Floor(maskN / 2.0);
            int radiusM = (int)Math.Floor(maskM / 2.0);

            int height = ih.Bmp.Height;
            int width = ih.Bmp.Width;

            Bitmap res = new Bitmap(image.Width, image.Height);
            Bitmap buffer = ExtendBitmapByOne(image);

            //Run through every pixel of the original image(not buffer)
            for(int i = 1; i < buffer.Height - 1; i++)
            {
                for (int j = 1; j < buffer.Width - 1; j++)
                {
                    //Put a 3x3 mask on every pixel of the image(including borders of the buffer.)
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

                    //Order the mask, so we can trim the border values
                    Array.Sort(maskR, (x, y) => x.R.CompareTo(y.R));
                    Array.Sort(maskG, (x, y) => x.G.CompareTo(y.G));
                    Array.Sort(maskB, (x, y) => x.B.CompareTo(y.B));

                    //Put sorted arrays in lists.
                    List<Color> colorsR = new List<Color>(maskR);
                    List<Color> colorsG = new List<Color>(maskG);
                    List<Color> colorsB = new List<Color>(maskB);

                    //Remove alpha elements from both sides of the sorted arrayList
                    for (int l = 0; l < alpha; l++)
                    {
                        colorsR.RemoveAt(0);
                        colorsR.RemoveAt(colorsR.Count - 1);

                        colorsG.RemoveAt(0);
                        colorsG.RemoveAt(colorsG.Count - 1);

                        colorsB.RemoveAt(0);
                        colorsB.RemoveAt(colorsB.Count - 1);
                    }

                    //Calculate the mean value of the mask
                    meanR = colorsR.Sum(x => x.R) / colorsR.Count;
                    meanG = colorsG.Sum(x => x.G) / colorsG.Count;
                    meanB = colorsB.Sum(x => x.B) / colorsB.Count;

                    //Assign the mean value to the target pixel
                    res.SetPixel(j - 1, i - 1, Color.FromArgb(meanR, meanG, meanB));
                }
            }
            ih.saveImage(res, savePath);
        }
        /// <summary>
        /// Applies geometric mean filter on an image to denoise it.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        public static void GeometricMeanFilter(Bitmap image, string savePath, int m, int n)
        {
            int maskM = m;
            int maskN = n;

            //We want our mask dimensions to be odd, so we can avoid situations where we can't take
            //equally as many pixels from around the center pxiel.
            if (m % 2 == 0) m++;
            if (n % 2 == 0) n++;

            int radiusN = (int)Math.Floor(maskN / 2.0);
            int radiusM = (int)Math.Floor(maskM / 2.0);

            Bitmap res = new Bitmap(image.Width, image.Height);
            Bitmap buffer = ExtendBitmapByOne(image);

            //Run through every pixel of the original image(not buffer)
            for (int i = 1; i < buffer.Height - 1; i++)
            {
                for (int j = 1; j < buffer.Width - 1; j++)
                {
                    //Put a 3x3 mask on every pixel of the image(including buffer, as we need the borders)
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

                    //Assign non-zero value to product of arrays.
                    double productR = maskR[0].R;
                    double productG = maskG[0].G;
                    double productB = maskB[0].B;

                    //Multiply every element of every array. We start at x=1, because we've previously assigned the first value of the array to product.
                    for(int x = 1; x < maskR.Length; x++)
                    {
                        productR *= maskR[x].R;
                        productG *= maskG[x].G;
                        productB *= maskB[x].B;
                    }

                    double divider = 1.0 / (m * n);

                    //Assign the geometric product value to the target pixel
                    res.SetPixel(j - 1, i - 1, Color.FromArgb((int)Math.Pow(productR, divider), (int)Math.Pow(productG, divider), (int)Math.Pow(productB, divider)));
                }
            }
            ih.saveImage(res, savePath);
        }
        /// <summary>
        /// Calculates the mean square error between every pixel of two given images.
        /// </summary>
        /// <param name="firstImage"></param>
        /// <param name="secondImage"></param>
        /// <returns>An int that indicates the level of similarity between images.</returns>
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

            //Copy bmpData1 and bmpData2 to pixels1 and pixels2 arrays respecitvely.
            Marshal.Copy(bmpData1.Scan0, pixels1, 0, height * bmpData1.Stride);
            Marshal.Copy(bmpData2.Scan0, pixels2, 0, height * bmpData2.Stride);

            int mse;

            //As the red, green and blue values are independent of each other, we can calculate their sum of squares, at the same time.
            Task<double> taskR= Task<double>.Factory.StartNew(() =>
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
        /// <summary>
        /// Calculates the peak mean square error between every pixel of two given images.
        /// </summary>
        /// <param name="firstImage"></param>
        /// <param name="secondImage"></param>
        /// <returns>An int that indicates the level of similarity between images.</returns>
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

            //Copy bmpData1 and bmpData2 to pixels1 and pixels2 arrays respecitvely.
            Marshal.Copy(bmpData1.Scan0, pixels1, 0, height * bmpData1.Stride);
            Marshal.Copy(bmpData2.Scan0, pixels2, 0, height * bmpData2.Stride);

            double sumOfSquaresR = 0;
            double sumOfSquaresG = 0;
            double sumOfSquaresB = 0;
            double maxR = 0;
            double maxG = 0;
            double maxB = 0;

            double pmse;

            //Run through every value of pixels 1 and 2 arrays.
            for(int x = 0; x < height * bmpData1.Stride - 2; x += 3)
            {

                byte pixel1R = pixels1[x];
                byte pixel1G = pixels1[x + 1];
                byte pixel1B = pixels1[x + 2];
                byte pixel2R = pixels2[x];
                byte pixel2G = pixels2[x + 1];
                byte pixel2B = pixels2[x + 2];

                //Fin the overall max value for each of rgb channels.
                if (pixel1R > maxR)
                    maxR = pixel1R;
                if (pixel1G > maxG)
                    maxG = pixel1G;
                if (pixel1B > maxB)
                    maxB = pixel1B;

                //Calculate the sum of squares of respective rgb values from two images.
                sumOfSquaresR += Math.Pow(pixel1R - pixel2R, 2);
                sumOfSquaresG += Math.Pow(pixel1G - pixel2G, 2);
                sumOfSquaresB += Math.Pow(pixel1B - pixel2B, 2);
            }

            pmse = (sumOfSquaresR + sumOfSquaresG + sumOfSquaresB) / (3 * (height * width) * Math.Pow((maxR + maxG + maxB) / 3, 2));

            return pmse;
        }
        /// <summary>
        /// Calculates the maximum difference between two given images.
        /// </summary>
        /// <param name="firstImage"></param>
        /// <param name="secondImage"></param>
        /// <returns>An int that indicates the level of similarity between images.</returns>
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

            //Copy bmpData1 and bmpData2 to pixels1 and pixels2 arrays respecitvely.
            Marshal.Copy(bmpData1.Scan0, pixels1, 0, height * bmpData1.Stride);
            Marshal.Copy(bmpData2.Scan0, pixels2, 0, height * bmpData2.Stride);

            double maxDiff = 0;

            //Run through every pixel of pixels1 and pixels2 arrays.
            for (int x = 0; x < height * bmpData1.Stride - 2; x += 3)
            {

                byte pixel1R = pixels1[x];
                byte pixel1G = pixels1[x + 1];
                byte pixel1B = pixels1[x + 2];
                byte pixel2R = pixels2[x];
                byte pixel2G = pixels2[x + 1];
                byte pixel2B = pixels2[x + 2];

                //Calculate the difference between two respective rgb values in two images.
                int redDiff = Math.Abs(pixel1R - pixel2R);
                int greenDiff = Math.Abs(pixel1G - pixel2G);
                int blueDiff = Math.Abs(pixel1B - pixel2B);

                //Calculate the mean value of rgb differences.
                double sumDiff = (redDiff + greenDiff + blueDiff) / 3;

                //Find the overall biggest difference.
                if (sumDiff > maxDiff)
                    maxDiff = sumDiff;
            }

            return (int)maxDiff;
        }
        /// <summary>
        /// Calculates signal to noise ratio between two images.
        /// </summary>
        /// <param name="firstImage"></param>
        /// <param name="secondImage"></param>
        /// <returns>An int that indicates the level of similarity between images.</returns>
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

            //Copy bmpData1 and bmpData2 to pixels1 and pixels2 arrays respecitvely.
            Marshal.Copy(bmpData1.Scan0, pixels1, 0, height * bmpData1.Stride);
            Marshal.Copy(bmpData2.Scan0, pixels2, 0, height * bmpData2.Stride);

            double sumOfSquaresR = 0;
            double sumOfSquaresG = 0;
            double sumOfSquaresB = 0;

            double sumOfSquarePixelR = 0;
            double sumOfSquarePixelG = 0;
            double sumOfSquarePixelB = 0;

            double snr;

            //Run through every value of pixels1 and pixels2 arrays.
            for (int x = 0; x < height * bmpData1.Stride - 2; x += 3)
            {
                byte pixel1R = pixels1[x];
                byte pixel1G = pixels1[x + 1];
                byte pixel1B = pixels1[x + 2];
                byte pixel2R = pixels2[x];
                byte pixel2G = pixels2[x + 1];
                byte pixel2B = pixels2[x + 2];

                //Calculate sum of squares of differences of respective pixels in two images.
                sumOfSquaresR += Math.Pow(pixel1R - pixel2R, 2);
                sumOfSquaresG += Math.Pow(pixel1G - pixel2G, 2);
                sumOfSquaresB += Math.Pow(pixel1B - pixel2B, 2);

                //Calculate the sum of squares of rgb values in the first image.
                sumOfSquarePixelR += Math.Pow(pixel1R, 2);
                sumOfSquarePixelG += Math.Pow(pixel1G, 2);
                sumOfSquarePixelB += Math.Pow(pixel1B, 2);
            }
            snr = 10 * Math.Log10((sumOfSquarePixelR + sumOfSquarePixelG + sumOfSquarePixelB) / (sumOfSquaresR + sumOfSquaresG + sumOfSquaresB));

            return snr;
        }
        /// <summary>
        /// Calculates peak signal to noise ratio between two images.
        /// </summary>
        /// <param name="firstImage"></param>
        /// <param name="secondImage"></param>
        /// <returns>An int that indicates the level of similarity between images.</returns>
        public static double PeakSignalToNoiseRatio(string firstImage, string secondImage)
        {

            return 20 * Math.Log10(255 + 255 + 255) - 10 * Math.Log10(MeanSquareError(firstImage, secondImage));
        }
        /// <summary>
        /// Generates a histogram of an image, of the specified channel ('Red', 'Green', 'Blue')
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
        /// <param name="channel"></param>
        public static void HistogramImage(Bitmap image, string savePath, Channel channel)
        {
            int chosenChannel = 0;

            if(channel == Channel.Red)
                chosenChannel = 2;
            else if(channel == Channel.Green)
                chosenChannel = 1;
            else if(channel == Channel.Blue)
                chosenChannel = 0;


            int width = image.Width;
            int height = image.Height; 

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //Size of bitmapdata
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            //Copy the bitmapdata to rgbValues array
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, height * bmpData.Stride);

            image.UnlockBits(bmpData);

            byte[] chosenChannelValues = new byte[width * height];
            double[] histogram = new double[256];

            //Separate the desired rgb channel
            int k = 0;
            for (int i = 0; i < bytes; i+=3)
            {
                chosenChannelValues[k++] = rgbValues[i + chosenChannel];
            }

            //Create a histogram for the desired channel
            for(int i = 0; i < height * width; i++)
            {
                histogram[chosenChannelValues[i]]++;
            }

            //Scale down the image, if needed
            while (histogram.Max() > 512)
            {
                for (int i = 0; i < 256; i++)
                {
                    Math.Floor(histogram[i] /= 10);
                }
            }

            //Create a bew bitmap, to save the histogram to
            Bitmap output = new Bitmap(512, 512);
            BitmapData bmpData2 = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytes2 = bmpData2.Stride * bmpData2.Height;
            byte[] histogramValues = new byte[bytes2];

            Marshal.Copy(bmpData2.Scan0, histogramValues, 0, bytes2);

            //Set the bitmap to be white
            for(int i = 0; i < bytes2; i++)
            {
                histogramValues[i] = 255;
            }

            //Draw the histogram
            int d = output.Height * bmpData2.Stride - bmpData2.Stride;
            int p = 0;
            for(int i = d; i < bytes2; i+=6)
            {
                for(int j = 0; j < histogram[p]; j++)
                {
                    histogramValues[i - j * bmpData2.Stride] = 0;
                    histogramValues[i - j * bmpData2.Stride + 1] = 0;
                    histogramValues[i - j * bmpData2.Stride + 2] = 0;
                    histogramValues[i - j * bmpData2.Stride + 3] = 0;
                    histogramValues[i - j * bmpData2.Stride + 4] = 0;
                    histogramValues[i - j * bmpData2.Stride + 5] = 0;
                }
                p++;
            }

            Marshal.Copy(histogramValues, 0, bmpData2.Scan0, bytes2);
            output.UnlockBits(bmpData2);

            ih.saveImage(output, savePath);
        }
        /// <summary>
        /// Output the improved version of the image, using power 2/3 probability density function.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
        /// <param name="gMin"></param>
        /// <param name="gMax"></param>
        private static void PowerFinalProbabilityDensityFunction(Bitmap image, string savePath, double gMin, double gMax)
        {
            if(gMin < 0 || gMax > 255 || gMin > gMax)
            {
                Console.WriteLine("Wrong input parameters. Make sure that 0 <= gmin < gmax <= 255"); 
                return;
            }

            gMin = (int)gMin;
            gMax = (int)gMax;

            int width = image.Width;
            int height = image.Height;
            int numOfPixels = height * width;

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //Size of bitmapdata
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            //Copy the bitmapdata to rgbValues array
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, height * bmpData.Stride);

            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            double[] histogramR = new double[256];
            double[] histogramG = new double[256];
            double[] histogramB = new double[256];

            //Split the image into 3 rgb channels
            int k = 0;
            for (int i = 0; i < bytes; i += 3)
            {
                b[k] = rgbValues[i];
                g[k] = rgbValues[i + 1];
                r[k++] = rgbValues[i + 2];
            }

            //Create histograms for all the channels
            for (int i = 0; i < height * width; i++)
            {
                histogramR[r[i]]++;
                histogramG[g[i]]++;
                histogramB[b[i]]++;
            }

            double divider = 1.0 / 3.0;
            int p = 0;
            for(int i = 0; i < bytes; i+=3)
            {
                double histogramRSum = 0;
                double histogramGSum = 0;
                double histogramBSum = 0;
                for (int j = 0; j < r[p]; j++)
                {
                    histogramRSum += histogramR[j];
                }
                for (int j = 0; j < g[p]; j++)
                {
                    histogramGSum += histogramG[j];
                }
                for (int j = 0; j < b[p]; j++)
                {
                    histogramBSum += histogramB[j];
                }
                rgbValues[i] = (byte)Math.Pow(Math.Pow(gMin, divider) + (Math.Pow(gMax, divider) - Math.Pow(gMin, divider)) * (1.0 / numOfPixels) * histogramBSum, 3);
                rgbValues[i + 1] = (byte)Math.Pow(Math.Pow(gMin, divider) + (Math.Pow(gMax, divider) - Math.Pow(gMin, divider)) * (1.0 / numOfPixels) * histogramGSum, 3);
                rgbValues[i + 2] = (byte)Math.Pow(Math.Pow(gMin, divider) + (Math.Pow(gMax, divider) - Math.Pow(gMin, divider)) * (1.0 / numOfPixels) * histogramRSum, 3);
                p++;
            }

            Marshal.Copy(rgbValues, 0, bmpData.Scan0, bytes);
            image.UnlockBits(bmpData);

            ih.saveImage(image, savePath);
        }
        /// <summary>
        /// Calculates the mean characteristic, of a given image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static double Mean(Bitmap image)
        {
            double mean;

            int width = image.Width;
            int height = image.Height;
            int numOfPixels = height * width;

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //Size of bitmapdata
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            //Copy the bitmapdata to rgbValues array
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, height * bmpData.Stride);

            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            double[] histogramR = new double[256];
            double[] histogramG = new double[256];
            double[] histogramB = new double[256];

            //Split the image into 3 rgb channels
            int k = 0;
            for (int i = 0; i < bytes; i += 3)
            {
                b[k] = rgbValues[i];
                g[k] = rgbValues[i + 1];
                r[k++] = rgbValues[i + 2];
            }

            image.UnlockBits(bmpData);

            //Create histograms for all the channels
            for (int i = 0; i < height * width; i++)
            {
                histogramR[r[i]]++;
                histogramG[g[i]]++;
                histogramB[b[i]]++;
            }

            //Sum the histograms.
            double histogramRSum = 0;
            double histogramGSum = 0;
            double histogramBSum = 0;
            for (int i = 0; i < 256; i++)
            {
                histogramRSum +=  i * histogramR[i];
                histogramGSum += i * histogramG[i];
                histogramBSum += i * histogramB[i];
            }

            double histogramSum = (histogramRSum + histogramBSum + histogramGSum) / 3.0;

            mean = (1.0 / numOfPixels) * histogramSum;

            return mean;
        }
        /// <summary>
        /// Calculates the variance characteristic of a given image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static double Variance(Bitmap image)
        {
            double variance;
            double mean = Mean(image);

            int width = image.Width;
            int height = image.Height;
            int numOfPixels = height * width;

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //Size of bitmapdata
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            //Copy the bitmapdata to rgbValues array
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, height * bmpData.Stride);

            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            double[] histogramR = new double[256];
            double[] histogramG = new double[256];
            double[] histogramB = new double[256];

            //Split the image into 3 rgb channels
            int k = 0;
            for (int i = 0; i < bytes; i += 3)
            {
                b[k] = rgbValues[i];
                g[k] = rgbValues[i + 1];
                r[k++] = rgbValues[i + 2];
            }

            image.UnlockBits(bmpData);

            //Create histograms for all the channels
            for (int i = 0; i < height * width; i++)
            {
                histogramR[r[i]]++;
                histogramG[g[i]]++;
                histogramB[b[i]]++;
            }

            //Sum the histograms.
            double histogramRSum = 0;
            double histogramGSum = 0;
            double histogramBSum = 0;
            for (int i = 0; i < 256; i++)
            {
                histogramRSum += Math.Pow(i - mean, 2) * histogramR[i];
                histogramGSum += Math.Pow(i - mean, 2) * histogramG[i];
                histogramBSum += Math.Pow(i - mean, 2) * histogramB[i];
            }

            double histogramSum = (histogramRSum + histogramBSum + histogramGSum) / 3.0;

            variance = (1.0 / numOfPixels) * histogramSum;

            return variance;
        }
        /// <summary>
        /// Calculates the standard deviation characteristic of a given image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static double StandardDeviation(Bitmap image)
        {
            return Math.Sqrt(Variance(image));
        }
        /// <summary>
        /// Calculates the variation coefficient I characteristic of a given image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static double VariationCoefficientI(Bitmap image)
        {
            return StandardDeviation(image) / Mean(image);
        }
        /// <summary>
        /// Calculates the asymmetry coefficient characteristic of a given image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static double AsymmetryCoefficient(Bitmap image)
        {
            double asymmetry;

            double mean = Mean(image);
            double deviation = StandardDeviation(image);

            int width = image.Width;
            int height = image.Height;
            int numOfPixels = height * width;

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //Size of bitmapdata
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            //Copy the bitmapdata to rgbValues array
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, height * bmpData.Stride);

            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            double[] histogramR = new double[256];
            double[] histogramG = new double[256];
            double[] histogramB = new double[256];

            //Split the image into 3 rgb channels
            int k = 0;
            for (int i = 0; i < bytes; i += 3)
            {
                b[k] = rgbValues[i];
                g[k] = rgbValues[i + 1];
                r[k++] = rgbValues[i + 2];
            }

            image.UnlockBits(bmpData);

            //Create histograms for all the channels
            for (int i = 0; i < height * width; i++)
            {
                histogramR[r[i]]++;
                histogramG[g[i]]++;
                histogramB[b[i]]++;
            }

            //Sum the histograms.
            double histogramRSum = 0;
            double histogramGSum = 0;
            double histogramBSum = 0;
            for (int i = 0; i < 256; i++)
            {
                histogramRSum += Math.Pow(i - mean, 3.0) * histogramR[i];
                histogramGSum += Math.Pow(i - mean, 3.0) * histogramG[i];
                histogramBSum += Math.Pow(i - mean, 3.0) * histogramB[i];
            }

            double histogramSum = (histogramRSum + histogramBSum + histogramGSum) / 3.0;

            asymmetry = (1.0 / Math.Pow(deviation, 3)) * (1.0 / numOfPixels) * histogramSum;

            return asymmetry;
        }
        /// <summary>
        /// Calculates the flattening coefficient characteristic of a given image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static double FlatteningCoefficient(Bitmap image)
        {
            double flattening;

            double mean = Mean(image);
            double deviation = StandardDeviation(image);

            int width = image.Width;
            int height = image.Height;
            int numOfPixels = height * width;

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //Size of bitmapdata
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            //Copy the bitmapdata to rgbValues array
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, height * bmpData.Stride);

            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            double[] histogramR = new double[256];
            double[] histogramG = new double[256];
            double[] histogramB = new double[256];

            //Split the image into 3 rgb channels
            int k = 0;
            for (int i = 0; i < bytes; i += 3)
            {
                b[k] = rgbValues[i];
                g[k] = rgbValues[i + 1];
                r[k++] = rgbValues[i + 2];
            }

            image.UnlockBits(bmpData);

            //Create histograms for all the channels
            for (int i = 0; i < height * width; i++)
            {
                histogramR[r[i]]++;
                histogramG[g[i]]++;
                histogramB[b[i]]++;
            }

            //Sum the histograms.
            double histogramRSum = 0;
            double histogramGSum = 0;
            double histogramBSum = 0;
            for (int i = 0; i < 256; i++)
            {
                histogramRSum += Math.Pow(i - mean, 4.0) * histogramR[i] - 3.0;
                histogramGSum += Math.Pow(i - mean, 4.0) * histogramG[i] - 3.0;
                histogramBSum += Math.Pow(i - mean, 4.0) * histogramB[i] - 3.0;
            }

            double histogramSum = (histogramRSum + histogramBSum + histogramGSum) / 3.0;

            flattening = (1.0 / Math.Pow(deviation, 4)) * (1.0 / numOfPixels) * histogramSum;

            return flattening;
        }
        /// <summary>
        /// Calculates the variant coefficient II characteristic of a given image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static double VariationCoefficientII(Bitmap image)
        {
            double variation;

            int width = image.Width;
            int height = image.Height;
            int numOfPixels = height * width;

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //Size of bitmapdata
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            //Copy the bitmapdata to rgbValues array
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, height * bmpData.Stride);

            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            double[] histogramR = new double[256];
            double[] histogramG = new double[256];
            double[] histogramB = new double[256];

            //Split the image into 3 rgb channels
            int k = 0;
            for (int i = 0; i < bytes; i += 3)
            {
                b[k] = rgbValues[i];
                g[k] = rgbValues[i + 1];
                r[k++] = rgbValues[i + 2];
            }

            image.UnlockBits(bmpData);

            //Create histograms for all the channels
            for (int i = 0; i < height * width; i++)
            {
                histogramR[r[i]]++;
                histogramG[g[i]]++;
                histogramB[b[i]]++;
            }

            //Sum the histograms.
            double histogramRSum = 0;
            double histogramGSum = 0;
            double histogramBSum = 0;
            for (int i = 0; i < 256; i++)
            {
                histogramRSum += Math.Pow(histogramR[i], 2);
                histogramGSum += Math.Pow(histogramG[i], 2);
                histogramBSum += Math.Pow(histogramB[i], 2);
            }

            double histogramSum = (histogramRSum + histogramGSum + histogramBSum) / 3.0;

            variation = Math.Pow(1.0 / numOfPixels, 2) * histogramSum;

            return variation;
        }
        public static double InformationSourceEntropy(Bitmap image)
        {
            double entropy;

            int width = image.Width;
            int height = image.Height;
            int numOfPixels = height * width;

            BitmapData bmpData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //Size of bitmapdata
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            //Copy the bitmapdata to rgbValues array
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, height * bmpData.Stride);

            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            double[] histogramR = new double[256];
            double[] histogramG = new double[256];
            double[] histogramB = new double[256];

            //Split the image into 3 rgb channels
            int k = 0;
            for (int i = 0; i < bytes; i += 3)
            {
                b[k] = rgbValues[i];
                g[k] = rgbValues[i + 1];
                r[k++] = rgbValues[i + 2];
            }

            image.UnlockBits(bmpData);

            //Create histograms for all the channels
            for (int i = 0; i < height * width; i++)
            {
                histogramR[r[i]]++;
                histogramG[g[i]]++;
                histogramB[b[i]]++;
            }

            //Sum the histograms.
            double histogramRSum = 0;
            double histogramGSum = 0;
            double histogramBSum = 0;
            for (int i = 0; i < 256; i++)
            {
                if (histogramR[i] != 0)
                    histogramRSum += histogramR[i] * Math.Log(histogramR[i] / numOfPixels, 2);
                if (histogramG[i] != 0)
                    histogramGSum += histogramG[i] * Math.Log(histogramG[i] / numOfPixels, 2);
                if (histogramB[i] != 0)
                    histogramBSum += histogramB[i] * Math.Log(histogramB[i] / numOfPixels, 2);
            }

            double histogramSum = (histogramRSum + histogramGSum + histogramBSum) / 3.0;

            entropy = - (1.0 / numOfPixels) * histogramSum;

            return entropy;
        }
        /// <summary>
        /// Applys the extraction of details filter on an image.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
        public static void ExtractionOfDetailsI(Bitmap image, string savePath)
        {
            int height = image.Height;
            int width = image.Width;

            int[] hMask = new int[9] { 1, 1, 1, 1, -2, 1, -1, -1, -1 };

            Bitmap res = new Bitmap(image.Width, image.Height);

            //Run through every pixel of the original image.
            for (int i = 0; i < height; i++)
            {
                if (i == 0) continue;
                if (i == height - 1) break;
                for (int j = 0; j < width; j++)
                {
                    if (j == 0) continue;
                    if (j == width - 1) break;
                    //Put a 3x3 mask on every pixel of the image.
                    int k = 0;

                    Color[] mask = new Color[9];

                    for (int x = i - 1; x < i + 1 + 1; x++)
                    {
                        for (int y = j - 1; y < j + 1 + 1; y++)
                        {
                            Color value;
                            value = image.GetPixel(y, x);
                            mask[k] = value;
                            k++;
                        }
                    }

                    int newR = 0;
                    int newG = 0;
                    int newB = 0;

                    for (int z = 0; z < 9; z++)
                    {
                        newR += mask[z].R * hMask[z];
                        newG += mask[z].G * hMask[z];
                        newB += mask[z].B * hMask[z];
                    }

                    //Assign the new value to the target pixel
                    res.SetPixel(j, i, Color.FromArgb(Clamp(newR), Clamp(newG), Clamp(newB)));
                }
            }
            ih.saveImage(res, savePath);
        }
        public static void ExtractionOfDetailsIOptimized(Bitmap image, string savePath)
        {
            int height = image.Height;
            int width = image.Width;

            Bitmap res = new Bitmap(image.Width, image.Height);

            //Run through every pixel of the original image.
            for (int i = 0; i < height; i++)
            {
                if (i == 0) continue;
                if (i == height - 1) break;
                for (int j = 0; j < width; j++)
                {
                    if (j == 0) continue;
                    if (j == width - 1) break;

                    int newR = 0;

                    Color[] pixelValues = new Color[9];

                    pixelValues[0] = image.GetPixel(j - 1, i - 1);
                    pixelValues[1] = image.GetPixel(j, i - 1);
                    pixelValues[2] = image.GetPixel(j + 1, i - 1);
                    pixelValues[3] = image.GetPixel(j - 1, i);
                    pixelValues[4] = image.GetPixel(j, i);
                    pixelValues[5] = image.GetPixel(j + 1, i);
                    pixelValues[6] = image.GetPixel(j - 1, i + 1);
                    pixelValues[7] = image.GetPixel(j, i + 1);
                    pixelValues[8] = image.GetPixel(j + 1, i + 1);

                    newR = pixelValues[0].R * 1 + pixelValues[1].R * 1 + pixelValues[2].R * 1 + pixelValues[3].R * 1 + pixelValues[4].R * (-2) + pixelValues[5].R * 1 + pixelValues[6].R * (-1) + pixelValues[7].R * (-1) + pixelValues[8].R * (-1);

                    res.SetPixel(j, i, Color.FromArgb(Clamp(newR), Clamp(newR), Clamp(newR)));
                }
            }
            ih.saveImage(res, savePath);
        }
        /// <summary>
        /// Applys the roberts filter on an image.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="savePath"></param>
        public static void RobertsII(Bitmap image, string savePath)
        {
            int height = image.Height;
            int width = image.Width;

            Bitmap res = new Bitmap(image.Width, image.Height);

            byte newR;
            byte newG;
            byte newB;

            //Run through every pixel of the original image.
            for (int i = 0; i < height; i++)
            {
                if (i == 0) continue;
                if (i == height - 1) break;
                for (int j = 0; j < width; j++)
                {
                    if (j == 0) continue;
                    if (j == width - 1) break;
                    newR = (byte)(Math.Abs(image.GetPixel(j, i).R - image.GetPixel(j + 1, i + 1).R) + Math.Abs(image.GetPixel(j, i + 1).R - image.GetPixel(j + 1, i ).R));
                    newG = (byte)(Math.Abs(image.GetPixel(j, i).G - image.GetPixel(j + 1, i + 1).G) + Math.Abs(image.GetPixel(j, i + 1).G - image.GetPixel(j + 1, i).G));
                    newB = (byte)(Math.Abs(image.GetPixel(j, i).B - image.GetPixel(j + 1, i + 1).B) + Math.Abs(image.GetPixel(j, i + 1).B - image.GetPixel(j + 1, i).B));

                    res.SetPixel(j, i, Color.FromArgb(newR, newG, newB));
                }
            }
            ih.saveImage(res, savePath);
        }
    }
}
