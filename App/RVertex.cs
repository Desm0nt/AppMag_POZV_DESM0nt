using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class RVertex
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public RVertex(double ix, double iy, double iz)
        {
            X = ix;
            Y = iy;
            Z = iz;
        }
        public RVertex()
        { }
    }
}
