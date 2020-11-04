using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Elements
{
    /// <summary>
    /// Triangular finite element
    /// </summary>
    [Serializable]
    public class Triangle
    {
        #region Constructors
        /// <summary>
        /// Triangular finite element 
        /// </summary>
        /// <param name="first">First node</param>
        /// <param name="second">Second node</param>
        /// <param name="third">Third node</param>
        public Triangle(Node first, Node second, Node third)
        {
            Nodes.Add(first);
            Nodes.Add(second);
            Nodes.Add(third);
            //Nodes.Sort(new NodesComparer());

            //if (Nodes[1].X < Nodes[2].X)
            //{
            //    Node tmp = Nodes[1];
            //    Nodes[1] = Nodes[2];
            //    Nodes[2] = tmp;
            //}
            IdMaterial = GetElementMaterial();
        }

        /// <summary>
        /// Triangular finite element 
        /// </summary>
        /// <param name="points">Nodes of a triangle</param>
        public Triangle(List<Node> points)
        {
            Nodes = points;
            //Nodes.Sort(new NodesComparer());
            //if (Nodes[1].Y == Nodes[2].Y)
            //{
            //    Node tmp = Nodes[1];
            //    Nodes[1] = Nodes[2];
            //    Nodes[2] = tmp;
            //}
            IdMaterial = GetElementMaterial();
        }

        ///// <summary>
        ///// Triangular finite element 
        ///// </summary>
        ///// <param name="points">Nodes of a triangle</param>
        //public Triangle(List<Node> points)
        //{
        //    IdMaterial = GetElementMaterial();
        //    Nodes = points;
        //}

        /// <summary>
        /// Triangular finite element 
        /// </summary>
        /// <param name="triangle">Triangle</param>
        public Triangle(Triangle triangle)
        {
            IdMaterial = triangle.IdMaterial;
            Nodes = triangle.Nodes;
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
        public bool IsInTriangle(Node node)
        {
            double n1 = (Nodes[1].Y - Nodes[0].Y) * (node.X - Nodes[0].X) - (Nodes[1].X - Nodes[0].X) * (node.Y - Nodes[0].Y);
            double n2 = (Nodes[2].Y - Nodes[1].Y) * (node.X - Nodes[1].X) - (Nodes[2].X - Nodes[1].X) * (node.Y - Nodes[1].Y);
            double n3 = (Nodes[0].Y - Nodes[2].Y) * (node.X - Nodes[2].X) - (Nodes[0].X - Nodes[2].X) * (node.Y - Nodes[2].Y);

            return (n1 > 0 && n2 > 0 && n3 > 0) || (n1 < 0 && n2 < 0 && n3 < 0); // only inside triangle. on side result false
        }

        public bool IsInTriangleXY(Node node)
        {
            double n1 = (Nodes[1].Y - Nodes[0].Y) * (node.X - Nodes[0].X) - (Nodes[1].X - Nodes[0].X) * (node.Y - Nodes[0].Y);
            double n2 = (Nodes[2].Y - Nodes[1].Y) * (node.X - Nodes[1].X) - (Nodes[2].X - Nodes[1].X) * (node.Y - Nodes[1].Y);
            double n3 = (Nodes[0].Y - Nodes[2].Y) * (node.X - Nodes[2].X) - (Nodes[0].X - Nodes[2].X) * (node.Y - Nodes[2].Y);

            return (n1 > 0 && n2 > 0 && n3 > 0) || (n1 < 0 && n2 < 0 && n3 < 0); // only inside triangle. on side result false
        }

        public bool IsInTriangleXZ(Node node)
        {
            double n1 = (Nodes[1].Z - Nodes[0].Z) * (node.X - Nodes[0].X) - (Nodes[1].X - Nodes[0].X) * (node.Z - Nodes[0].Z);
            double n2 = (Nodes[2].Z - Nodes[1].Z) * (node.X - Nodes[1].X) - (Nodes[2].X - Nodes[1].X) * (node.Z - Nodes[1].Z);
            double n3 = (Nodes[0].Z - Nodes[2].Z) * (node.X - Nodes[2].X) - (Nodes[0].X - Nodes[2].X) * (node.Z - Nodes[2].Z);

            return (n1 > 0 && n2 > 0 && n3 > 0) || (n1 < 0 && n2 < 0 && n3 < 0); // only inside triangle. on side result false
        }

        public bool IsInTriangleYZ(Node node)
        {
            double n1 = (Nodes[1].Y - Nodes[0].Y) * (node.Z - Nodes[0].Z) - (Nodes[1].Z - Nodes[0].Z) * (node.Y - Nodes[0].Y);
            double n2 = (Nodes[2].Y - Nodes[1].Y) * (node.Z - Nodes[1].Z) - (Nodes[2].Z - Nodes[1].Z) * (node.Y - Nodes[1].Y);
            double n3 = (Nodes[0].Y - Nodes[2].Y) * (node.Z - Nodes[2].Z) - (Nodes[0].Z - Nodes[2].Z) * (node.Y - Nodes[2].Y);

            return (n1 > 0 && n2 > 0 && n3 > 0) || (n1 < 0 && n2 < 0 && n3 < 0); // only inside triangle. on side result false
        }

        /// <summary>
        /// Area of triangle
        /// </summary>
        /// <returns>Area</returns>
        public double Area()
        {
            double ab = Math.Sqrt(Math.Pow(Nodes[0].X - Nodes[1].X, 2) + Math.Pow(Nodes[0].Y - Nodes[1].Y, 2) + Math.Pow(Nodes[0].Z - Nodes[1].Z, 2));
            double bc = Math.Sqrt(Math.Pow(Nodes[1].X - Nodes[2].X, 2) + Math.Pow(Nodes[1].Y - Nodes[2].Y, 2) + Math.Pow(Nodes[1].Z - Nodes[2].Z, 2));
            double ac = Math.Sqrt(Math.Pow(Nodes[0].X - Nodes[2].X, 2) + Math.Pow(Nodes[0].Y - Nodes[2].Y, 2) + Math.Pow(Nodes[0].Z - Nodes[2].Z, 2));
            double p = (ab + bc + ac) / 2;
            return Math.Sqrt((p - ab) * (p - bc) * (p - ac) * p);
            //return (1.0 / 2.0) * ((Nodes[0].X - Nodes[2].X) * (Nodes[1].Y - Nodes[2].Y) - (Nodes[1].X - Nodes[2].X) * (Nodes[0].Y - Nodes[2].Y));
        }

        /// <summary>
        /// Gets hashcode of the triangle
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (31 * Nodes[0].GetHashCode() >> 1)
                ^ (Nodes[1].GetHashCode() >> 2)
                ^ (Nodes[2].GetHashCode() << 1);
        }

        /// <summary>
        /// Comparasion of the triangles
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Triangle item = obj as Triangle;
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
            return $"First: {Nodes[0]}, Second: {Nodes[1]}, Third: {Nodes[2]}";
        }
        #endregion
    }
}
