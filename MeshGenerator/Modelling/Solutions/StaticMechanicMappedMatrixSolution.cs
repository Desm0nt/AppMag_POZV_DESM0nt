using MeshGenerator.Modelling.Conditions;
using MeshGenerator.Modelling.Loads;
using MeshGenerator.Modelling.Solvers;
using MeshGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Solutions
{
    public class StaticMechanicMappedMatrixSolution : ISolution
    {
        private const string DIRECTORY_PATH = "global_matrix/";
        private const int DEGREES_OF_FREEDOM = 3;
        private const double EPSILON = 0.0000000001;
        private ISolve<string[]> solver;
        private double[] results;
        private double[] loads;
        private string[] globalMatrix;

        public StaticMechanicMappedMatrixSolution(ISolve<string[]> solver)
        {
            this.solver = solver;
            globalMatrix = solver.GlobalMatrix;
            loads = new double[solver.GlobalMatrix.Length];
            results = new double[solver.GlobalMatrix.Length];
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
            results = new double[solver.GlobalMatrix.Length];

            SetLoads(load);
            SetBoundaryConditions(type, conditions);

            results = MathOps.MethodOfGauss(DIRECTORY_PATH, globalMatrix, loads);
            //results = MathOps.CGMethod(DIRECTORY_PATH, globalMatrix, loads, EPSILON);

        }

        private void SetBoundaryConditions(TypeOfFixation type, IBoundaryCondition conditions)
        {
            foreach (var item in conditions.FixedNodes)
            {
                int ind = item.Key * DEGREES_OF_FREEDOM;
                switch (type)
                {
                    case TypeOfFixation.RIGID:
                        SetRowConditions(ind);
                        SetRowConditions(ind + 1);
                        SetRowConditions(ind + 2);
                        //MathOps.SetZerosRow(accessor, lengthMatrix, ind + 1);
                        //MappedMatrix.SetDoubleValue(accessor, ind + 1, ind + 1, lengthMatrix, 1.0);
                        //MathOps.SetZerosRow(accessor, lengthMatrix, ind + 2);
                        //MappedMatrix.SetDoubleValue(accessor, ind + 2, ind + 2, lengthMatrix, 1.0);

                        loads[ind] = 0.0;
                        loads[ind + 1] = 0.0;
                        loads[ind + 2] = 0.0;
                        break;
                    case TypeOfFixation.ARTICULATION_YZ: throw new NotImplementedException();
                    case TypeOfFixation.ARTICULATION_XZ: throw new NotImplementedException();
                    case TypeOfFixation.ARTICULATION_XY: throw new NotImplementedException();
                    default: throw new ArgumentException("Wrong type of the fixation");
                }
            }
        }

        private void SetRowConditions(int index)
        {
            using (var mappedFile = MappedMatrix.Open($"{DIRECTORY_PATH}{globalMatrix[index]}.matrix", globalMatrix[index]))
            {
                using (var accessor = mappedFile.CreateViewAccessor())
                {
                    MathOps.SetZerosRow(accessor, globalMatrix.Length);
                    MappedMatrix.SetDoubleValue(accessor, index, 1.0);
                }
            }
        }

        private void SetLoads(ILoad load)
        {
            loads = new double[solver.GlobalMatrix.Length];
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
