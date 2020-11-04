using MeshGenerator.Elements;
using MeshGenerator.Modelling.Conditions;
using MeshGenerator.Planes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Loads
{
    public class Force : ILoad
    {
        private double value;
        private Dictionary<int, LoadVector> loadVectors;
        private List<IPlane> topPlanes;
        private List<IPlane> bottomPlanes;
        private List<Node> nodes;
        private List<Triangle> triangles;
        private double step;

        #region Constructors
        public Force(List<SelectedSide> loadedSides, double value, bool isPositive, List<Node> nodes, List<IPlane> topPlanes, List<IPlane> bottomPlanes)
        {
            this.topPlanes = topPlanes;
            this.bottomPlanes = bottomPlanes;
            this.value = value;
            this.nodes = nodes;

            loadVectors = new Dictionary<int, LoadVector>();
            foreach (var side in loadedSides)
            {
                Dictionary<int, LoadVector> vectors = SetLoadedNodes(side, isPositive);

                foreach (var item in vectors)
                {
                    loadVectors.Add(item.Key, item.Value);
                }
            }
        }

        public Force(SelectedSide loadedSide, double value, bool isPositive, List<Triangle> triangles)
        {
            this.triangles = triangles;
            List<Triangle> trngls = SetLoadedTriangles(loadedSide);
            this.value = value;

            loadVectors = SetLoadedNodes(loadedSide, isPositive, trngls);
        }

        public Force(SelectedSide loadedSide, Node point, double value, bool isPositive, List<Triangle> triangles)
        {
            this.triangles = triangles;
            List<Triangle> trngls = SetLoadedTriangles(point);
            HashSet<Node> nodeSet = new HashSet<Node>();
            trngls.ForEach(trn => trn.Nodes.ForEach(nd => nodeSet.Add(nd)));
            double area = trngls.Sum(tr => tr.Area());
            this.value = value / (trngls.Count * 3 / nodeSet.Count);
            LoadedTriangles = trngls;

            loadVectors = SetLoadedNodes(loadedSide, isPositive, trngls);
        }

        public Force(SelectedSide loadedSide, double value, bool isPositive, List<Node> nodes, double step = 0.0001)
        {
            this.nodes = nodes;
            this.value = value;
            this.step = step;
            loadVectors = SetLoadedNodes(loadedSide, isPositive, true);
        }
        #endregion

        #region Methods
        Dictionary<int, LoadVector> SetLoadedNodes(SelectedSide side, bool isPositive)
        {
            Dictionary<int, LoadVector> result = new Dictionary<int, LoadVector>();
            double value = 0;
            switch (side)
            {
                case SelectedSide.TOP:
                    value = isPositive ? (-1) * Value : Value;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (topPlanes[0].GetProjection(nodes[i]) != null)
                        {
                            Node projection = topPlanes[0].GetProjection(nodes[i]);
                            bool isEquals = (Math.Round(projection.X, 2) == Math.Round(nodes[i].X, 2))
                                && (Math.Round(projection.Y, 2) == Math.Round(nodes[i].Y, 2))
                                && (Math.Round(projection.Z, 2) == Math.Round(nodes[i].Z, 2));

                            if (projection.Equals(nodes[i]) || isEquals)
                            {
                                result.Add(nodes[i].GlobalIndex, new LoadVector(value, VectorDirection.Z));
                            }
                        }
                    }
                    break;
                case SelectedSide.BOTTOM:
                    value = isPositive ? Value : (-1) * Value;
                    nodes.ForEach(n =>
                    {
                        bottomPlanes.ForEach(pl =>
                        {
                            if (pl.GetProjection(n).Equals(n))
                                result.Add(n.GlobalIndex, new LoadVector(value, VectorDirection.Z));
                        });
                    });
                    break;
            }
            return result;
        }

        Dictionary<int, LoadVector> SetLoadedNodes(SelectedSide side, bool isPositive, bool isFlag)
        {
            Dictionary<int, LoadVector> result = new Dictionary<int, LoadVector>();
            var nodesToLoad = new List<Node>();
            double value = isPositive ? (-1) * Value : Value;
            double boundZ = 0.0;
            switch (side)
            {
                case SelectedSide.TOP:
                    boundZ = nodes.Max(nd => nd.Z);

                    break;
                case SelectedSide.BOTTOM:
                    boundZ = nodes.Min(nd => nd.Z);
                    break;
            }
            nodesToLoad.AddRange(nodes.Where(nd => nd.Z <= boundZ + step && nd.Z >= boundZ - step).ToList());

            nodesToLoad.ForEach(n =>
            {
                if (!result.ContainsKey(n.GlobalIndex))
                {
                    result.Add(n.GlobalIndex, new LoadVector(value, VectorDirection.Z));
                }
            });

            return result;
        }

        Dictionary<int, LoadVector> SetLoadedNodes(SelectedSide side, bool isPositive, List<Triangle> loadedTriangles)
        {
            Dictionary<int, LoadVector> result = new Dictionary<int, LoadVector>();

            double value = 0;
            switch (side)
            {
                case SelectedSide.TOP:
                    value = isPositive ? (-1) * Value : Value;
                    break;
                case SelectedSide.BOTTOM:
                    value = isPositive ? Value : (-1) * Value;
                    break;
                default:
                    throw new NotImplementedException();
            }

            foreach (var item in loadedTriangles)
            {
                item.Nodes.ForEach(n =>
                {
                    if (!result.ContainsKey(n.GlobalIndex))
                    {
                        result.Add(n.GlobalIndex, new LoadVector(value, VectorDirection.Z));
                    }
                });
            }

            return result;
        }

        List<Triangle> SetLoadedTriangles(SelectedSide side)
        {
            ConcurrentBag<Triangle> list = new ConcurrentBag<Triangle>();
            Dictionary<Node, Triangle> dictTriangles = new Dictionary<Node, Triangle>();
            triangles.ForEach(tr => dictTriangles.Add(tr.Center, tr));
            int processCount = triangles.Count;
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                foreach (var item in triangles)
                {
                    ThreadPool.QueueUserWorkItem(thrItem =>
                    {
                        double distance = Distance((thrItem as Triangle).Nodes[0], (thrItem as Triangle).Nodes[1]) * 0.5;
                        Node center = (thrItem as Triangle).Center;
                        switch (side)
                        {
                            case SelectedSide.TOP:
                                List<Triangle> higherTrnls = new List<Triangle>();

                                foreach (var key in dictTriangles.Keys)
                                {
                                    if (Distance(key, center) <= distance && !key.Equals(center) && key.Z > center.Z)
                                    {
                                        higherTrnls.Add(dictTriangles[key]);
                                        break;
                                    }
                                }
                                if (higherTrnls.Count == 0)
                                {
                                    list.Add(item);
                                }
                                break;
                            case SelectedSide.BOTTOM:
                                List<Triangle> lowerTrnls = new List<Triangle>();
                                foreach (var key in dictTriangles.Keys)
                                {
                                    if (Distance(key, center) <= distance && !key.Equals(center) && key.Z < center.Z)
                                    {
                                        lowerTrnls.Add(dictTriangles[key]);
                                        break;
                                    }
                                }
                                if (lowerTrnls.Count == 0)
                                {
                                    list.Add(item);
                                }
                                break;
                            default: throw new NotImplementedException();
                        }
                        if (Interlocked.Decrement(ref processCount) == 0)
                        {
                            resetEvent.Set();
                        }
                    }, item);
                }
                resetEvent.WaitOne();
            }

            return list.ToList();
        }

        List<Triangle> SetLoadedTriangles(Node point)
        {
            List<Triangle> list = new List<Triangle>();

            foreach (var item in triangles)
            {
                List<double> distances = new List<double>()
                {
                    Distance(item.Nodes[0], item.Nodes[1]),
                    Distance(item.Nodes[0], item.Nodes[2]),
                    Distance(item.Nodes[1], item.Nodes[2])
                };
                double distance = distances.Average();
                if (item.Center.Z >= point.Z - distance && item.Center.Z <= point.Z + distance)
                {
                    list.Add(item);
                }
            }

            double minX = list.Min(trngl => trngl.Center.X);
            double maxX = list.Max(trngl => trngl.Center.X);

            //var res = list.Where(trngl => trngl.Center.X >= minX + (maxX - minX) / 2.0).ToList();
            //var res = list.Where(trngl => trngl.Center.X <= minX + (maxX - minX) / 2.0).ToList();
            //return res;
            return list;
        }
        /// <summary>
        /// Calculate distance between nodes
        /// </summary>
        /// <param name="first">First node</param>
        /// <param name="second">Second node</param>
        /// <returns>Distance between nodes</returns>
        double Distance(Node first, Node second)
        {
            return Math.Sqrt(Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2));
        }
        #endregion

        #region Properties
        public double Value => value;

        public Dictionary<int, LoadVector> LoadVectors => loadVectors;
        public List<Triangle> LoadedTriangles { get; private set; } = new List<Triangle>(); //TODO remove after testing

        #endregion
    }
}
