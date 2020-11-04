using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Modelling.Conditions;
using MeshGenerator.Modelling.Loads;
using MeshGenerator.Modelling.Solvers;
using MeshGenerator.Utils;

namespace MeshGenerator.Modelling.Solutions
{
    public class StaticMechanicMemorySolution : ISolution
    {
        private const int DEGREES_OF_FREEDOM = 3;
        private ISolve<IntPtr> solver;
        private double[] results;
        private double[] loads;
        private IntPtr globalMatrix;
        private int lengthMatrix;

        public StaticMechanicMemorySolution(ISolve<IntPtr> solver, int lengthMatrix)
        {
            this.solver = solver;
            globalMatrix = solver.GlobalMatrix;
            this.lengthMatrix = lengthMatrix;
            loads = new double[lengthMatrix];
            results = new double[lengthMatrix];
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
            results = new double[lengthMatrix];

            SetLoads(load);
            SetBoundaryConditions(type, conditions);

            results = MathOps.MethodOfGauss(ref globalMatrix, loads);
        }

        private void SetBoundaryConditions(TypeOfFixation type, IBoundaryCondition conditions)
        {
            foreach (var item in conditions.FixedNodes)
            {
                int ind = item.Key * DEGREES_OF_FREEDOM;
                switch (type)
                {
                    case TypeOfFixation.RIGID:
                        MathOps.SetZerosRow(ref globalMatrix, lengthMatrix, ind);
                        MemoryMatrix.SetDoubleValue(ref globalMatrix, ind * lengthMatrix + ind, 1.0);
                        MathOps.SetZerosRow(ref globalMatrix, lengthMatrix, ind + 1);
                        MemoryMatrix.SetDoubleValue(ref globalMatrix, ((ind + 1) * lengthMatrix) + ind + 1, 1.0);
                        MathOps.SetZerosRow(ref globalMatrix, lengthMatrix, ind + 2);
                        MemoryMatrix.SetDoubleValue(ref globalMatrix, ((ind + 2) * lengthMatrix) + ind + 2, 1.0);

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

        private void SetLoads(ILoad load)
        {
            loads = new double[lengthMatrix];
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
