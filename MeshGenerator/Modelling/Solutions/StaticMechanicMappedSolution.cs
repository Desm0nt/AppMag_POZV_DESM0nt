using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Modelling.Conditions;
using MeshGenerator.Modelling.Loads;
using System.IO.MemoryMappedFiles;
using MeshGenerator.Modelling.Solvers;
using MeshGenerator.Utils;

namespace MeshGenerator.Modelling.Solutions
{
    public class StaticMechanicMappedSolution : ISolution
    {
        private const int DEGREES_OF_FREEDOM = 3;
        //private ISolve<MemoryMappedFile> solver;
        private double[] results;
        private double[] loads;
        private MemoryMappedFile globalMatrix;
        private int lengthMatrix;

        public StaticMechanicMappedSolution(/*ISolve<MemoryMappedFile> solver, */int lengthMatrix)
        {
            //this.solver = solver;
            //globalMatrix = solver.GlobalMatrix;
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
            //globalMatrix = solver.GlobalMatrix;
            results = new double[lengthMatrix];

            using (globalMatrix = MappedMatrix.Open("global.matrix", "globalMatrix"))
            {
                using (var accessor = globalMatrix.CreateViewAccessor())
                {
                    SetLoads(load);
                    SetBoundaryConditions(accessor, type, conditions);

                    results = MathOps.MethodOfGauss(accessor, loads);
                }
            }
        }

        private void SetBoundaryConditions(MemoryMappedViewAccessor accessor, TypeOfFixation type, IBoundaryCondition conditions)
        {
            foreach (var item in conditions.FixedNodes)
            {
                int ind = item.Key * DEGREES_OF_FREEDOM;
                switch (type)
                {
                    case TypeOfFixation.RIGID:
                        MathOps.SetZerosRow(accessor, lengthMatrix, ind);
                        MappedMatrix.SetDoubleValue(accessor, ind, ind, lengthMatrix, 1.0);
                        MathOps.SetZerosRow(accessor, lengthMatrix, ind + 1);
                        MappedMatrix.SetDoubleValue(accessor, ind + 1, ind + 1, lengthMatrix, 1.0);
                        MathOps.SetZerosRow(accessor, lengthMatrix, ind + 2);
                        MappedMatrix.SetDoubleValue(accessor, ind + 2, ind + 2, lengthMatrix, 1.0);

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
