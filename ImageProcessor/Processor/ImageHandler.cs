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
            catch(ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
            return bmp;
        }

        public void saveImage(string path)
        {
            bmp.Save(path);
        }
    }
}
