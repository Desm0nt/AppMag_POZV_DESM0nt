//using ManagedCuda.CudaSolve;
//using ManagedCuda.CudaSparse;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using MeshGenerator.Model;
using MeshGenerator.Modelling.Conditions;
using MeshGenerator.Modelling.Loads;
using MeshGenerator.Modelling.Solvers;
using MeshGenerator.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Solutions
{
    public class StaticMechanicCudaSolution : ISolution
    {
        private const double ACCURACY = 0.0000000001;
        private const int DEGREES_OF_FREEDOM = 3;
        private ISolve<SparseMatrix> solver;
        private int length;
        private double[] results;
        private double[] loads;
        private SparseMatrix globalMatrix;

        public StaticMechanicCudaSolution(ISolve<SparseMatrix> solver, FeModel model)
        {
            this.solver = solver;
            globalMatrix = solver.GlobalMatrix;
            length = model.Nodes.Count * DEGREES_OF_FREEDOM;
            loads = new double[length];
            results = new double[length];
        }

        public StaticMechanicCudaSolution(ISolve<SparseMatrix> solver, IScene scene)
        {
            this.solver = solver;
            globalMatrix = solver.GlobalMatrix;
            length = scene.Nodes.Count * DEGREES_OF_FREEDOM;
            loads = new double[length];
            results = new double[length];
        }

        public double[] Results => results;

        public void Solve(TypeOfFixation type, IBoundaryCondition conditions, ILoad load)
        {
            globalMatrix = solver.GlobalMatrix;
            results = new double[length];


            SetLoads(load);
            SetBoundaryConditions(type, conditions, globalMatrix);

            var storage = globalMatrix.Storage as SparseCompressedRowMatrixStorage<double>;
            List<double> nonZeroRows = globalMatrix.Storage.EnumerateNonZero().ToList();

            //CudaSolveSparse sp = new CudaSolveSparse();
            //CudaSparseMatrixDescriptor matrixDescriptor = new CudaSparseMatrixDescriptor();
            //        sp.CsrlsvluHost(globalMatrix.RowCount, nonZeroRows.Count, matrixDescriptor, nonZeroRows.ToArray(),
            //storage.RowPointers, storage.ColumnIndices, loads, ACCURACY, 0, results);

            for (int i = 0; i < results.Length; i++)
            {
                if (globalMatrix[i, i] == 1.0)
                {
                    results[i] = 0.0;
                }
            }
        }

        private void SetBoundaryConditions(TypeOfFixation type, IBoundaryCondition conditions, SparseMatrix dictionaryMatrix)
        {
            foreach (var node in conditions.FixedNodes)
            {
                switch (type)
                {
                    case TypeOfFixation.RIGID:
                        globalMatrix.ClearRow(node.Key * DEGREES_OF_FREEDOM);
                        globalMatrix[node.Key * DEGREES_OF_FREEDOM, node.Key * DEGREES_OF_FREEDOM] = 1.0;

                        globalMatrix.ClearRow(node.Key * DEGREES_OF_FREEDOM + 1);
                        globalMatrix[node.Key * DEGREES_OF_FREEDOM + 1, node.Key * DEGREES_OF_FREEDOM + 1] = 1.0;

                        globalMatrix.ClearRow(node.Key * DEGREES_OF_FREEDOM + 2);
                        globalMatrix[node.Key * DEGREES_OF_FREEDOM + 2, node.Key * DEGREES_OF_FREEDOM + 2] = 1.0;

                        loads[node.Key * DEGREES_OF_FREEDOM] = 0.0;
                        loads[node.Key * DEGREES_OF_FREEDOM + 1] = 0.0;
                        loads[node.Key * DEGREES_OF_FREEDOM + 2] = 0.0;
                        break;
                    case TypeOfFixation.ARTICULATION_YZ: throw new NotImplementedException();
                    case TypeOfFixation.ARTICULATION_XZ: throw new NotImplementedException();
                    case TypeOfFixation.ARTICULATION_XY: throw new NotImplementedException();
                    default: throw new ArgumentException("Wrong type of the fixation");
                }
            }
        }

        private void SetLoads(ILoad load)
        {
            loads = new double[length];
            foreach (var item in load.LoadVectors)
            {
                switch (item.Value.Direction)
                {
                    case VectorDirection.X:
                        loads[item.Key * DEGREES_OF_FREEDOM] = item.Value.Value;
                        break;
                    case VectorDirection.Y:
                        loads[item.Key * DEGREES_OF_FREEDOM + 1] = item.Value.Value;
                        break;
                    case VectorDirection.Z:
                        loads[item.Key * DEGREES_OF_FREEDOM + 2] = item.Value.Value;
                        break;
                }
            }
        }
    }
}
