using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Model;
using MeshGenerator.Scene;
using MeshGenerator.Utils;
using System.Runtime.InteropServices;

namespace MeshGenerator.Modelling.Solvers
{
    public class StressStrainDictionaryMemory : StressStrain<Dictionary<int, IntPtr>>
    {
        #region Constructors
        public StressStrainDictionaryMemory(FeModel model) : base(model)
        {
            GenerateGlobalMatrix();
        }

        public StressStrainDictionaryMemory(IScene scene) : base(scene)
        {
            GenerateGlobalMatrix();
        }
        #endregion

        #region Methods
        private void GenerateGlobalMatrix()
        {
            int length = nodes.Count * DEGREES_OF_FREEDOM;

            //MemoryMatrix.Init(ref globalMatrix, length);

            //globalMatrix = new Dictionary<int, IntPtr>(length);
            //for (int i = 0; i < length; i++)
            //{
            //    globalMatrix.Add(i, Marshal.AllocHGlobal(sizeof(double) * length));
            //    //globalMatrix.Add(i, Marshal.AllocCoTaskMem(sizeof(double) * length));
            //}

            Dictionary<int, double[]> dictionary = new Dictionary<int, double[]>(length);
            for (int i = 0; i < length; i++)
            {
                dictionary.Add(i, new double[length]);
                //globalMatrix.Add(i, Marshal.AllocCoTaskMem(sizeof(double) * length));
            }

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    MemoryMatrix.SetDoubleValue(ref globalMatrix, i, j, 0);
                }
            }

            foreach (var element in tetrahedrons)
            {
                double[,] k = GenerateLocalK(element);

                for (int si = 0; si < element.Nodes.Count; si++)
                {
                    for (int sj = 0; sj < element.Nodes.Count; sj++)
                    {
                        for (int ki = 0; ki < DEGREES_OF_FREEDOM; ki++)
                        {
                            for (int kj = 0; kj < DEGREES_OF_FREEDOM; kj++)
                            {

                                int gSi = element.Nodes[si].GlobalIndex;
                                int gSj = element.Nodes[sj].GlobalIndex;
                                int gI = gSi * DEGREES_OF_FREEDOM + ki;
                                int gJ = gSj * DEGREES_OF_FREEDOM + kj;
                                int locI = si * DEGREES_OF_FREEDOM + ki;
                                int locJ = sj * DEGREES_OF_FREEDOM + kj;
                                
                                double tmp = MemoryMatrix.GetDoubleValue(ref globalMatrix, gI, gJ);
                                MemoryMatrix.SetDoubleValue(ref globalMatrix, gI, gJ, tmp + k[locI, locJ]);

                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
