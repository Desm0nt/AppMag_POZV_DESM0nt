using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Model;
using MeshGenerator.Scene;
using MeshGenerator.Elements;
using MeshGenerator.Materials;
using static MeshGenerator.Utils.MathOps;

namespace MeshGenerator.Modelling.Solvers.Dynamic
{
    public class DynamicStressStrain<T> : StressStrain<T>
    {
        #region Constructors
        public DynamicStressStrain(FeModel model) : base(model)
        {
        }

        public DynamicStressStrain(IScene scene) : base(scene)
        {
        }
        #endregion

        #region Methods
        protected double[,] GenerateLocalM(Tetrahedron element)
        {
            int lenght = element.Nodes.Count * DEGREES_OF_FREEDOM;
            double[,] k = new double[lenght, lenght];
            Material material = MaterialStorage.Materials.FirstOrDefault(m => m.Id == element.IdMaterial);
            double[,] Q = GenerateQ();
            double[,] B = GenerateB(element);
            double[,] N = Multiply(Q, B);

            k = Multiply(Transponate(N), material.Density);
            k = Multiply(k, N);
            k = Multiply(k, element.Volume());

            return k;
        }

        protected double[,] GenerateLocalС(Tetrahedron element)
        {
            int lenght = element.Nodes.Count * DEGREES_OF_FREEDOM;
            double[,] k = new double[lenght, lenght];
            Material material = MaterialStorage.Materials.FirstOrDefault(m => m.Id == element.IdMaterial);
            double[,] Q = GenerateQ();
            double[,] B = GenerateB(element);
            double[,] N = Multiply(Q, B);

            k = Multiply(Transponate(N), material.PoissonsRatio); // may be vhange on internal friction
            k = Multiply(k, N);
            k = Multiply(k, element.Volume());

            return k;
        }
        #endregion
    }
}
