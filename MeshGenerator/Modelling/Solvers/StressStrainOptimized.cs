using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Model;
using MeshGenerator.Scene;

namespace MeshGenerator.Modelling.Solvers
{
    /// <summary>
    /// The optimized (for using only sets diagonals) problem of stress-strain state (for tetrahedral finite element)
    /// </summary>
    public class StressStrainOptimized : StressStrain<double[,]>
    {
        #region Constructors
        /// <summary>
        /// The problem of stress-strain state on the model data
        /// </summary>
        public StressStrainOptimized(FeModel model) : base(model)
        {
            GenerateGlobalMatrix();
        }

        /// <summary>
        /// The problem of stress-strain state on the scene data
        /// </summary>
        public StressStrainOptimized(IScene scene) : base(scene)
        {
            GenerateGlobalMatrix();
        }
        #endregion

        #region Methods
        private void GenerateGlobalMatrix()
        {
            int lenght = nodes.Count * DEGREES_OF_FREEDOM;
            int width = MaxHalfDiagonalWidth();
            globalMatrix = new double[lenght, width];

            foreach (var element in tetrahedrons)
            {
                double[,] k = GenerateLocalK(element);

                for (int si = 0; si < element.Nodes.Count; si++)
                {
                    for (int sj = 0; sj < element.Nodes.Count; sj++)
                    {
                        for (int ki = 0; ki < DEGREES_OF_FREEDOM; ki++)
                        {
                            for (int kj = 0; kj < DEGREES_OF_FREEDOM; kj++) // this may be optimized by going lower diagonal elements
                            {
                                int gSi = element.Nodes[si].GlobalIndex;
                                int gSj = element.Nodes[sj].GlobalIndex;
                                int gI = gSi * DEGREES_OF_FREEDOM + ki;
                                int gJ = gSj * DEGREES_OF_FREEDOM + kj;
                                int locI = si * DEGREES_OF_FREEDOM + ki;
                                int locJ = sj * DEGREES_OF_FREEDOM + kj;

                                if (gI - gJ  <= 0)
                                {
                                    globalMatrix[gI, gJ - gI] += k[locI, locJ];
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
