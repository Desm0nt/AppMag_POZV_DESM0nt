using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class PointHolder
    {
        private Volume volume;
        public int width, height, depth;
        public float threshold;

        public PointHolder(int width, int height, int depth, Volume v)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            volume = v;
        }

        public int Intensity(Vertex p)
        {
            if (p.X < 0 || p.Y < 0 || p.Z < 0 || p.X >= width || p.Y >= height || p.Z >= depth)
            {
                return 0;
            }
            return volume.load((int)p.X, (int)p.Y, (int)p.Z); 
        }

    }
}
