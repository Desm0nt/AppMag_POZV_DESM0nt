using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshGenerator.Elements;
using System.Threading;
using MeshGenerator.Materials;

namespace MeshGenerator.Scene
{
    public class TetrahedralScene : IScene
    {
        List<Node> nodes = new List<Node>();
        List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();
        double width;
        double height;
        double stepWidth, stepHeight;

        #region Constructors
        public TetrahedralScene(double width, double height, double step = 5)
        {
            this.stepWidth = step;
            this.stepHeight = step;
            this.width = width;
            this.height = height;

            this.width = (this.width <= 0) ? stepWidth : this.width;
            this.height = (this.height <= 0) ? stepWidth : this.height;
        }
        public TetrahedralScene(double width, double height, double stepWidth, double stepHeight)
        {
            this.stepWidth = stepWidth;
            this.stepHeight = stepHeight;
            this.width = width;
            this.height = height;

            this.width = (this.width <= 0) ? stepWidth : this.width;
            this.height = (this.height <= 0) ? stepWidth : this.height;
        }
        #endregion

        #region Properties
        public List<Node> Nodes { get => nodes; }
        public List<Tetrahedron> Tetrahedrons { get => tetrahedrons; }
        #endregion

        #region Methods
        public void Initialize()
        {
            List<List<Triangle>> trianglesLayers = new List<List<Triangle>>();
            List<List<Rectangle>> rectanglesLayers = new List<List<Rectangle>>();

            double stepsHeight = 0;
            while (stepsHeight <= height + 0.001)
            {
                List<Node> currentLayer = GenerateNodesLayer(stepsHeight);
                int startIndex = Nodes.Count;
                int endIndex = startIndex + currentLayer.Count;

                Nodes.AddRange(currentLayer);
                for (int i = startIndex, j = 0; i < endIndex; i++, j++)
                {
                    Nodes[i].GlobalIndex = i;
                    currentLayer[j].GlobalIndex = i;
                }
                //trianglesLayers.Add(GenerateTrianglesLayer(currentLayer));
                rectanglesLayers.Add(GenerateRectanglesLayer(currentLayer));

                stepsHeight += stepHeight;
            }
            //GenerateTetrahedrons(trianglesLayers);
            GenerateTetrahedrons(rectanglesLayers);
        }

        public List<Node> GenerateNodesLayer(double z)
        {
            List<Node> list = new List<Node>();
            bool isBound = false;

            for (double i = 0; i <= width; i += stepWidth)
            {
                for (double j = 0; j <= width; j += stepWidth)
                {
                    isBound = (i == 0 || i == width || j == 0 || j == width || z == 0 || z == height) ? true : false;
                    list.Add(new Node(j, i, z, MaterialStorage.Materials[0].Id, 0, isBound));
                }
            }

            return list;
        }

        public List<Triangle> GenerateTrianglesLayer(List<Node> layer)
        {
            List<Triangle> trnList = new List<Triangle>();

            foreach (Node node in layer)
            {
                Node right = layer.FirstOrDefault(n => n.X > node.X && n.Y == node.Y);
                Node bottom = layer.FirstOrDefault(n => n.X == node.X && n.Y > node.Y);
                Node bottomRight = (bottom != null)
                    ? layer.FirstOrDefault(n => n.X > bottom.X && n.Y == bottom.Y)
                    : null;

                if (right != null && bottom != null && bottomRight != null)
                {
                    trnList.Add(new Triangle(node, right, bottom));
                    trnList.Add(new Triangle(right, bottomRight, bottom));
                }
            }

            return trnList;
        }

        public List<Rectangle> GenerateRectanglesLayer(List<Node> layer)
        {
            List<Rectangle> rectList = new List<Rectangle>();

            foreach (Node node in layer)
            {
                Node right = layer.FirstOrDefault(n => n.X > node.X && n.Y == node.Y);
                Node bottom = layer.FirstOrDefault(n => n.X == node.X && n.Y > node.Y);
                Node bottomRight = (bottom != null)
                    ? layer.FirstOrDefault(n => n.X > bottom.X && n.Y == bottom.Y)
                    : null;

                if (right != null && bottom != null && bottomRight != null)
                {
                    rectList.Add(new Rectangle(node, right, bottomRight, bottom));
                }
            }

            return rectList;
        }

        void GenerateTetrahedrons(List<List<Rectangle>> rectanglesLayers)
        {
            int dx = (int)Math.Floor(width / stepWidth);
            bool isEven = true;
            bool isEvenZ = true;
            bool isEvenY = true;
            for (int i = 0; i < rectanglesLayers.Count - 1; i++)
            {
                isEven = isEvenZ;
                isEvenY = isEvenZ;
                for (int j = 0; j < rectanglesLayers[i].Count; j++)
                {
                    if (j % dx == 0 && j != 0)
                    {
                        isEvenY = !isEvenY;
                        isEven = isEvenY;
                    }
                    Tetrahedrons.AddRange(TetrahedronsFromParallelepiped(rectanglesLayers[i][j], rectanglesLayers[i + 1][j], true/*isEven*/));

                    isEven = !isEven;
                }
                isEvenZ = !isEvenZ;
            }
        }

        private IEnumerable<Tetrahedron> TetrahedronsFromParallelepiped(Rectangle first, Rectangle second, bool isEven)
        {
            IList<Tetrahedron> result = new List<Tetrahedron>();

            if (isEven)
            {
                //1
                result.Add(new Tetrahedron(second.Nodes[0], second.Nodes[1], second.Nodes[3], first.Nodes[0]));
                result.Add(new Tetrahedron(second.Nodes[1], first.Nodes[1], first.Nodes[3], first.Nodes[0]));
                result.Add(new Tetrahedron(second.Nodes[1], second.Nodes[3], first.Nodes[0], first.Nodes[3]));
                //2
                result.Add(new Tetrahedron(second.Nodes[1], second.Nodes[2], second.Nodes[3], first.Nodes[1]));
                result.Add(new Tetrahedron(second.Nodes[2], first.Nodes[2], first.Nodes[3], first.Nodes[1]));
                result.Add(new Tetrahedron(second.Nodes[2], second.Nodes[3], first.Nodes[1], first.Nodes[3]));
            }
            else
            {
                //3
                result.Add(new Tetrahedron(second.Nodes[0], second.Nodes[3], second.Nodes[1], first.Nodes[0]));
                result.Add(new Tetrahedron(second.Nodes[1], first.Nodes[3], first.Nodes[1], first.Nodes[0]));
                result.Add(new Tetrahedron(second.Nodes[1], first.Nodes[0], second.Nodes[3], first.Nodes[3]));
                //4
                result.Add(new Tetrahedron(second.Nodes[1], second.Nodes[3], second.Nodes[2], first.Nodes[1]));
                result.Add(new Tetrahedron(second.Nodes[2], first.Nodes[3], first.Nodes[2], first.Nodes[1]));
                result.Add(new Tetrahedron(second.Nodes[2], first.Nodes[1], second.Nodes[3], first.Nodes[3]));
            }
            return result;
        }

        void GenerateTetrahedrons(List<List<Triangle>> trianglesLayers)
        {
            for (int i = 0; i < trianglesLayers.Count - 1; i++)
            {
                for (int j = 0; j < trianglesLayers[i].Count; j++)
                {
                    Tetrahedrons.AddRange(TetrahedronsFromPrism(trianglesLayers[i][j], trianglesLayers[i + 1][j]));
                }
            }
        }

        /// <summary>
        /// Generate tetrahedrons from prism
        /// </summary>
        /// <param name="first">Top triangle of prism</param>
        /// <param name="second">Bottom triangle of prism</param>
        /// <returns>Tetrahedrons</returns>
        List<Tetrahedron> TetrahedronsFromPrism(Triangle first, Triangle second)
        {
            List<Tetrahedron> result = new List<Tetrahedron>
            {
                new Tetrahedron(second.Nodes[0], second.Nodes[1], second.Nodes[2], first.Nodes[0]),
                new Tetrahedron(second.Nodes[1], first.Nodes[1], first.Nodes[2], first.Nodes[0]),
                new Tetrahedron(second.Nodes[1], second.Nodes[2], first.Nodes[0], first.Nodes[2])
            };
            return result;
        }
        #endregion
    }
}
