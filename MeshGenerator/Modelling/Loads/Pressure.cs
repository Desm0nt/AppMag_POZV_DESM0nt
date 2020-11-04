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
    /// <summary>
    /// Pressure
    /// </summary>
    public class Pressure : ILoad
    {
        private double value;
        private Dictionary<int, LoadVector> loadVectors;
        private List<IPlane> topPlanes;
        private List<IPlane> bottomPlanes;
        private List<Node> nodes;
        private List<Triangle> triangles;

        #region Constructors
        /// <summary>
        /// Pressure
        /// </summary>
        /// <param name="loadedSides">Pressure loaded to the sides</param>
        /// <param name="value">Value of thr pressure</param>
        /// <param name="isPositive">Pressure positive or negative (stretching or compression)</param>
        /// <param name="layers">layers of the model</param>
        /// <param name="triangles">Layers with a triangles for each picture with element of vertebra</param>
        public Pressure(List<SelectedSide> loadedSides, double value, bool isPositive, List<Node> nodes, List<IPlane> topPlanes, List<IPlane> bottomPlanes)
        {
            this.topPlanes = topPlanes;
            this.bottomPlanes = bottomPlanes;
            this.value = SetValue(value, loadedSides[0]);
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

        public Pressure(SelectedSide loadedSide, double value, bool isPositive, List<Triangle> triangles)
        {
            this.triangles = triangles;
            List<Triangle> trngls = SetLoadedTriangles(loadedSide);
            double area = trngls.Sum(tr => tr.Area());
            this.value = value / area;
            LoadedTriangles = trngls;

            loadVectors = SetLoadedNodes(loadedSide, isPositive, trngls);
        }

        public Pressure(SelectedSide loadedSide, Node point, double value, bool isPositive, List<Triangle> triangles)
        {
            this.triangles = triangles;
            List<Triangle> trngls = SetLoadedTriangles(point);
            double area = trngls.Sum(tr => tr.Area());
            this.value = value / area;
            LoadedTriangles = trngls;

            loadVectors = SetLoadedNodes(loadedSide, isPositive, trngls);
        }
        #endregion

        #region Properties
        public List<Triangle> LoadedTriangles { get; private set; } = new List<Triangle>(); //TODO remove after testing

        /// <summary>
        /// Load vectors
        /// </summary>
        public Dictionary<int, LoadVector> LoadVectors
        {
            get
            {
                return loadVectors;
            }
        }

        /// <summary>
        /// Value of pressure
        /// </summary>
        public double Value
        {
            get
            {
                return value;
            }
        }
        #endregion

        #region Methods
        double SetValue(double value, SelectedSide side)
        {
            double area = 0;
            switch (side)
            {
                case SelectedSide.TOP:
                    area = topPlanes.Sum(pl => pl.Area());
                    break;
                case SelectedSide.BOTTOM:
                    area = bottomPlanes.Sum(pl => pl.Area());
                    break;
                    //case SelectedSide.BOUNDARIES:
                    //    throw new NotImplementedException();
            }

            return value / area;
        }

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
                    //case SelectedSide.BOUNDARIES:
                    //    throw new NotImplementedException();
            }
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
            triangles.ForEach(tr =>
            {
                if (!dictTriangles.ContainsKey(tr.Center))
                    dictTriangles.Add(tr.Center, tr);
            });
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
                //double distance = Distance(item.Nodes[0], item.Nodes[1]);
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

        VectorDirection GetDirection(List<Node> nodes, Node curent, SelectedSide side)
        {
            if (side == SelectedSide.TOP || side == SelectedSide.BOTTOM)
                return VectorDirection.Z;
            else
            {
                throw new NotImplementedException();
            }
        }
        #endregion

    }
}
