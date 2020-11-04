using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MeshGenerator.Elements;
using MeshGenerator.Materials;
using MeshGenerator.Model;
using MeshGenerator.Scene;
using MeshGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MeshGenerator.Utils.MathOps;

namespace MeshGenerator.Modelling.Solvers
{
    public class StressStrain<T> : ISolve<T>
    {
        protected const int DEGREES_OF_FREEDOM = 3;
        protected T globalMatrix;
        protected List<Node> nodes;
        protected List<Tetrahedron> tetrahedrons;

        #region Constructors
        /// <summary>
        /// The problem of stress-strain state on the model data
        /// </summary>
        public StressStrain(FeModel model)
        {
            nodes = model.Nodes;
            tetrahedrons = model.Tetrahedrons;
        }

        /// <summary>
        /// The problem of stress-strain state on the scene data
        /// </summary>
        public StressStrain(IScene scene)
        {
            nodes = scene.Nodes;
            tetrahedrons = scene.Tetrahedrons;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Global matrix of stress strain solve
        /// </summary>
        public T GlobalMatrix
        {
            get
            {
                return globalMatrix;
            }
        }
        #endregion

        #region Methods
        protected double[,] GenerateE(double G, double v)
        {
            //deformation
            //double a = (1 - 2 * v) / 2.0 * (1 - v);
            //double b = v / (1 - v);

            //double[,] E = {
            //{ 1, b, b, 0, 0, 0 },
            //{ b, 1, b, 0, 0, 0 },
            //{ b, b, 1, 0, 0, 0 },
            //{ 0, 0, 0, a, 0, 0 },
            //{ 0, 0, 0, 0, a, 0 },
            //{ 0, 0, 0, 0, 0, a } };

            //double koefficient = G * (1 - v) / ((1 + v) * (1 - 2.0 * v));

            //stress
            //double a = (1 - v) / 2.0;
            //double b = v;

            //double[,] E = {
            //{ 1, b, b, 0, 0, 0 },
            //{ b, 1, b, 0, 0, 0 },
            //{ b, b, 1, 0, 0, 0 },
            //{ 0, 0, 0, a, 0, 0 },
            //{ 0, 0, 0, 0, a, 0 },
            //{ 0, 0, 0, 0, 0, a } };

            //double koefficient = G / (1 - v * v);

            double a = 1 - v;
            double b = (1 - 2.0 * v) / 2.0;
            double c = v;

            double[,] E = {
            { a, c, c, 0, 0, 0 },
            { c, a, c, 0, 0, 0 },
            { c, c, a, 0, 0, 0 },
            { 0, 0, 0, b, 0, 0 },
            { 0, 0, 0, 0, b, 0 },
            { 0, 0, 0, 0, 0, b } };

            double koefficient = G / ((1 + v) * (1 - 2.0 * v));
            E = Multiply(E, koefficient);
            return E;
        }

        protected double[,] GenerateA(Tetrahedron element)
        {
            int lenght = element.Nodes.Count * DEGREES_OF_FREEDOM;
            double[,] A = new double[lenght, lenght];

            for (int i = 0; i < element.Nodes.Count; i++)
            {
                A[i * 3, 0] = 1.0f;
                A[i * 3, 1] = element.Nodes[i].X;
                A[i * 3, 2] = element.Nodes[i].Y;
                A[i * 3, 3] = element.Nodes[i].Z;

                A[i * 3 + 1, 4] = 1.0f;
                A[i * 3 + 1, 5] = element.Nodes[i].X;
                A[i * 3 + 1, 6] = element.Nodes[i].Y;
                A[i * 3 + 1, 7] = element.Nodes[i].Z;

                A[i * 3 + 2, 8] = 1.0f;
                A[i * 3 + 2, 9] = element.Nodes[i].X;
                A[i * 3 + 2, 10] = element.Nodes[i].Y;
                A[i * 3 + 2, 11] = element.Nodes[i].Z;
            }

            return A;
        }

        protected double[,] GenerateB(Tetrahedron element)
        {
            double[,] A = GenerateA(element);
            return Inv(A, element);
        }

        protected double[,] GenerateQ()
        {
            double[,] Q = {
                { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0 },
                { 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0 } };

            return Q;
        }

        //protected double[,] GenerateLocalK(Tetrahedron element)
        //{
        //    int lenght = element.Nodes.Count * DEGREES_OF_FREEDOM;
        //    double[,] k = new double[lenght, lenght];
        //    Material material = MaterialStorage.Materials.FirstOrDefault(m => m.Id == element.IdMaterial);

        //    double[] b = new double[DEGREES_OF_FREEDOM + 1];
        //    double[] c = new double[DEGREES_OF_FREEDOM + 1];
        //    double[] d = new double[DEGREES_OF_FREEDOM + 1];

        //    double vol = element.VolumeWithSign();
        //    double sign = Math.Abs(vol) / vol;
        //    double G = material.ElasticModulus / (2 * (1 + material.PoissonsRatio));
        //    double h = material.PoissonsRatio * material.ElasticModulus
        //        / ((1 + material.PoissonsRatio) * (1 - 2 * material.PoissonsRatio));
        //    double r = 2 * G + h;

        //    vol = 1 / (36 * Math.Abs(vol));

        //    b[0] = -Det(element.Nodes[1].Y, element.Nodes[1].Z, element.Nodes[2].Y, element.Nodes[2].Z, element.Nodes[3].Y, element.Nodes[3].Z) * sign;
        //    b[1] = Det(element.Nodes[0].Y, element.Nodes[0].Z, element.Nodes[2].Y, element.Nodes[2].Z, element.Nodes[3].Y, element.Nodes[3].Z) * sign;
        //    b[2] = -Det(element.Nodes[0].Y, element.Nodes[0].Z, element.Nodes[1].Y, element.Nodes[1].Z, element.Nodes[3].Y, element.Nodes[3].Z) * sign;
        //    b[3] = Det(element.Nodes[0].Y, element.Nodes[0].Z, element.Nodes[1].Y, element.Nodes[1].Z, element.Nodes[2].Y, element.Nodes[2].Z) * sign;

        //    c[0] = Det(element.Nodes[1].X, element.Nodes[1].Z, element.Nodes[2].X, element.Nodes[2].Z, element.Nodes[3].X, element.Nodes[3].Z) * sign;
        //    c[1] = -Det(element.Nodes[0].X, element.Nodes[0].Z, element.Nodes[2].X, element.Nodes[2].Z, element.Nodes[3].X, element.Nodes[3].Z) * sign;
        //    c[2] = Det(element.Nodes[0].X, element.Nodes[0].Z, element.Nodes[1].X, element.Nodes[1].Z, element.Nodes[3].X, element.Nodes[3].Z) * sign;
        //    c[3] = -Det(element.Nodes[0].X, element.Nodes[0].Z, element.Nodes[1].X, element.Nodes[1].Z, element.Nodes[2].X, element.Nodes[2].Z) * sign;

        //    d[0] = -Det(element.Nodes[1].X, element.Nodes[1].Y, element.Nodes[2].X, element.Nodes[2].Y, element.Nodes[3].X, element.Nodes[3].Y) * sign;
        //    d[1] = Det(element.Nodes[0].X, element.Nodes[0].Y, element.Nodes[2].X, element.Nodes[2].Y, element.Nodes[3].X, element.Nodes[3].Y) * sign;
        //    d[2] = -Det(element.Nodes[0].X, element.Nodes[0].Y, element.Nodes[1].X, element.Nodes[1].Y, element.Nodes[3].X, element.Nodes[3].Y) * sign;
        //    d[3] = Det(element.Nodes[0].X, element.Nodes[0].Y, element.Nodes[1].X, element.Nodes[1].Y, element.Nodes[2].X, element.Nodes[2].Y) * sign;

        //    for (int i = 0; i < DEGREES_OF_FREEDOM + 1; i++)
        //    {
        //        for (int j = 0; j < DEGREES_OF_FREEDOM + 1; j++)
        //        {
        //            k[i * DEGREES_OF_FREEDOM, j * DEGREES_OF_FREEDOM] = b[i] * b[j] * r + (c[i] * c[j] + d[i] * d[j]) * G;
        //            k[i * DEGREES_OF_FREEDOM, j * DEGREES_OF_FREEDOM + 1] = b[i] * c[j] * h + c[i] * b[j] * G;
        //            k[i * DEGREES_OF_FREEDOM, j * DEGREES_OF_FREEDOM + 2] = b[i] * d[j] * h + d[i] * b[j] * G;
        //            k[i * DEGREES_OF_FREEDOM + 1, j * DEGREES_OF_FREEDOM] = c[i] * b[j] * h + b[i] * c[j] * G;
        //            k[i * DEGREES_OF_FREEDOM + 1, j * DEGREES_OF_FREEDOM + 1] = c[i] * c[j] * r + (b[i] * b[j] + d[i] * d[j]) * G;
        //            k[i * DEGREES_OF_FREEDOM + 1, j * DEGREES_OF_FREEDOM + 2] = c[i] * d[j] * h + d[i] * c[j] * G;
        //            k[i * DEGREES_OF_FREEDOM + 2, j * DEGREES_OF_FREEDOM] = d[i] * b[j] * h + b[i] * d[j] * G;
        //            k[i * DEGREES_OF_FREEDOM + 2, j * DEGREES_OF_FREEDOM + 1] = d[i] * c[j] * h + c[i] * d[j] * G;
        //            k[i * DEGREES_OF_FREEDOM + 2, j * DEGREES_OF_FREEDOM + 2] = d[i] * d[j] * r + (c[i] * c[j] + b[i] * b[j]) * G;
        //        }
        //    }

        //    k = Multiply(k, vol);
        //    return k;
        //}

        protected double[,] GenerateLocalK(Tetrahedron element)
        {
            int lenght = element.Nodes.Count * DEGREES_OF_FREEDOM;
            double[,] k = new double[lenght, lenght];

            Material material = MaterialStorage.Materials.FirstOrDefault(m => m.Id == element.IdMaterial);
            double[,] E = GenerateE(material.ElasticModulus, material.PoissonsRatio);
            double[,] Q = GenerateQ();
            double[,] B = GenerateB(element);
            double[,] N = Multiply(Q, B);

            k = Multiply(Transponate(N), E);
            k = Multiply(k, N);
            //k = Multiply(Transponate(B), Transponate(Q));
            //k = Multiply(k, E);
            //k = Multiply(k, Q);
            //k = Multiply(k, B);

            k = Multiply(k, element.Volume());
            //k = Multiply(k, element.VolumeWithSign());

            return k;
        }

        private double Det(double x1, double y1, double x2, double y2, double x3, double y3)
        {

            return x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2);
        }

        public int MaxHalfDiagonalWidth()
        {
            int maxWidth = tetrahedrons.Max(x => x.NodesIdDifference) + 1;
            return maxWidth * DEGREES_OF_FREEDOM;
        }
        #endregion
    }
}
