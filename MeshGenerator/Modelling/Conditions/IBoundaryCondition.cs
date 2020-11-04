using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Conditions
{
    /// <summary>
    /// Description of boundary conditions
    /// </summary>
    public interface IBoundaryCondition
    {
        /// <summary>
        /// Fixed nodes by the boundary conditions
        /// </summary>
        Dictionary<int, Node> FixedNodes { get; }
    }
}
