using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Loads
{
    /// <summary>
    /// Description of load
    /// </summary>
    public interface ILoad
    {
        /// <summary>
        /// Value of Load
        /// </summary>
        double Value { get; }
        /// <summary>
        /// Loaded nodes
        /// </summary>
        Dictionary<int, LoadVector> LoadVectors { get; }
    }
}
