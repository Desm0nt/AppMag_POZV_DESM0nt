using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    /// <summary>
    /// Нормали вершин
    /// </summary>
    public class RNormal
    {

        public double NX { get; set; }
        public double NY { get; set; }
        public double NZ { get; set; }

        public RNormal(double nx, double ny, double nz)
        {
            NX = nx;
            NY = ny;
            NZ = nz;
        }
    }
}
