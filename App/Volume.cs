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
            int a = 8;
            pw = PW;
            ph = PH;
            pd = 0.8f / a;

            xDim = 512 / a;
            yDim = 512 / a;
            zDim = layers.Count;

            vol = new int[xDim, yDim, zDim];
            int z = 0;
            foreach (var layer in layers)
            {
                for (int w = 0; w < layer.Dicom.width / a; w = w + 1)
                {
                    for (int h = 0; h < layer.Dicom.height / a; h = h + 1)
                    {
                        //vol[w, h, z] = (layer.Dicom.Pixels16[w * layer.Dicom.height + h] >= 32900) ? 1 : 0;
                        vol[w, h, z] = (layer.ImageFromJpg.GetPixel(w, h).GetBrightness() > 0.9) ? 1 : 0;
                    }
                }
                z++;
            }
        }


        //public Volume(List<Layer> layers, float PW, float PH)
        //{
        //    int a = 4;
        //    pw = PH;
        //    ph = PW;
        //    pd = 0.8f / a;

        //    xDim = 512 / a;
        //    yDim = 512 / a;
        //    zDim = layers.Count;

        //    vol = new int[xDim, yDim, zDim];
        //    int z = 0;
        //    foreach (var layer in layers)
        //    {
        //        for (int w = 0; w < layer.Dicom.width / a; w = w + 1)
        //        {
        //            for (int h = 0; h < layer.Dicom.height / a; h = h + 1)
        //            {
        //                //vol[w, h, z] = (layer.Dicom.Pixels16[w * layer.Dicom.height + h] >= 32900) ? 1 : 0;
        //                vol[h, w, z] = (layer.ImageFromJpg.GetPixel(h, w).GetBrightness() > 0.9) ? 1 : 0;
        //            }
        //        }
        //        z++;
        //    }
        //}

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
