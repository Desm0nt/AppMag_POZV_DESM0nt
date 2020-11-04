using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Volume
{
    public class VolumeGeneration
    {
        private int step;

        #region Constructors
        /// <summary>
        /// Generation of volumetric finite elements
        /// </summary>
        /// <param name="step">Step of the mesh</param>
        public VolumeGeneration(int step = 10)
        {
            this.step = step;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generation of tetrahedrons from two layers, which consists from triangles.
        /// First layer must have less point or triangles than second!
        /// </summary>
        /// <param name="firstLayer">triangles from first layer</param>
        /// <param name="secondLayer">triangles from second layer</param>
        /// <returns>tetrahedrons</returns>
        public List<Tetrahedron> GenerateTetrahedrons(List<List<Triangle>> firstLayer, List<List<Triangle>> secondLayer)
        {
            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();
            HashSet<Tetrahedron> result = new HashSet<Tetrahedron>();
            List<Tetrahedron> smooth = new List<Tetrahedron>();

            List<List<Triangle>> relatedTriangles = new List<List<Triangle>>();
            List<List<Triangle>> firstRelatedTriangles = new List<List<Triangle>>();
            List<List<Triangle>> notRelated = new List<List<Triangle>>();
            List<List<Triangle>> firstNotRelated = new List<List<Triangle>>();

            notRelated = new List<List<Triangle>>(secondLayer);
            firstNotRelated = new List<List<Triangle>>(SetNotRelated(firstLayer));

            foreach (List<Triangle> triangles in firstLayer)
            {
                List<Triangle> relatedTmp = new List<Triangle>();
                List<Triangle> firstRelatedTmp = new List<Triangle>();
                foreach (Triangle item in triangles)
                {
                    foreach (List<Triangle> second in secondLayer)
                    {
                        double range = step * 2 / 3;
                        Triangle otherTriangle = null;
                        foreach (var tr in second)
                        {
                            if (tr.Center.PX >= item.Center.PX - range && tr.Center.PX <= item.Center.PX + range && tr.Center.PY >= item.Center.PY - range && tr.Center.PY <= item.Center.PY + range)
                            {
                                otherTriangle = tr;
                                break;
                            }
                        }

                        if (otherTriangle != null)
                        {
                            tetrahedrons.AddRange(TetrahedronsFromPrism(item, otherTriangle));
                            relatedTmp.Add(otherTriangle);
                            firstRelatedTmp.Add(item);
                        }
                    }
                }
                relatedTriangles.Add(relatedTmp);
                firstRelatedTriangles.Add(firstRelatedTmp);
            }
            List<List<Triangle>> boundTriangles = new List<List<Triangle>>();
            boundTriangles.AddRange(SetBoundTriangles(firstLayer));
            List<List<Triangle>> secondBoundTriangles = new List<List<Triangle>>(SetBoundTriangles(secondLayer));
            for (int i = 0; i < relatedTriangles.Count; i++)
            {
                relatedTriangles[i].ForEach(it => notRelated[i].Remove(it));
            }
            for (int i = 0; i < firstRelatedTriangles.Count; i++)
            {
                firstRelatedTriangles[i].ForEach(it => firstNotRelated[i].Remove(it));
            }
            tetrahedrons.AddRange(Smoothing(boundTriangles, notRelated));
            tetrahedrons.AddRange(Smoothing(secondBoundTriangles, firstNotRelated));
            tetrahedrons.ToList().ForEach(item =>
            {
                result.Add(item);
            });
            return result.ToList();
        }


        /// <summary>
        /// Generation of tetrahedrons from two layers, one from them consists from triangles.
        /// First layer have only one point
        /// </summary>
        /// <param name="point">point from first layer</param>
        /// <param name="second">triangles from second layer</param>
        /// <returns>tetrahedrons</returns>
        public List<Tetrahedron> GenerateTetrahedrons(Node point, List<Triangle> second)
        {
            List<Tetrahedron> result = new List<Tetrahedron>();
            foreach (var item in second)
            {
                if (Distance(item.Center, point) < step)
                    result.Add(new Tetrahedron(point, item.Nodes[0], item.Nodes[1], item.Nodes[2]));
            }
            return result;
        }

        /// <summary>
        /// Smooting volume
        /// </summary>
        /// <param name="firstLayer">Base layer</param>
        /// <param name="smooth">Smothing layer</param>
        /// <returns>Tetrahedrons for smoothing</returns>
        List<Tetrahedron> Smoothing(List<List<Triangle>> firstLayer, List<List<Triangle>> smooth)
        {
            smooth.RemoveAll(x => x.Count == 0);
            List<Tetrahedron> result = new List<Tetrahedron>();
            foreach (var triangles in firstLayer)
            {
                foreach (var triangle in triangles)
                {
                    List<List<Triangle>> trianglesForSmoothByNodes = new List<List<Triangle>>();
                    List<Triangle> trianglesForSmooth = new List<Triangle>();
                    HashSet<Triangle> smoothingTriangles = new HashSet<Triangle>();
                    List<Node> boundNodes = new List<Node>(GetBoundNodes(triangle));
                    foreach (var smoothItem in smooth)
                    {
                        List<Node> nodes = new List<Node>();
                        foreach (var node in boundNodes)
                        {
                            var query = smoothItem
                                .Where(tr => tr.Center.PX >= node.PX - step && tr.Center.PX <= node.PX + step && tr.Center.PY >= node.PY - step && tr.Center.PY <= node.PY + step)
                                .ToList();
                            if (query.Count > 1)
                            {
                                HashSet<Node> nearNodes = new HashSet<Node>();
                                query.
                                    Select(x => x.NearNode(node)).
                                    ToList().
                                    ForEach(it => nearNodes.Add(it));
                                if (nearNodes.Count > 1)
                                {
                                    double min = Double.MaxValue;
                                    Node current = null;
                                    foreach (var item in nearNodes)
                                    {
                                        if (min > Distance(item, node))
                                        {
                                            min = Distance(item, node);
                                            current = item;
                                        }
                                    }
                                    nodes.Add(current);
                                }
                                else
                                    nodes.Add(nearNodes.First());
                            }
                            else if (query.Count > 0)
                                nodes.Add(query[0].NearNode(node));
                        }

                        foreach (var node in nodes)
                        {
                            double range = step * 2 / 3;
                            var querySmoothTriangles = smoothItem
                                .Where(tr => tr.Center.PX >= node.PX - range && tr.Center.PX <= node.PX + range && tr.Center.PY >= node.PY - range && tr.Center.PY <= node.PY + range)
                                .ToList();
                            trianglesForSmoothByNodes.Add(querySmoothTriangles);
                            trianglesForSmooth.AddRange(querySmoothTriangles);
                        }
                        if (boundNodes.Count > 1)
                        {
                            trianglesForSmooth.ForEach(tr =>
                            {
                                if (trianglesForSmooth.Where(x => x.GetHashCode() == tr.GetHashCode()).Count() == 2)  // see may be > 1
                                    smoothingTriangles.Add(tr);
                            });
                            List<List<int>> indSmooth = IndexesSmoothNodes(trianglesForSmoothByNodes, nodes, smoothingTriangles);
                            if (smoothingTriangles.Count >= 1)
                            {
                                for (int i = 0; i < boundNodes.Count; i++)
                                {
                                    result.AddRange(SmoothTetrahedronsByNodes(i, (i + 1) % boundNodes.Count, boundNodes, indSmooth, smoothingTriangles.ToList()));
                                    if (boundNodes.Count == 2) break;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Set indexes of the base nodes in the smoothing triangles
        /// </summary>
        /// <param name="trianglesSmoothByNodes">Triangles for smoothing by nodes</param>
        /// <param name="nodes">List basic nodes</param>
        /// <param name="smoothingTriangles">Smooting triangles</param>
        /// <returns>Indexes of the base nodes in the smoothing triangles</returns>
        List<List<int>> IndexesSmoothNodes(List<List<Triangle>> trianglesSmoothByNodes, List<Node> nodes, HashSet<Triangle> smoothingTriangles)
        {
            List<List<int>> indexesList = new List<List<int>>();
            foreach (var item in smoothingTriangles)
            {
                List<int> tmpList = new List<int>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (item.Nodes.Where(n => n.PX >= nodes[i].PX - step / 2 && n.PX <= nodes[i].PX + step / 2 && n.PY >= nodes[i].PY - step / 2 && n.PY <= nodes[i].PY + step / 2).Count() >= 1)
                    {
                        if (trianglesSmoothByNodes[i].Where(x => x.GetHashCode() == item.GetHashCode()).Count() > 0)
                            tmpList.Add(i);
                    }
                }
                indexesList.Add(tmpList);
            }
            return indexesList;
        }

        /// <summary>
        /// Generate tetrahedrons by nodes and smoothing triangles
        /// </summary>
        /// <param name="first">Index of first node</param>
        /// <param name="second">Index of second node</param>
        /// <param name="nodes">Nodes for linking</param>
        /// <param name="idSmoothTriangles">Indexes of the base nodes in the smoothing triangles</param>
        /// <param name="smoothTriangles">Smothing triangles</param>
        /// <returns>Tetrahedrons by nodes and smoothing triangles</returns>
        List<Tetrahedron> SmoothTetrahedronsByNodes(int first, int second, List<Node> nodes, List<List<int>> idSmoothTriangles, List<Triangle> smoothTriangles)
        {
            for (int i = 0; i < idSmoothTriangles.Count; i++)
            {
                if (idSmoothTriangles[i].Count == 0) idSmoothTriangles.RemoveAt(i--);
            }
            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();
            if (smoothTriangles.Count == 2 && idSmoothTriangles.Where(x => x.Count == 1).Count() == 1)
            {
                Node firstNode = smoothTriangles[first].NearNode(nodes[first]);
                Node secondNode = smoothTriangles[second].NearNode(nodes[second]);
                firstNode = (Distance(firstNode, nodes[first]) <= step / 2) ? firstNode : null;
                secondNode = (Distance(secondNode, nodes[second]) <= step / 2) ? secondNode : null;

                if (firstNode != null && secondNode == null)
                    tetrahedrons.AddRange(TetrahedronsFromHalfPrism(nodes[first], nodes[second], smoothTriangles[first]));
                if (secondNode != null && firstNode == null)
                    tetrahedrons.AddRange(TetrahedronsFromHalfPrism(nodes[first], nodes[second], smoothTriangles[second]));
                if (firstNode != null && secondNode != null)
                {
                    int sameSides = 0;
                    smoothTriangles[0].Nodes.ForEach(nd =>
                    {
                        smoothTriangles[1].Nodes.ForEach(it =>
                        {
                            if (nd.Equals(it)) sameSides++;
                        });
                    });
                    if (sameSides == 2)
                    {
                        Node firstTAnother = smoothTriangles[first].Nodes.
                            Where(x => (x.PX == firstNode.PX && x.PY == firstNode.PY) || (x.PX != secondNode.PX && x.PY != secondNode.PY)).
                            ToList()[0];
                        Node secondTAnother = smoothTriangles[second].Nodes.
                            Where(x => (x.PX == secondNode.PX || x.PY == secondNode.PY) && (x.PX != firstNode.PX && x.PY != firstNode.PY)).
                            ToList()[0];
                        Triangle firstSmTrngl = new Triangle(nodes[first], firstNode, firstTAnother);
                        Triangle secondSmTrngl = new Triangle(nodes[second], secondNode, secondTAnother);
                        tetrahedrons.AddRange(TetrahedronsFromPrism(firstSmTrngl, secondSmTrngl));
                    }
                    else
                    {
                        tetrahedrons.AddRange(TetrahedronsFromHalfPrism(nodes[first], nodes[second], smoothTriangles[0]));
                        tetrahedrons.AddRange(TetrahedronsFromHalfPrism(nodes[first], nodes[second], smoothTriangles[1]));
                    }
                }

            }
            else if (smoothTriangles.Count == 2)
            {
                tetrahedrons.AddRange(TetrahedronsFromHalfPrism(nodes[first], nodes[second], smoothTriangles[0]));
                tetrahedrons.AddRange(TetrahedronsFromHalfPrism(nodes[first], nodes[second], smoothTriangles[1]));
            }
            else // for count == 1
            {
                tetrahedrons.AddRange(TetrahedronsFromHalfPrism(nodes[first], nodes[second], smoothTriangles[0]));
            }
            return tetrahedrons;
        }

        /// <summary>
        /// Calculate distance between nodes
        /// </summary>
        /// <param name="first">First node</param>
        /// <param name="second">Second node</param>
        /// <returns>Distance between nodes</returns>
        double Distance(Node first, Node second)
        {
            return Math.Sqrt(Math.Pow(first.PX - second.PX, 2) + Math.Pow(first.PY - second.PY, 2));
        }

        /// <summary>
        /// Generate tetrahedrons from prism
        /// </summary>
        /// <param name="first">Top triangle of prism</param>
        /// <param name="second">Bottom triangle of prism</param>
        /// <returns>Tetrahedrons</returns>
        List<Tetrahedron> TetrahedronsFromPrism(Triangle first, Triangle second)
        {
            List<Tetrahedron> result = new List<Tetrahedron>();

            result.Add(new Tetrahedron(first.Nodes[0], second.Nodes[0], second.Nodes[1], second.Nodes[2]));
            result.Add(new Tetrahedron(second.Nodes[1], first.Nodes[0], first.Nodes[1], first.Nodes[2]));
            result.Add(new Tetrahedron(first.Nodes[0], first.Nodes[2], second.Nodes[2], second.Nodes[1]));
            return result;
        }

        /// <summary>
        /// Generate tetrahedrons from half prism
        /// </summary>
        /// <param name="first">First node of top prism</param>
        /// <param name="second">Second node of top prism</param>
        /// <param name="triangle">Bottom side triangle</param>
        /// <returns>Tetrahedrons from half prism</returns>
        List<Tetrahedron> TetrahedronsFromHalfPrism(Node first, Node second, Triangle triangle)
        {
            List<Tetrahedron> result = new List<Tetrahedron>();

            result.Add(new Tetrahedron(first, triangle.Nodes[0], triangle.Nodes[1], triangle.Nodes[2]));
            int num = triangle.NearNodeIndex(second);
            result.Add(new Tetrahedron(first, second, triangle.Nodes[num], triangle.Nodes[(num + 1) % 3]));
            return result;
        }

        /// <summary>
        /// Find bound nodes in triangle
        /// </summary>
        /// <param name="item">Triangle for search</param>
        /// <returns>Bound nodes</returns>
        List<Node> GetBoundNodes(Triangle item)
        {
            List<Node> notRelated = new List<Node>(item.Nodes.Where(x => x.IsBound).ToList());
            return notRelated;
        }

        /// <summary>
        /// Find not related triangles in a layer (in all areas of the layer)
        /// </summary>
        /// <param name="trianglesLayer">Triangles layer</param>
        /// <returns>Not related triangles</returns>
        List<List<Triangle>> SetNotRelated(List<List<Triangle>> trianglesLayer)
        {
            List<List<Triangle>> result = new List<List<Triangle>>();
            trianglesLayer.ForEach(it => result.Add(new List<Triangle>(it)));
            return result;
        }

        /// <summary>
        /// Find bound triangles in a layer (in all areas of the layer)
        /// </summary>
        /// <param name="trianglesLayer">Triangles layer</param>
        /// <returns>Bound triangles in a layer</returns>
        List<List<Triangle>> SetBoundTriangles(List<List<Triangle>> trianglesLayer)
        {
            List<List<Triangle>> result = new List<List<Triangle>>();

            trianglesLayer.ForEach(item =>
            result
            .Add(new List<Triangle>(item.Where(tr => tr.Nodes.Where(nd => nd.IsBound).Count() > 1).ToList())));
            return result;
        }
        #endregion
    }
}
