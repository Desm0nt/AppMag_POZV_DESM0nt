using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Planes
{
    public interface IPlane
    {
        double Area();
        Node GetNearestNode(Node node);
        Node GetProjection(Node node);
        int IdMaterial { get; set; }
        bool IsInside(Node node);
    }
}
