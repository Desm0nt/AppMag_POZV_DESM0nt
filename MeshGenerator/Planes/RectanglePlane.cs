using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Elements;

namespace MeshGenerator.Planes
{
    public class RectanglePlane: IPlane
    {
        double width;
        double height;
        Node offset;

        public RectanglePlane(double width, double height, Node offset, int idMaterial)
        {
            this.width = width;
            this.height = height;
            this.offset = offset;
            IdMaterial = idMaterial;
        }

        public int IdMaterial { get; set; }

        public double Area()
        {
            return width * height;
        }

        public Node GetNearestNode(Node node)
        {
            throw new NotImplementedException();
        }

        public Node GetProjection(Node node)
        {
            if (IsInside(node))
                return new Node(node.X, node.Y, offset.Z);
            else
                return null;
        }

        public bool IsInside(Node node)
        {
            if ((node.X >= offset.X && node.X <= offset.X + width) && (node.Y >= offset.Y && node.Y <= offset.Y + height))
                return true;
            else
                return false;
        }
    }
}
