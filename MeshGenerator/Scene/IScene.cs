using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Scene
{
    public interface IScene
    {
        List<Node> Nodes { get; }
        List<Tetrahedron> Tetrahedrons { get; }
        
        void Initialize();

    }
}
