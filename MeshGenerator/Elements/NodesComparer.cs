
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Elements
{
    /// <summary>
    /// Comapares nodes for sorting
    /// </summary>
    public class NodesComparer : IComparer<Node>
    {
        /// <summary>
        /// Compare two nodes
        /// </summary>
        /// <param name="x">First node</param>
        /// <param name="y">Second node</param>
        /// <returns>Result of comparasion</returns>
        public int Compare(Node x, Node y)
        {
            if (x.Y > y.Y)
            {
                return 1;
            }
            else
            {
                if (x.Y < y.Y)
                {
                    return -1;

                }
                else
                {
                    if (x.X > y.X)
                    {
                        return 1;
                    }
                    else
                    {
                        if (x.X < y.X)
                        {
                            return -1;
                        }
                        else
                            return 0;
                    }
                }
            }
        }
    }
}
