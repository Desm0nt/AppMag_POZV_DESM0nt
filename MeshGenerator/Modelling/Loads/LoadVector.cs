using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Loads
{
    /// <summary>
    /// Load vector
    /// </summary>
    public class LoadVector
    {
        /// <summary>
        /// Load vector
        /// </summary>
        /// <param name="value">Value of the load</param>
        /// <param name="direction">Direction by (X,Y or Z)</param>
        public LoadVector(double value, VectorDirection direction)
        {
            Value = value;
            Direction = direction;
        }
        /// <summary>
        /// Value of load
        /// </summary>
        public double Value { get; private set; }
        /// <summary>
        /// Direction of the load vector
        /// </summary>
        public VectorDirection Direction { get; private set; }
    }
}
