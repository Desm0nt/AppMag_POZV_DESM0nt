﻿using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Model;
using System.Threading;
using MeshGenerator.Elements;
using MeshGenerator.Scene;
using System.Numerics;

namespace MeshGenerator.Modelling.Solvers
{
    public class StressStrainSparseMatrix : StressStrain<SparseMatrix>
    {
        #region Constructors
        /// <summary>
        /// The problem of stress-strain state on the model data
        /// </summary>
        public StressStrainSparseMatrix(FeModel model) : base(model)
        {
            GenerateGlobalMatrix();
        }

        /// <summary>
        /// The problem of stress-strain state on the scene data
        /// </summary>
        public StressStrainSparseMatrix(IScene scene) : base(scene)
        {
            GenerateGlobalMatrix();
        }
        #endregion

        #region Methods
        private void GenerateGlobalMatrix()
        {
            int lenght = nodes.Count * DEGREES_OF_FREEDOM;
            globalMatrix = new SparseMatrix(lenght, lenght);
            int elementCount = tetrahedrons.Count;
            //using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            //{
            foreach (var tetrahedron in tetrahedrons)
            {
                //ThreadPool.QueueUserWorkItem(elem =>
                //{
                //Tetrahedron tetrahedron = elem as Tetrahedron;
                double[,] k = GenerateLocalK(tetrahedron);

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

                                double oldValue = globalMatrix[gI, gJ];
                                globalMatrix[gI, gJ] = oldValue + k[locI, locJ];
                            }
                        }
                    }
                }

                //    if (Interlocked.Decrement(ref elementCount) == 0)
                //    {
                //        resetEvent.Set();
                //    }
                //}, element);
            }
            //    resetEvent.WaitOne();
            //}

        }
        #endregion
    }
}
