using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Model;
using MeshGenerator.Scene;
using MeshGenerator.Utils;

namespace MeshGenerator.Modelling.Solvers
{
    public class StressStrainMappedMatix : StressStrain<string[]>
    {
        private const string DIRECTORY_PATH = "global_matrix/";
        #region Constructors
        /// <summary>
        /// The problem of stress-strain state on the model data
        /// </summary>
        public StressStrainMappedMatix(FeModel model) : base(model)
        {
            InitFilePathes();
            GenerateGlobalMatrix();
        }

        /// <summary>
        /// The problem of stress-strain state on the scene data
        /// </summary>
        public StressStrainMappedMatix(IScene scene) : base(scene)
        {
            InitFilePathes();
            GenerateGlobalMatrix();
        }
        #endregion

        #region Methods
        private void InitFilePathes()
        {
            int length = nodes.Count * DEGREES_OF_FREEDOM;
            globalMatrix = new string[length];
            for (int i = 0; i < length; i++)
            {
                globalMatrix[i] = $"global_{i}";
            }
        }
        private void GenerateGlobalMatrix()
        {
            int length = nodes.Count * DEGREES_OF_FREEDOM;
            
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

                                using (var mappedFile = MappedMatrix.OpenOrCreate($"{DIRECTORY_PATH}{GlobalMatrix[gI]}.matrix", GlobalMatrix[gI], GlobalMatrix.Length))
                                {
                                    using (var accessor = mappedFile.CreateViewAccessor())
                                    {
                                        double tmp = MappedMatrix.GetDoubleValue(accessor, gJ);
                                        MappedMatrix.SetDoubleValue(accessor, gJ, tmp + k[locI, locJ]);
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
