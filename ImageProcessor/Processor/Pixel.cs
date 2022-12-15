using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processor
{
    public class Pixel
    {
        private int x;
        private int y;

        public Pixel(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X { get { return x; } }
        public int Y { get { return y; } }
        public override bool Equals(object obj)
        {
            Pixel p = (Pixel)obj;
            if (this.y == p.Y && this.x == p.X)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            int hashCode = -624234986;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    }
}
