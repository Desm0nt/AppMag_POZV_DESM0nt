using MeshGenerator.Elements;
using MeshGenerator.Planes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeshGenerator.Modelling.Conditions
{
    /// Description of boundary conditions for volume
    /// </summary>
    public class VolumeBoundaryCondition: IBoundaryCondition
    {
        private List<IPlane> topPlanes;
        private List<IPlane> bottomPlanes;
        private List<Node> nodes;
        private Dictionary<int, Node> fixedNodes;
        private List<Triangle> triangles;
        private double step;

        #region Constructors
        /// <summary>
        ///  Description of boundary conditions for volume
        /// </summary>
        /// <param name="fixations">Fixed sides</param>
        /// <param name="nodes">all nodes of the model</param>
        /// <param name="topPlanes">all top planes</param>
        /// <param name="bottomPlanes">all bottom planes</param>
        public VolumeBoundaryCondition(List<SelectedSide> fixations, List<Node> nodes, List<IPlane> topPlanes, List<IPlane> bottomPlanes)
        {
            this.topPlanes = topPlanes;
            this.bottomPlanes = bottomPlanes;
            this.nodes = nodes;
            fixedNodes = new Dictionary<int, Node>();
            foreach (var fixation in fixations)
            {
                List<Node> tmpList = new List<Node>();
                tmpList.AddRange(SetFixations(fixation));
                tmpList.ForEach(node =>
                {
                    if (!fixedNodes.ContainsKey(node.GlobalIndex))
                        fixedNodes.Add(node.GlobalIndex, node);
                });
            }
        }

        /// <summary>
        /// Description of boundary conditions for volume
        /// </summary>
        /// <param name="fixation">Fixed side</param>
        /// <param name="triangles">all fixed surfaces</param>
        public VolumeBoundaryCondition(SelectedSide fixation, List<Triangle> triangles)
        {
            this.triangles = triangles;
            List<Triangle> fixedTriangles = SetFixedTriangles(fixation);
            fixedNodes = SetFixations(fixedTriangles);

            FixedTriangles = fixedTriangles;
        }

        /// <summary>
        /// Description of boundary conditions for volume
        /// </summary>
        /// <param name="fixation">Fixed side</param>
        /// <param name="point">selected point on fixed surface</param>
        /// <param name="triangles">all fixed surfaces</param>
        public VolumeBoundaryCondition(SelectedSide fixation, Node point, List<Triangle> triangles)
        {
            this.triangles = triangles;
            List<Triangle> fixedTriangles = SetFixedTriangles(point);
            fixedNodes = SetFixations(fixedTriangles);

            FixedTriangles = fixedTriangles;
        }

        public VolumeBoundaryCondition(SelectedSide fixation, List<Node> nodes, double step = 0.0001)
        {
            this.nodes = nodes;
            this.step = step;
            fixedNodes = SetFixations(fixation, true);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Fixed nodes by the boundary conditions
        /// </summary>
        public Dictionary<int, Node> FixedNodes
        {
            get
            {
                return fixedNodes;
            }
        }

        public List<Triangle> FixedTriangles { get; private set; } = new List<Triangle>(); //TODO remove after testing
        #endregion

        #region Methods
        List<Node> SetFixations(SelectedSide fixation)
        {
            List<Node> list = new List<Node>();
            switch (fixation)
            {
                case SelectedSide.TOP:
                    nodes.ForEach(n =>
                    {
                        topPlanes.ForEach(pl =>
                        {
                            if (pl.GetProjection(n).Equals(n))
                                list.Add(n);
                        });
                    });
                    break;
                case SelectedSide.BOTTOM:
                    nodes.ForEach(n =>
                    {
                        bottomPlanes.ForEach(pl =>
                        {
                            if (pl.GetProjection(n) != null)
                            {
                                if(pl.GetProjection(n).Equals(n))
                                    list.Add(n);
                            }
                        });
                    });
                    break;
                    //case SelectedSide.BOUNDARIES:
                    //    for (int i = 0; i < layers.Count; i++)
                    //    {
                    //        layers[i].ForEach(area => {
                    //            area.Nodes.ForEach(node => {
                    //                if (node.IsBound)
                    //                    nodes.Add(node);
                    //            });
                    //        });
                    //    }
                    //    break;
            }
            return list;
        }

        Dictionary<int, Node> SetFixations(SelectedSide fixation, bool isFlag)
        {
            Dictionary<int, Node> result = new Dictionary<int, Node>();
            var nodesToFix = new List<Node>();
            double boundZ = 0.0;
            switch (fixation)
            {
                case SelectedSide.TOP:
                    boundZ = nodes.Max(nd => nd.Z);

                    break;
                case SelectedSide.BOTTOM:
                    boundZ = nodes.Min(nd => nd.Z);
                    break;
            }
            var cloudPoints = nodes.Where(nd => nd.Z <= boundZ + step && nd.Z >= boundZ - step).ToList();
            HashSet<Node> pointSet = new HashSet<Node>();
            foreach(var pt in cloudPoints)
            {
                var samePoints = cloudPoints.Where(nd => nd.X == pt.X && nd.Y == pt.Y).ToList();
                double min = Math.Sqrt(Math.Pow(samePoints[0].Z - boundZ, 2));
                Node minNode = samePoints[0];
                for (int i = 0; i < samePoints.Count; i++)
                {
                    if (min > Math.Sqrt(Math.Pow(samePoints[i].Z - boundZ, 2)))
                    {
                        min = Math.Sqrt(Math.Pow(samePoints[i].Z - boundZ, 2));
                        minNode = samePoints[i];
                    }
                }
                pointSet.Add(minNode);
            }
            nodesToFix.AddRange(pointSet.ToList());

            nodesToFix.ForEach(nd => 
            {
                result.Add(nd.GlobalIndex, nd);
            });
            
            return result;
        }

        Dictionary<int, Node> SetFixations(List<Triangle> fixedTriangles)
        {
            Dictionary<int, Node> list = new Dictionary<int, Node>();

            foreach (var item in fixedTriangles)
            {
                item.Nodes.ForEach(n =>
                {
                    if (!list.ContainsKey(n.GlobalIndex))
                    {
                        list.Add(n.GlobalIndex, n);
                    }
                });
            }

            return list;
        }

        List<Triangle> SetFixedTriangles(SelectedSide side)
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
                        double distance = Distance((thrItem as Triangle).Nodes[0], (thrItem as Triangle).Nodes[1]) / 2.0;
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

        List<Triangle> SetFixedTriangles(Node point)
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
                double distance = distances.Average() / 1.5;

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


    }
}
