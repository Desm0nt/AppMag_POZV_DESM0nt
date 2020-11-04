using MeshGenerator.Model;
using MeshGenerator.Scene;
using MeshGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Solvers
{
    /// <summary>
    /// The problem of stress-strain state (for tetrahedral finite element)
    /// </summary>
    public class StressStrainMemory : StressStrain<IntPtr>
    {
        #region Constructors
        /// <summary>
        /// The problem of stress-strain state on the model data
        /// </summary>
        public StressStrainMemory(FeModel model) : base(model)
        {
            GenerateGlobalMatrix();
        }

        /// <summary>
        /// The problem of stress-strain state on the scene data
        /// </summary>
        public StressStrainMemory(IScene scene) : base(scene)
        {
            GenerateGlobalMatrix();
        }
        #endregion

        #region Methods
        private void GenerateGlobalMatrix()
        {
            int length = nodes.Count * DEGREES_OF_FREEDOM;

            ulong size = sizeof(double) * (ulong)length * (ulong)length;
            
            MemoryMatrix.Init(ref globalMatrix, size);

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    //MemoryMatrix.SetDoubleValue(ref globalMatrix, i * length + j, 0);
                    MemoryMatrix.SetDoubleValue(ref globalMatrix, i, j, length, 0);
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

                                //double tmp = MemoryMatrix.GetDoubleValue(ref globalMatrix, gI * length + gJ);
                                //MemoryMatrix.SetDoubleValue(ref globalMatrix, gI * length + gJ, tmp + k[locI, locJ]);

                                double tmp = MemoryMatrix.GetDoubleValue(ref globalMatrix, gI, gJ, length);
                                MemoryMatrix.SetDoubleValue(ref globalMatrix, gI, gJ, length, tmp + k[locI, locJ]);

                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
