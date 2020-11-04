using MeshGenerator.Elements;
using MeshGenerator.Planes;
using MeshGenerator.Scene;
using MeshGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Model
{
    public class FeModel
    {
        Guid id;
        IScene scene;
        List<IPlane> topPlanes;
        List<IPlane> bottomPlanes;

        #region Properties
        public Guid Id { get => id; }
        public List<Node> Nodes { get; private set; }
        public List<Tetrahedron> Tetrahedrons { get; private set; } = new List<Tetrahedron>();
        public List<Triangle> Triangles { get; private set; } = new List<Triangle>();
        #endregion

        #region Constructors
        public FeModel(IScene scene, List<IPlane> topPlanes, List<IPlane> bottomPlanes)
        {
            id = Guid.NewGuid();
            this.scene = scene;
            Nodes = scene.Nodes;
            this.topPlanes = topPlanes;
            this.bottomPlanes = bottomPlanes;

            ModelGeneration();
            foreach (Tetrahedron item in Tetrahedrons)
            {
                item.Nodes.ForEach(n => n.GlobalIndex = Nodes.IndexOf(n));
            }
            SortAllTetrahedrons();
        }

        public FeModel(List<Node> nodes, List<Tetrahedron> tetrahedrons)
        {
            id = Guid.NewGuid();
            Nodes = nodes;
            Tetrahedrons = tetrahedrons;
            RemoveNotTetrahedrons();
            InitOuterTriangles();
            SortAllTetrahedrons();
        }

        public FeModel(List<Node> nodes, List<Triangle> triangles, List<Tetrahedron> tetrahedrons)
        {
            id = Guid.NewGuid();
            Nodes = nodes;
            Tetrahedrons = tetrahedrons;
            Triangles = triangles;
            RemoveNotTetrahedrons();
            SortAllTetrahedrons();
        }
        #endregion

        #region Methods
        void InitOuterTriangles()
        {
            foreach (var tetrahedron in Tetrahedrons)
            {
                List<Node> nodes = tetrahedron.Nodes.Where(node => node.IsBound).ToList();
                if (nodes.Count == 3)
                {
                    Triangles.Add(new Triangle(nodes));
                }
            }
        }

        private bool IsPointOnLines(Line first, Line second)
        {
            int firstDifferenceX = Math.Abs(first.Nodes[0].PX - first.Nodes[1].PX);
            int secondDifferenceX = Math.Abs(second.Nodes[0].PX - second.Nodes[1].PX);
            int firstDifferenceY = Math.Abs(first.Nodes[0].PY - first.Nodes[1].PY);
            int secondDifferenceY = Math.Abs(second.Nodes[0].PY - second.Nodes[1].PY);
            if ((firstDifferenceX == 0 && secondDifferenceX == 0) || (firstDifferenceY == 0 && secondDifferenceY == 0))
            {
                return true;
            }
            return false;
        }

        //private bool IsTetrahedron(Tetrahedron tetrahedron)
        //{
        //    for (int i = 0; i < tetrahedron.Nodes.Count - 1; i++)
        //    {
        //        for (int j = i + 1; j < tetrahedron.Nodes.Count; j++)
        //        {
        //            if (tetrahedron.Nodes[i].Equals(tetrahedron.Nodes[j]))
        //            {
        //                return false;
        //            }
        //        }
        //    }

        //    List<Line> lines = new List<Line>()
        //    {
        //        new Line(tetrahedron.Nodes[0], tetrahedron.Nodes[1]),
        //        new Line(tetrahedron.Nodes[0], tetrahedron.Nodes[2]),
        //        new Line(tetrahedron.Nodes[0], tetrahedron.Nodes[3]),
        //        new Line(tetrahedron.Nodes[1], tetrahedron.Nodes[2]),
        //        new Line(tetrahedron.Nodes[1], tetrahedron.Nodes[3]),
        //        new Line(tetrahedron.Nodes[2], tetrahedron.Nodes[3])
        //    };
        //    for (int i = 0; i < lines.Count; i++)
        //    {
        //        for (int j = 0; j < lines.Count; j++)
        //        {
        //            if (i != j)
        //            {
        //                if (/*lines[i].IsParallel(lines[j]) ||*/ Math.Abs(tetrahedron.Volume()) < 0.000000000001)
        //                    return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        private void ModelGeneration()
        {
            for (int i = 0; i < topPlanes.Count; i++)
            {
                foreach (Tetrahedron tetrahedron in scene.Tetrahedrons)
                {
                    Node projectTop = topPlanes[i].GetProjection(tetrahedron.Center);
                    Node projectBottom = bottomPlanes[i].GetProjection(tetrahedron.Center);
                    if (projectTop != null && projectBottom != null)
                    {
                        if (tetrahedron.Center.Z <= projectTop.Z && tetrahedron.Center.Z >= projectBottom.Z)
                        {
                            Tetrahedrons.Add(tetrahedron);
                        }

                    }
                }
            }
        }

        private void RemoveNotTetrahedrons()
        {
            for (int i = 0; i < Tetrahedrons.Count; i++)
            {
                if (/*!IsTetrahedron(Tetrahedrons[i])*/Tetrahedrons[i].Volume() < 0.0000001)
                {
                    Tetrahedrons.RemoveAt(i);
                    i--;
                }
            }
        }

        private void SortAllTetrahedrons()
        {
            Parallel.ForEach(Tetrahedrons, tetrahedron =>
            {
                SortNodes(ref tetrahedron);
            });
        }

        private void SortNodes(ref Tetrahedron tetrahedron)
        {
            Tuple<double, double, double, double> coefficients = new Tuple<double, double, double, double>(0, 0, 0, 0);
            Tuple<double, double, double> coefLine = new Tuple<double, double, double>(0, 0, 0);
            int nodesPlaneIndex = 0;
            int forthIndex = 0;
            int nodesLineIndex = 0;
            int relativeLineIndex = 0;

            do
            {
                coefficients = GetPlaneCoefficients(tetrahedron.Nodes[nodesPlaneIndex % Nodes.Count],
                    tetrahedron.Nodes[(nodesPlaneIndex + 1) % Nodes.Count],
                    tetrahedron.Nodes[(nodesPlaneIndex + 2) % Nodes.Count]);

                forthIndex = (nodesPlaneIndex + 3) % Nodes.Count;

                nodesPlaneIndex++;
            } while (coefficients.Item1 == 0 && coefficients.Item2 == 0 && coefficients.Item3 == 0);

            int isAbovePlane = PositionRelativePlane(coefficients.Item1, coefficients.Item2, coefficients.Item3, coefficients.Item4, tetrahedron.Nodes[forthIndex]);

            if (isAbovePlane > 0)
            {
                if (forthIndex != 0)
                {
                    SwapNodes(ref tetrahedron, 0, forthIndex);
                    nodesLineIndex = 1;
                }
            }
            else if (isAbovePlane < 0)
            {
                if (forthIndex != tetrahedron.Nodes.Count - 1)
                {
                    SwapNodes(ref tetrahedron, tetrahedron.Nodes.Count - 1, forthIndex);
                }
            }

            do
            {
                coefLine = GetLineCoefficients(tetrahedron.Nodes[nodesLineIndex % Nodes.Count],
                    tetrahedron.Nodes[(nodesLineIndex + 1) % Nodes.Count]);

                relativeLineIndex = (nodesLineIndex + 2) % Nodes.Count;

                nodesLineIndex++;
            } while (coefficients.Item1 == 0 && coefficients.Item2 == 0 && coefficients.Item3 == 0);

            relativeLineIndex = (relativeLineIndex == 0) ? relativeLineIndex + nodesLineIndex : relativeLineIndex;

            nodesLineIndex--;

            int isAboveLine = PositionRelativeLine(coefLine.Item1, coefLine.Item2, coefLine.Item3, tetrahedron.Nodes[relativeLineIndex]);
            if (isAboveLine > 0)
            {
                if (nodesLineIndex != relativeLineIndex)
                {
                    SwapNodes(ref tetrahedron, nodesLineIndex, relativeLineIndex);
                }
                if (tetrahedron.Nodes[nodesLineIndex + 1].X < tetrahedron.Nodes[nodesLineIndex + 2].X)
                {
                    SwapNodes(ref tetrahedron, nodesLineIndex + 1, nodesLineIndex + 2);
                }
            }
            else if (isAboveLine < 0)
            {
                if (nodesLineIndex != relativeLineIndex)
                {
                    SwapNodes(ref tetrahedron, relativeLineIndex, nodesLineIndex + 1);

                }
                if (tetrahedron.Nodes[nodesLineIndex].X > tetrahedron.Nodes[nodesLineIndex + 1].X)
                {
                    SwapNodes(ref tetrahedron, nodesLineIndex + 1, nodesLineIndex + 2);
                }
            }
        }

        /// <summary>
        /// Gets position relative point to the line. Geater zero is above line. Less zero is under line. Equal zero is on line.
        /// </summary>
        /// <param name="a">Goefficient A of the line equation</param>
        /// <param name="b">Goefficient B of the line equation</param>
        /// <param name="c">Goefficient C of the line equation</param>
        /// <param name="point">Relative point</param>
        /// <returns>position relative line</returns>
        private int PositionRelativeLine(double a, double b, double c, Node point)
        {
            int result = (int)(a * point.X + b * point.Y + c);
            return result;
        }

        /// <summary>
        /// Gets position relative point to the plane. Geater zero is above plane. Less zero is under plane. Equal zero is on plane.
        /// </summary>
        /// <param name="a">Goefficient A of the plane equation</param>
        /// <param name="b">Goefficient B of the plane equation</param>
        /// <param name="c">Goefficient C of the plane equation</param>
        /// <param name="d">Goefficient D of the plane equation</param>
        /// <param name="point">Relative point</param>
        /// <returns>position relative plane</returns>
        private int PositionRelativePlane(double a, double b, double c, double d, Node point)
        {
            int result = (int)(a * point.X + b * point.Y + c * point.Z + d);
            return result;
        }

        private Tuple<double, double, double, double> GetPlaneCoefficients(Node first, Node second, Node third)
        {
            double a = MathOps.Det(new double[,]
            {
                { second.Y - first.Y, second.Z - first.Z },
                { third.Y - first.Y, third.Z - first.Z}
            });
            a *= (first.X * (-1) < 0) ? (-1) : 1;
            double b = MathOps.Det(new double[,]
            {
                { second.X - first.X, second.Z - first.Z},
                { third.X - first.X, third.Z - first.Z}
            });
            b *= (first.Y * (-1) < 0) ? (-1) : 1;
            double c = MathOps.Det(new double[,]
            {
                { second.X - first.X, second.Y - first.Y},
                { third.X - first.X, third.Y - first.Y}
            });
            c *= (first.Z * (-1) < 0) ? (-1) : 1;
            double d = (-1) * (a * first.X + b * first.Y + c * first.Z);


            return new Tuple<double, double, double, double>(a, b, c, d);
        }

        private Tuple<double, double, double> GetLineCoefficients(Node first, Node second)
        {
            double a = first.Y - second.Y;
            double b = second.X - first.X;
            double c = first.X * second.Y - second.X * first.Y;

            return new Tuple<double, double, double>(a, b, c);
        }

        private void SwapNodes(ref Tetrahedron tetrahedron, int firstIndex, int secondIndex)
        {
            Node tmp = new Node(tetrahedron.Nodes[firstIndex]);
            tetrahedron.Nodes[firstIndex] = new Node(tetrahedron.Nodes[secondIndex]);
            tetrahedron.Nodes[secondIndex] = tmp;
        }
        #endregion
    }
}
