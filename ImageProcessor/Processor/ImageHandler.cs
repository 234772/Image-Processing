using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processor
{
    public class ImageHandler : IImageHandler
    {
        private Bitmap bmp = null;
        public Bitmap Bmp { get { return bmp; } set { bmp = value; } }
        public  Bitmap loadImage(string path)
        {
            try 
            {
                bmp = new Bitmap(path);
            }
            catch(ArgumentException)
            {
                Console.WriteLine("Load file: Path \"" + path + "\" does not exist");
                return null;
            }
            return bmp;
        }

        public void saveImage(Bitmap image, string path)
        {
            try
            {
                bmp = image;
                image.Save(path);
            }
            catch(ArgumentException)
            {
                Console.WriteLine("Write file: \"" + path + "\" does not exist");
            }
        }
    }
}
