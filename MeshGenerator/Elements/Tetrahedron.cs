using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Utils;

namespace MeshGenerator.Elements
{
    /// <summary>
    /// Tetrahedral finite element
    /// </summary>
    [Serializable]
    public class Tetrahedron
    {
        #region Constructors
        /// <summary>
        /// Tetrahedral finite element
        /// </summary>
        /// <param name="nodes">Nodes of the tetrahedron</param>
        public Tetrahedron(List<Node> nodes)
        {
            Nodes.AddRange(nodes);
        }

        /// <summary>
        /// Tetrahedral finite element
        /// </summary>
        /// <param name="first">First node</param>
        /// <param name="second">Second node</param>
        /// <param name="third">Third node</param>
        /// <param name="forth">Forth node</param>
        public Tetrahedron(Node first, Node second, Node third, Node forth)
        {
            List<Node> nodes = new List<Node>
            {
                first,
                second,
                third,
                forth
            };
            Nodes.AddRange(nodes);
        }

        /// <summary>
        /// Tetrahedral finite element
        /// </summary>
        /// <param name="tetrahedron">Tetrahedron</param>
        public Tetrahedron(Tetrahedron tetrahedron)
        {
            Nodes.AddRange(tetrahedron.Nodes);
        }
        #endregion

        #region Properties
        /// <summary>
        /// ID Material of the element
        /// </summary>
        public int IdMaterial { get => GetElementMaterial(); }
        /// <summary>
        /// Nodes of a tetrahedron
        /// </summary>
        public List<Node> Nodes { get; } = new List<Node>();

        public Node Center
        {
            get
            {
                double xc = Nodes.Sum(x => x.X) / 4;
                double yc = Nodes.Sum(y => y.Y) / 4;
                double zc = Nodes.Sum(z => z.Z) / 4;

                return new Node(xc, yc, zc);
            }
        }

        public int NodesIdDifference
        {
            get
            {
                int max = Math.Abs(Nodes[0].GlobalIndex - Nodes[1].GlobalIndex);

                for (int i = 0; i < Nodes.Count - 1; i++)
                {
                    for (int j = i + 1; j < Nodes.Count; j++)
                    {
                        if (max < Math.Abs(Nodes[i].GlobalIndex - Nodes[j].GlobalIndex))
                        {
                            max = Math.Abs(Nodes[i].GlobalIndex - Nodes[j].GlobalIndex);
                        }
                    }
                }
                return max;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get material of all element by nodes materials
        /// </summary>
        /// <returns></returns>
        private int GetElementMaterial()
        {
            int count = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                count = 0;
                foreach (var node in Nodes)
                {
                    count += (Nodes[i].IdMaterial == node.IdMaterial) ? 1 : 0;
                }
                if (count >= 2)
                {
                    return Nodes[i].IdMaterial;
                }
            }
            return Nodes[0].IdMaterial;
        }

        /// <summary>
        /// Volume of the tetrahedron
        /// </summary>
        /// <returns>Volume</returns>
        public double Volume()
        {
            double[,] md = {
                {Nodes[1].X-Nodes[0].X, Nodes[1].Y-Nodes[0].Y, Nodes[1].Z-Nodes[0].Z},
                {Nodes[2].X-Nodes[0].X, Nodes[2].Y-Nodes[0].Y, Nodes[2].Z-Nodes[0].Z},
                {Nodes[3].X-Nodes[0].X, Nodes[3].Y-Nodes[0].Y, Nodes[3].Z-Nodes[0].Z}
                //{ 1, Nodes[0].X, Nodes[0].Y, Nodes[0].Z },
                //{ 1, Nodes[1].X, Nodes[1].Y, Nodes[1].Z },
                //{ 1, Nodes[2].X, Nodes[2].Y, Nodes[2].Z },
                //{ 1, Nodes[3].X, Nodes[3].Y, Nodes[3].Z }
            };

            double det = MathOps.Det(md);
            double Ve = Math.Abs(det) / 6.0;
            return Ve;
        }

        /// <summary>
        /// Determinant matrix of volume of the tetrahedron without abs 
        /// </summary>
        /// <returns>Volume</returns>
        public double VolumeWithSign()
        {
            double[,] md = {
                { 1, Nodes[0].X, Nodes[0].Y, Nodes[0].Z },
                { 1, Nodes[1].X, Nodes[1].Y, Nodes[1].Z },
                { 1, Nodes[2].X, Nodes[2].Y, Nodes[2].Z },
                { 1, Nodes[3].X, Nodes[3].Y, Nodes[3].Z }
            };
            double det = MathOps.Det(md);
            double Ve = det / 6.0;
            return Ve;
        }
        /// <summary>
        /// Gets hashcode of tetrahedron
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 30 * Nodes[0].GetHashCode() << 1
                ^ (30 * Nodes[1].GetHashCode())
                ^ Nodes[2].GetHashCode()
                ^ (Nodes[3].GetHashCode() >> 1);
        }

        /// <summary>
        /// Comparasion of tetrahedrons
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Tetrahedron item = obj as Tetrahedron;
            if (item == null)
            {
                return false;
            }

            return GetHashCode() == item.GetHashCode();
        }

        public bool IsInTetrahedron(Node point)
        {
            bool result = false;
            double det = 0;
            double bx = Nodes[1].X - Nodes[0].X;
            double by = Nodes[1].Y - Nodes[0].Y;
            double bz = Nodes[1].Z - Nodes[0].Z;
            double cx = Nodes[2].X - Nodes[0].X;
            double cy = Nodes[2].Y - Nodes[0].Y;
            double cz = Nodes[2].Z - Nodes[0].Z;
            double dx = Nodes[3].X - Nodes[0].X;
            double dy = Nodes[3].Y - Nodes[0].Y;
            double dz = Nodes[3].Z - Nodes[0].Z;
            double px = point.X - Nodes[0].X;
            double py = point.Y - Nodes[0].Y;
            double pz = point.Z - Nodes[0].Z;
            double cdx = cy * dz - cz * dy;
            double cdy = cz * dx - cx * dz;
            double cdz = cx * dy - cy * dx;
            det = bx * cdx + by * cdy + bz * cdz;
            if (det != 0)
            {
                double b = px * cdx + py * cdy + pz * cdz;
                double c = px * (dy * bz - dz * by) + py * (dz * bx - dx * bz) + pz * (dx * by - dy * bx);
                double d = px * (by * cz - bz * cy) + py * (bz * cx - bx * cz) + pz * (bx * cy - by * cx);
                if (det > 0)
                {
                    result = (b >= 0) && (c >= 0) && (d >= 0) && (b + c + d <= det);
                }
                else
                {
                    result = (b <= 0) && (c <= 0) && (d <= 0) && (b + c + d >= det);
                }
            }

            return result;
        }

        /// <summary>
        /// Conrerts tetrahedron to string format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"First: {Nodes[0]}, Second: {Nodes[1]}, Third: {Nodes[2]}, Forth: {Nodes[3]}";
        }
        #endregion
    }
}
