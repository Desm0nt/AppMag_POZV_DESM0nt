using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Modelling.Conditions;
using MeshGenerator.Modelling.Loads;
using MeshGenerator.Modelling.Solvers;
using MeshGenerator.Utils;
using MeshGenerator.Model;
using System.Threading;
using MeshGenerator.Elements;
using MeshGenerator.Scene;

namespace MeshGenerator.Modelling.Solutions
{
    public class StaticMechanicDictSolution : ISolution
    {
        private const double EPSILON = 0.01;
        private const int DEGREES_OF_FREEDOM = 3;
        private ISolve<DictionaryMatrix> solver;
        private int length;
        private double[] results;
        private double[] loads;
        private DictionaryMatrix globalMatrix;

        public StaticMechanicDictSolution(ISolve<DictionaryMatrix> solver, FeModel model)
        {
            this.solver = solver;
            globalMatrix = solver.GlobalMatrix;
            length = model.Nodes.Count * DEGREES_OF_FREEDOM;
            loads = new double[length];
            results = new double[length];
        }

        public StaticMechanicDictSolution(ISolve<DictionaryMatrix> solver, IScene scene)
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

            //MathOps.IncompleteCholeskyFactorization(ref globalMatrix);
            //DictionaryMatrix C = MathOps.Multiply(ref globalMatrix);

            AddZeros();
            SetLoads(load);
            //SetBoundaryConditions(type, conditions, C);
            SetBoundaryConditions(type, conditions, globalMatrix);

            results = MathOps.MethodOfGauss(globalMatrix, loads);
            //results = MathOps.CGMethod(globalMatrix, loads, EPSILON);
            //results = MathOps.CGMethod(C, loads, EPSILON);
        }

        private void AddZeros()
        {
            foreach (var row in globalMatrix.Rows)
            {
                int rightIndex = row.Value.Columns.Count > 0
                 ? row.Value.Columns.Keys.Max()
                 : 0;
                //int leftIndex = row.Value.Columns.Count > 0
                // ? row.Value.Columns.Keys.Min()
                // : row.Key;
                int leftIndex = row.Key;

                for (int i = leftIndex; i < rightIndex; i++)
                {
                    if (row.Value.GetValue(i) == 0.0)
                    {
                        row.Value.SetValue(i, 0.0);
                    }
                }

            }
            //int processCount = matrix.Rows.Count;
            //using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            //{
            //    foreach (var row in matrix.Rows)
            //    {
            //        ThreadPool.QueueUserWorkItem(currentRow =>
            //        {
            //            int rightIndex = ((KeyValuePair<int, MatrixRow>)currentRow).Value.Columns.Count > 0
            //             ?((KeyValuePair<int, MatrixRow>)currentRow).Value.Columns.Keys.Max()
            //             : 0;
            //            int leftIndex = ((KeyValuePair<int, MatrixRow>)currentRow).Key;
            //            //int leftIndex = ((KeyValuePair<int, MatrixRow>)currentRow).Value.Columns.Count > 0
            //            // ? ((KeyValuePair<int, MatrixRow>)currentRow).Value.Columns.Keys.Min()
            //            // : 0;

            //            for (int i = leftIndex; i < rightIndex; i++)
            //            {
            //                if (((KeyValuePair<int, MatrixRow>)currentRow).Value.GetValue(i) == 0.0)
            //                {
            //                    ((KeyValuePair<int, MatrixRow>)currentRow).Value.SetValue(i, 0.0);
            //                }
            //            }

            //            if (Interlocked.Decrement(ref processCount) == 0)
            //            {
            //                resetEvent.Set();
            //            }
            //        }, row);
            //    }
            //    resetEvent.WaitOne();
            //}
        }

        private void SetBoundaryConditions(TypeOfFixation type, IBoundaryCondition conditions, DictionaryMatrix dictionaryMatrix)
        {
            int processCount = conditions.FixedNodes.Count;
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                foreach (var item in conditions.FixedNodes)
                {
                    ThreadPool.QueueUserWorkItem(thrItem =>
                    {
                        KeyValuePair<int, Node> node = (KeyValuePair<int, Node>)thrItem;

                        switch (type)
                        {
                            case TypeOfFixation.RIGID:
                                MathOps.SetZerosRowColumn(ref dictionaryMatrix, node.Key * DEGREES_OF_FREEDOM, length);
                                dictionaryMatrix.SetValue(node.Key * DEGREES_OF_FREEDOM, node.Key * DEGREES_OF_FREEDOM, 1.0);
                                MathOps.SetZerosRowColumn(ref dictionaryMatrix, node.Key * DEGREES_OF_FREEDOM + 1, length);
                                dictionaryMatrix.SetValue(node.Key * DEGREES_OF_FREEDOM + 1, node.Key * DEGREES_OF_FREEDOM + 1, 1.0);
                                MathOps.SetZerosRowColumn(ref dictionaryMatrix, node.Key * DEGREES_OF_FREEDOM + 2, length);
                                dictionaryMatrix.SetValue(node.Key * DEGREES_OF_FREEDOM + 2, node.Key * DEGREES_OF_FREEDOM + 2, 1.0);

                                loads[node.Key * DEGREES_OF_FREEDOM] = 0.0;
                                loads[node.Key * DEGREES_OF_FREEDOM + 1] = 0.0;
                                loads[node.Key * DEGREES_OF_FREEDOM + 2] = 0.0;
                                break;
                            case TypeOfFixation.ARTICULATION_YZ: throw new NotImplementedException();
                            case TypeOfFixation.ARTICULATION_XZ: throw new NotImplementedException();
                            case TypeOfFixation.ARTICULATION_XY: throw new NotImplementedException();
                            default: throw new ArgumentException("Wrong type of the fixation");
                        }
                        if (Interlocked.Decrement(ref processCount) == 0)
                        {
                            resetEvent.Set();
                        }
                    }, item);
                }
                resetEvent.WaitOne();
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
