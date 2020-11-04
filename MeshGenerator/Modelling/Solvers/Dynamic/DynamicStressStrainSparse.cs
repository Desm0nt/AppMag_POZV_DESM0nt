using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MeshGenerator.Model;
using MeshGenerator.Scene;
using MeshGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Solvers.Dynamic
{
    public class DynamicStressStrainSparse : DynamicStressStrain<SparseMatrix>
    {
        private double period = Math.PI / 2;
        #region Constructors
        /// <summary>
        /// The problem of stress-strain state on the model data
        /// </summary>
        public DynamicStressStrainSparse(FeModel model) : base(model)
        {
            GenerateGlobalMatrix();
        }

        /// <summary>
        /// The problem of stress-strain state on the scene data
        /// </summary>
        public DynamicStressStrainSparse(IScene scene) : base(scene)
        {
            GenerateGlobalMatrix();
        }
        #endregion

        #region Methods
        private void GenerateGlobalMatrix()
        {
            int lenght = nodes.Count * DEGREES_OF_FREEDOM;
            globalMatrix = new SparseMatrix(lenght * 2, lenght * 2);

            //SparseMatrix K = new SparseMatrix(lenght);
            //SparseMatrix M = new SparseMatrix(lenght);
            //SparseMatrix C = new SparseMatrix(lenght);

            foreach (var tetrahedron in tetrahedrons)
            {
                double[,] k = GenerateLocalK(tetrahedron);
                double[,] m = GenerateLocalM(tetrahedron);
                double[,] c = GenerateLocalС(tetrahedron);

                for (int si = 0; si < tetrahedron.Nodes.Count; si++)
                {
                    for (int sj = 0; sj < tetrahedron.Nodes.Count; sj++)
                    {
                        for (int ki = 0; ki < DEGREES_OF_FREEDOM; ki++)
                        {
                            for (int kj = 0; kj < DEGREES_OF_FREEDOM; kj++)
                            {

                                int gSi = tetrahedron.Nodes[si].GlobalIndex;
                                int gSj = tetrahedron.Nodes[sj].GlobalIndex;
                                int gI = gSi * DEGREES_OF_FREEDOM + ki;
                                int gJ = gSj * DEGREES_OF_FREEDOM + kj;
                                int locI = si * DEGREES_OF_FREEDOM + ki;
                                int locJ = sj * DEGREES_OF_FREEDOM + kj;

                                double kmm = globalMatrix.At(gI, gJ) + (k[locI, locJ] - m[locI, locJ] * Math.Pow(period, 2));
                                double cm = globalMatrix.At(gI + lenght, gJ) + c[locI, locJ] * period * (-1);

                                globalMatrix[gI, gJ] = kmm;
                                globalMatrix[gI + globalMatrix.RowCount >> 1, gJ + globalMatrix.RowCount >> 1] = kmm;
                                globalMatrix[gI + globalMatrix.RowCount >> 1, gJ] = cm;
                                globalMatrix[gI, gJ + globalMatrix.RowCount >> 1] = cm;

                                //K[gI, gJ] = K.At(gI, gJ) + k[locI, locJ];
                                //M[gI, gJ] = M.At(gI, gJ) + m[locI, locJ];
                                //C[gI, gJ] = C.At(gI, gJ) + c[locI, locJ];
                            }
                        }
                    }
                }
            }

            //C.Multiply(period * (-1));
            //M.Multiply(Math.Pow(period, 2));
            //K.Subtract(M);

            //globalMatrix.SetSubMatrix(0, 0, K);
            //globalMatrix.SetSubMatrix(lenght, lenght, K);
            //globalMatrix.SetSubMatrix(0, lenght, C);
            //globalMatrix.SetSubMatrix(lenght, 0, C);
            
            //for (int i = 0; i < C.RowCount; i++)
            //{
            //    Vector<double> vector = C.Row(i);
            //    globalMatrix.SetRow(i, vector.Count, vector.Count, vector);
            //    globalMatrix.SetRow(i + vector.Count, 0, vector.Count, vector);
                //for (int j = 0; j < vector.Count; j++)
                //{
                //    double tmp = vector.At(j);
                //    globalMatrix[i + C.RowCount, j] = tmp;
                //    globalMatrix[i, j + C.ColumnCount] = tmp;
                //}
            //}
        }
        #endregion
    }
}
