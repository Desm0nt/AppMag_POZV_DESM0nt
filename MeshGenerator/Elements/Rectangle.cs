using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Elements
{
    /// <summary>
    /// Rectangular finite element
    /// </summary>
    [Serializable]
    public class Rectangle
    {
        #region Constructors
        /// <summary>
        /// Triangular finite element 
        /// </summary>
        /// <param name="first">First node</param>
        /// <param name="second">Second node</param>
        /// <param name="third">Third node</param>
        /// <param name="forth">Forth node</param>
        public Rectangle(Node first, Node second, Node third, Node forth)
        {
            Nodes.Add(first);
            Nodes.Add(second);
            Nodes.Add(third);
            Nodes.Add(forth);
            IdMaterial = GetElementMaterial();
        }

        /// <summary>
        /// Triangular finite element 
        /// </summary>
        /// <param name="points">Nodes of a triangle</param>
        public Rectangle(List<Node> points)
        {
            Nodes = points;
            IdMaterial = GetElementMaterial();
        }

        /// <summary>
        /// Triangular finite element 
        /// </summary>
        /// <param name="triangle">Triangle</param>
        public Rectangle(Rectangle rectangle)
        {
            IdMaterial = rectangle.IdMaterial;
            Nodes = rectangle.Nodes;
        }
        #endregion

        #region Properties of triangle
        /// <summary>
        /// ID Material of the element
        /// </summary>
        public int IdMaterial { get; set; }
        /// <summary>
        /// Nodes of a triangle
        /// </summary>
        public List<Node> Nodes { get; set; } = new List<Node>();
        /// <summary>
        /// Center of a triangle
        /// </summary>
        public Node Center
        {
            get
            {
                double x = Nodes.Average(px => px.X);
                double y = Nodes.Average(py => py.Y);
                double z = Nodes.Average(pz => pz.Z);
                return new Node(x, y, z, IdMaterial);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get material of all element by nodes materials
        /// </summary>
        /// <returns></returns>
        int GetElementMaterial()
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
        /// Get the nearest node to the point
        /// </summary>
        /// <param name="point">Verified point</param>
        /// <returns>The nearest node</returns>
        public Node NearNode(Node point)
        {
            Node result = null;
            int min = Int32.MaxValue;
            foreach (var item in Nodes)
            {
                int localMin = (int)(Math.Pow(item.X - point.X, 2) + Math.Pow(item.Y - point.Y, 2));
                if (min > localMin)
                {
                    min = localMin;
                    result = item;
                }
            }
            return result;
        }

        /// <summary>
        /// Get the nearest node index to the point
        /// </summary>
        /// <param name="point">Verified point</param>
        /// <returns>The nearest node index</returns>
        public int NearNodeIndex(Node point)
        {
            int result = -1;
            int min = Int32.MaxValue;
            foreach (var item in Nodes)
            {
                int localMin = (int)(Math.Pow(item.X - point.X, 2) + Math.Pow(item.Y - point.Y, 2));
                if (min > localMin)
                {
                    min = localMin;
                    result = Nodes.IndexOf(item);
                }
            }
            return result;
        }

        /// <summary>
        /// Checking in triangle verifient node or not
        /// </summary>
        /// <param name="node">Verified node</param>
        /// <returns>Result of the verification</returns>
        public bool IsInRectangle(Node node)
        {
            double minX = Nodes.Min(nd => nd.X);
            double maxX = Nodes.Max(nd => nd.X);
            double minY = Nodes.Min(nd => nd.Y);
            double maxY = Nodes.Max(nd => nd.Y);

            return (node.X >= minX && node.X <= maxX) && (node.Y >= minY && node.Y <= maxY);
        }

        /// <summary>
        /// Area of triangle
        /// </summary>
        /// <returns>Area</returns>
        public double Area()
        {
            double minX = Nodes.Min(nd => nd.X);
            double maxX = Nodes.Max(nd => nd.X);
            double minY = Nodes.Min(nd => nd.Y);
            double maxY = Nodes.Max(nd => nd.Y);

            return (maxX - minX) * (maxY - minY);
        }

        /// <summary>
        /// Gets hashcode of the triangle
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (31 * Nodes[0].GetHashCode() >> 1)
                ^ (Nodes[1].GetHashCode() >> 2)
                ^ (Nodes[2].GetHashCode() << 1)
                ^ (Nodes[3].GetHashCode() >> 2);
        }

        /// <summary>
        /// Comparasion of the triangles
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Rectangle item = obj as Rectangle;
            if (item == null)
            {
                return false;
            }

            return GetHashCode() == item.GetHashCode();
        }

        /// <summary>
        /// Returns triangle information in string format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"First: {Nodes[0]}, Second: {Nodes[1]}, Third: {Nodes[2]}, Forth: {Nodes[3]}";
        }
        #endregion
    }
}
