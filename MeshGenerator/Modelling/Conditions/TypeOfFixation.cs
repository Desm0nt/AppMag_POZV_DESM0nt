using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Conditions
{
    /// <summary>
    /// Types of fixations
    /// </summary>
    public enum TypeOfFixation
    {
        /// <summary>
        /// Rigid fixation
        /// </summary>
        RIGID,
        /// <summary>
        /// Articulation (moving by Z)
        /// </summary>
        ARTICULATION_XY,
        /// <summary>
        /// Articulation (moving by X)
        /// </summary>
        ARTICULATION_YZ,
        /// <summary>
        /// Articulation (moving by Y)
        /// </summary>
        ARTICULATION_XZ,
    }
}
