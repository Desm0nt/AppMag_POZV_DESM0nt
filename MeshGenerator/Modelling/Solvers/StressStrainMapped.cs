using MeshGenerator.Model;
using MeshGenerator.Scene;
using MeshGenerator.Utils;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Solvers
{
    public class StressStrainMapped : StressStrain<MemoryMappedFile>
    {
        #region Constructors
        /// <summary>
        /// The problem of stress-strain state on the model data
        /// </summary>
        public StressStrainMapped(FeModel model) : base(model)
        {
            GenerateGlobalMatrix();
        }

        /// <summary>
        /// The problem of stress-strain state on the scene data
        /// </summary>
        public StressStrainMapped(IScene scene) : base(scene)
        {
            GenerateGlobalMatrix();
        }
        #endregion

        #region Methods
        private void GenerateGlobalMatrix()
        {
            int lenght = nodes.Count * DEGREES_OF_FREEDOM;

            using (globalMatrix = MappedMatrix.Init("global.matrix", "globalMatrix", lenght))
            {
                using (var accessor = globalMatrix.CreateViewAccessor())
                {
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

                                        double tmp = MappedMatrix.GetDoubleValue(accessor, gI, gJ, lenght);
                                        MappedMatrix.SetDoubleValue(accessor, gI, gJ, lenght, tmp + k[locI, locJ]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
