using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Image_Processor
{
    public class Test
    {
        public static Bitmap image1;

        public static int Truncate(int a)
        {
            if (a > 255)
                return 255;
            else if (a < 0)
                return 0;
            else
                return a;
        }

        public static void Main()
        {
            image1 = new Bitmap(@"D:\Programming\Projects\Image-Processing\Image Processor\baboon.bmp", false);
            var image2 = new Bitmap(image1.Width, image1.Height);

            for(int i = 0; i < image1.Height; i++)
            {
                for (int j = 0; j < image1.Width; j++)
                {
                    Color color = image1.GetPixel(i, j);
                    Color newColor = Color.FromArgb(Truncate(color.R + 50), Truncate(color.G + 50), Truncate(color.B + 50));
                    image2.SetPixel(i, j, newColor);
                    //Console.WriteLine(image1.GetPixel(j, i) + " " + image1.GetPixel(j, i).GetBrightness());
                }
            }
            image2.Save("baboon2.bmp");
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
