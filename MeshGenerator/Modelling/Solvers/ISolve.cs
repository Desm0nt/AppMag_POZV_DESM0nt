using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Solvers
{
    /// <summary>
    /// The description of the solution of the finite-element problem
    /// </summary>
    public interface ISolve<T>
    {
        /// <summary>
        /// Global matrix of solve
        /// </summary>
        T GlobalMatrix { get; }
    }
}
