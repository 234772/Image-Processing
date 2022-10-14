using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Processor
{
    public static class ImageProcessor
    {
        private static ImageHandler ih = new ImageHandler();
        public static ImageHandler Ih { get { return ih; } }

        public static int Truncate(int pixelValue)
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
            for(int y = 0; y < ih.Bmp.Height; y++)
            {
                for(int x = 0; x < ih.Bmp.Width; x++)
                {
                    Color color = ih.Bmp.GetPixel(y, x);
                    Color newColor = Color.FromArgb(Truncate(color.R + changeValue), Truncate(color.G + changeValue), Truncate(color.B + changeValue));
                    //Console.WriteLine(color.ToString());
                    ih.Bmp.SetPixel(y, x, newColor);
                }
            }
        }
        public static void NegativeImage()
        {
            for(int y = 0; y < ih.Bmp.Height; y++)
            {
                for(int x = 0; x < ih.Bmp.Width; x++)
                {
                    Color color = ih.Bmp.GetPixel(y, x);
                    Color newColor = Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
                    //Console.WriteLine(color.ToString());
                    ih.Bmp.SetPixel(y, x, newColor);
                }
            }
        }
 
    }
}
