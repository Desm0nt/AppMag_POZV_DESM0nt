using MeshGenerator.Modelling.Conditions;
using MeshGenerator.Modelling.Loads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Solutions
{
    /// <summary>
    /// Solution finite element problem
    /// </summary>
    public interface ISolution
    {
        /// <summary>
        /// Solves the finite-element problem
        /// </summary>
        /// <param name="type">Type of fixation</param>
        /// <param name="conditions">Boundary conditions</param>
        ///  <param name="load">Load</param>
        /// <returns>Result vector</returns>
        void Solve(TypeOfFixation type, IBoundaryCondition conditions, ILoad load);

        double[] Results { get; }
    }
}
