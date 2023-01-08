using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processor
{
    internal class EdgeMask
    {
        private Bitmap mask1;
        private Bitmap mask2;

        public EdgeMask()
        {
            mask1 = new Bitmap("mask1.bmp");
            mask2 = new Bitmap("mask2.bmp");
        }

        public Bitmap GetMask(int numberOfMask)
        {
            switch(numberOfMask)
            {
                case 1:
                    return mask1;
                case 2: 
                    return mask2;
                default:
                    return null;
            }
        }
    }
}
