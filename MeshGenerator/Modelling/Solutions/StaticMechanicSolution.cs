using MeshGenerator.Modelling.Conditions;
using MeshGenerator.Modelling.Loads;
using MeshGenerator.Modelling.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Utils;

namespace MeshGenerator.Modelling.Solutions
{
    /// <summary>
    /// Static mechanic finite element solution
    /// </summary>
    public class StaticMechanicSolution : ISolution
    {
        private const int DEGREES_OF_FREEDOM = 3;
        private ISolve<double[,]> solver;
        private double[] results;
        private double[] loads;
        private double[,] globalMatrix;

        public StaticMechanicSolution(ISolve<double[,]> solver)
        {
            this.solver = solver;
            globalMatrix = solver.GlobalMatrix;
            loads = new double[globalMatrix.GetLength(0)];
            results = new double[globalMatrix.GetLength(0)];
        }

        public double[] Results => results;


        /// <summary>
        /// Solves the problem of stress-strain state
        /// </summary>
        /// <param name="type">Type of fixation</param>
        /// <param name="conditions">Boundary conditions</param>
        /// <returns>Result vector</returns>
        public void Solve(TypeOfFixation type, IBoundaryCondition conditions, ILoad load)
        {
            globalMatrix = solver.GlobalMatrix;
            results = new double[globalMatrix.GetLength(1)];

            SetLoads(load);
            SetBoundaryConditions(type, conditions);

            results = MathOps.GausSlau(globalMatrix, loads);
        }

        private void SetBoundaryConditions(TypeOfFixation type, IBoundaryCondition conditions)
        {
            foreach (var item in conditions.FixedNodes)
            {
                switch (type)
                {
                    case TypeOfFixation.RIGID:
                        MathOps.SetZerosRow(ref globalMatrix, item.Key * DEGREES_OF_FREEDOM);
                        globalMatrix[item.Key * DEGREES_OF_FREEDOM, item.Key * DEGREES_OF_FREEDOM] = 1.0;
                        MathOps.SetZerosRow(ref globalMatrix, item.Key * DEGREES_OF_FREEDOM + 1);
                        globalMatrix[item.Key * DEGREES_OF_FREEDOM + 1, item.Key * DEGREES_OF_FREEDOM + 1] = 1.0;
                        MathOps.SetZerosRow(ref globalMatrix, item.Key * DEGREES_OF_FREEDOM + 2);
                        globalMatrix[item.Key * DEGREES_OF_FREEDOM + 2, item.Key * DEGREES_OF_FREEDOM + 2] = 1.0;

                        loads[item.Key * DEGREES_OF_FREEDOM] = 0.0;
                        loads[item.Key * DEGREES_OF_FREEDOM + 1] = 0.0;
                        loads[item.Key * DEGREES_OF_FREEDOM + 2] = 0.0;
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
            loads = new double[globalMatrix.GetLength(1)];
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
