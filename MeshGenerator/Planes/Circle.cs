using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Elements;

namespace MeshGenerator.Planes
{
    public class Circle : IPlane
    {
        double radius;
        Node offset;

        public Circle(double radius, Node offset, int idMaterial)
        {
            this.radius = radius;
            this.offset = offset;
            IdMaterial = idMaterial;
        }

        public int IdMaterial { get; set; }

        public double Area()
        {
            return Math.PI * Math.Pow(radius, 2);
        }

        public Node GetNearestNode(Node node)
        {
            throw new NotImplementedException();
        }

        //public double GetPlaneFunction(double x)
        //{
        //    return Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(x - offset.X, 2)) + offset.Y;
        //}

        public Node GetProjection(Node node)
        {
            if (IsInside(node))
                return new Node(node.X, node.Y, offset.Z);
            else
                return null;
        }

        public bool IsInside(Node node)
        {
            if (Math.Pow(node.X - offset.X, 2) + Math.Pow(node.Y - offset.Y, 2) <= radius * radius)
                return true;
            else
                return false;
        }
    }
}
