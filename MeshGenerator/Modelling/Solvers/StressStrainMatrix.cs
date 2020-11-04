using MeshGenerator.Elements;
using MeshGenerator.Materials;
using MeshGenerator.Model;
using MeshGenerator.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MeshGenerator.Utils.MathOps;

namespace MeshGenerator.Modelling.Solvers
{
    /// <summary>
    /// The problem of stress-strain state (for tetrahedral finite element)
    /// </summary>
    public class StressStrainMatrix : StressStrain<double[,]>
    {
        #region Constructors
        /// <summary>
        /// The problem of stress-strain state on the model data
        /// </summary>
        public StressStrainMatrix(FeModel model) : base(model)
        {
            GenerateGlobalMatrix();
        }

        /// <summary>
        /// The problem of stress-strain state on the scene data
        /// </summary>
        public StressStrainMatrix(IScene scene) : base(scene)
        {
            GenerateGlobalMatrix();
        }
        #endregion

        #region Methods
        private void GenerateGlobalMatrix()
        {
            int lenght = nodes.Count * DEGREES_OF_FREEDOM;
            globalMatrix = new double[lenght, lenght];

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

                                globalMatrix[gI, gJ] += k[locI, locJ];
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
