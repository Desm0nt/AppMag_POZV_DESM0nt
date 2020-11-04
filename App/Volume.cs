using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class Volume
    {
        public int xDim, yDim, zDim;
        public float pw, ph, pd;
        int[,,] vol;

        public Volume(List<Layer> layers, float PW, float PH)
        {
            pw = PW;
            ph = PH;
            pd = 0.8f;

            xDim = 512;
            yDim = 512;
            zDim = layers.Count;

            vol = new int[xDim, yDim, zDim];
            int z = 0;
            foreach (var layer in layers)
            {
                for (int w = 0; w < layer.Dicom.width; w = w + 1)
                {
                    for (int h = 0; h < layer.Dicom.height; h = h + 1)
                    {
                        vol[w, h, z] = (layer.Dicom.Pixels16[w * layer.Dicom.height + h] >= 32900) ? 1 : 0;
                    }
                }
                z++;
            }
        }
        public int load(int x, int y, int z)
        {
            try
            {
                return vol[x, y, z];
            }
            catch (Exception e)
            {
                throw new Exception("don't find xyz in volume: "+e.Message);
            }
        }

    }
}
