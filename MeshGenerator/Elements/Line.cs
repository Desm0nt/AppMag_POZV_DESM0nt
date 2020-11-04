using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Elements
{
    /// <summary>
    /// Line segment
    /// </summary>
    [Serializable]
    public class Line
    {
        #region Properties
        /// <summary>
        /// Nodes of a side
        /// </summary>
        public List<Node> Nodes { get; private set; } = new List<Node>();
        /// <summary>
        /// Side is related or not
        /// </summary>
        public bool IsRelated { get; set; } = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Line segment
        /// </summary>
        /// <param name="first">First node</param>
        /// <param name="second">Second node</param>
        public Line(Node first, Node second)
        {
            Nodes.Add(first);
            Nodes.Add(second);
        }

        /// <summary>
        /// Line segment
        /// </summary>
        /// <param name="nodes">Nodes of a side</param>
        public Line(List<Node> nodes)
        {
            Nodes = nodes;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sides are parallel or not
        /// </summary>
        /// <param name="item">Verified side</param>
        /// <returns>result of verification (true if parallel)</returns>
        public bool IsParallel(Line item)
        {
            double kxy1 = (Nodes[1].X - Nodes[0].X != 0) ? (Nodes[1].Y - Nodes[0].Y) / (Nodes[1].X - Nodes[0].X) : 0;
            double kxy2 = (item.Nodes[1].X - item.Nodes[0].X != 0)
                ? (item.Nodes[1].Y - item.Nodes[0].Y) / (item.Nodes[1].X - item.Nodes[0].X)
                : 0;

            double kxz1 = (Nodes[1].X - Nodes[0].X != 0) ? (Nodes[1].Z - Nodes[0].Z) / (Nodes[1].X - Nodes[0].X) : 0;
            double kxz2 = (item.Nodes[1].X - item.Nodes[0].X != 0)
                ? (item.Nodes[1].Z - item.Nodes[0].Z) / (item.Nodes[1].X - item.Nodes[0].X)
                : 0;

            double kyz1 = (Nodes[1].Y - Nodes[0].Y != 0) ? (Nodes[1].Z - Nodes[0].Z) / (Nodes[1].Y - Nodes[0].Y) : 0;
            double kyz2 = (item.Nodes[1].X - item.Nodes[0].X != 0)
                ? (item.Nodes[1].Y - item.Nodes[0].Y) / (item.Nodes[1].Y - item.Nodes[0].Y)
                : 0;

            if (kxz1 == 0 && kxz2 == 0)
            {
                return !(((Nodes[1].Z - Nodes[0].Z) == 0 && (item.Nodes[1].X - item.Nodes[0].X) == 0)
                    && ((item.Nodes[1].Z - item.Nodes[0].Z) == 0 && (Nodes[1].X - Nodes[0].X) == 0));
            }

            if (kyz1 == 0 && kyz2 == 0)
            {
                return !(((Nodes[1].Z - Nodes[0].Z) == 0 && (item.Nodes[1].Y - item.Nodes[0].Y) == 0)
                    && ((item.Nodes[1].Z - item.Nodes[0].Z) == 0 && (Nodes[1].Y - Nodes[0].Y) == 0));
            }

            if (kxy1 == 0 && kxy2 == 0)
            {
                return !(((Nodes[1].Y - Nodes[0].Y) == 0 && (item.Nodes[1].X - item.Nodes[0].X) == 0)
                    && ((item.Nodes[1].Y - item.Nodes[0].Y) == 0 && (Nodes[1].X - Nodes[0].X) == 0));
            }
            if (kxy1 == kxy2)
            {
                double bxy1 = kxy1 * Nodes[0].X - Nodes[0].Y;
                double bxy2 = kxy2 * item.Nodes[0].X - item.Nodes[0].Y;
                return (bxy1 != bxy2) ? true : false;
            }
            else if (kxz1 == kxz2)
            {
                double bxz1 = kxz1 * Nodes[0].X - Nodes[0].Z;
                double bxz2 = kxz2 * item.Nodes[0].X - item.Nodes[0].Z;
                return (bxz1 != bxz2) ? true : false;
            }
            else if (kyz1 == kyz2)
            {
                double byz1 = kyz1 * Nodes[0].Y - Nodes[0].Z;
                double byz2 = kyz2 * item.Nodes[0].Y - item.Nodes[0].Z;
                return (byz1 != byz2) ? true : false;
            }
            else return false;
        }

        /// <summary>
        /// Comparasion of sides
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Line item = obj as Line;
            if (item == null)
                return false;
            return (Nodes[0] == item.Nodes[0]) && (Nodes[1] == item.Nodes[1])
                || (Nodes[0] == item.Nodes[1]) && (Nodes[1] == item.Nodes[0]);
        }
        /// <summary>
        /// Gets hashcode of the side
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)Math.Pow(Nodes[0].GetHashCode(), 2)
                + (int)Math.Pow(Nodes[1].GetHashCode(), 2);
        }
        /// <summary>
        /// Returns information about the side in string format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Hashcode: {GetHashCode()},First: {Nodes[0]}, second: {Nodes[1]}, isRelated: {IsRelated}";
        }
        #endregion
    }
}
