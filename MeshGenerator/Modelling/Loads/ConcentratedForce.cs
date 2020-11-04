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
    public class ConcentratedForce : ILoad
    {
        private double value;
        private Dictionary<int, LoadVector> loadVectors;
        private List<Triangle> triangles;
        private List<IPlane> topPlanes;
        private List<IPlane> bottomPlanes;
        private List<Node> nodes;
        private double step;

        #region Constructors
        public ConcentratedForce(List<SelectedSide> loadedSides, double value, bool isPositive, List<Node> nodes, List<IPlane> topPlanes, List<IPlane> bottomPlanes)
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

        public ConcentratedForce(SelectedSide loadedSide, Node point, double value, bool isPositive, List<Triangle> triangles)
        {
            this.triangles = triangles;
            List<Triangle> trngls = SetLoadedTriangles(point);

            this.value = value;
            LoadedTriangles = trngls;

            loadVectors = SetLoadedNodes(loadedSide, isPositive, trngls);
        }

        //public ConcentratedForce(SelectedSide loadedSide, double value, bool isPositive, List<Node> nodes)
        //{
        //    this.nodes = nodes;
        //    this.value = value;

        //    loadVectors = SetLoadedNodes(loadedSide, isPositive, true);
        //}

        public ConcentratedForce(SelectedSide loadedSide, double value, bool isPositive, List<Node> nodes, double step = 0.0001)
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
            var nodesToLoad = new List<Node>();
            double value = 0;
            value = isPositive ? (-1) * Value : Value;
            switch (side)
            {
                case SelectedSide.TOP:
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
                                nodesToLoad.Add(nodes[i]);
                            }
                        }
                    }
                    break;
                case SelectedSide.BOTTOM:
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (bottomPlanes[0].GetProjection(nodes[i]) != null)
                        {
                            Node projection = bottomPlanes[0].GetProjection(nodes[i]);
                            bool isEquals = (Math.Round(projection.X, 2) == Math.Round(nodes[i].X, 2))
                                && (Math.Round(projection.Y, 2) == Math.Round(nodes[i].Y, 2))
                                && (Math.Round(projection.Z, 2) == Math.Round(nodes[i].Z, 2));

                            if (projection.Equals(nodes[i]) || isEquals)
                            {
                                nodesToLoad.Add(nodes[i]);
                            }
                        }
                    }
                    break;
            }
            double maxX = nodesToLoad.Max(nd => nd.X);
            double minX = nodesToLoad.Min(nd => nd.X);
            double maxY = nodesToLoad.Max(nd => nd.Y);
            double minY = nodesToLoad.Min(nd => nd.Y);

            double centerX = (maxX + minX) / 2.0;
            double centerY = (maxY + minY) / 2.0;

            Node node = nodesToLoad[0];
            double min = Distance(nodesToLoad[0], new Node(centerX, centerY, 0.0));
            for (int i = 0; i < nodesToLoad.Count; i++)
            {
                if (min > Distance(nodesToLoad[i], new Node(centerX, centerY, 0.0)))
                {
                    min = Distance(nodesToLoad[i], new Node(centerX, centerY, 0.0));
                    node = nodesToLoad[i];
                }
            }

            result.Add(node.GlobalIndex, new LoadVector(value, VectorDirection.Z));

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

            double maxX = nodesToLoad.Max(nd => nd.X);
            double minX = nodesToLoad.Min(nd => nd.X);
            double maxY = nodesToLoad.Max(nd => nd.Y);
            double minY = nodesToLoad.Min(nd => nd.Y);

            double centerX = (maxX + minX) / 2.0;
            double centerY = (maxY + minY) / 2.0;

            Node node = nodesToLoad[0];
            double min = Distance(nodesToLoad[0], new Node(centerX, centerY, 0.0));
            for (int i = 0; i < nodesToLoad.Count; i++)
            {
                if (min > Distance(nodesToLoad[i], new Node(centerX, centerY, 0.0)))
                {
                    min = Distance(nodesToLoad[i], new Node(centerX, centerY, 0.0));
                    node = nodesToLoad[i];
                }
            }

            var centerNodes = nodesToLoad.Where(nd => nd.X == node.X && nd.Y == node.Y).ToList();

            node = centerNodes[0];
            min = Math.Sqrt(Math.Pow(centerNodes[0].Z - boundZ, 2));
            for (int i = 0; i < centerNodes.Count; i++)
            {
                if (min > Math.Sqrt(Math.Pow(centerNodes[i].Z - boundZ, 2)))
                {
                    min = Math.Sqrt(Math.Pow(centerNodes[i].Z - boundZ, 2));
                    node = centerNodes[i];
                }
            }

            result.Add(node.GlobalIndex, new LoadVector(value, VectorDirection.Z));

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

            HashSet<Node> nodeSet = new HashSet<Node>();
            loadedTriangles.ForEach(trn => trn.Nodes.ForEach(nd => nodeSet.Add(nd)));

            double maxX = nodeSet.Max(nd => nd.X);
            double minX = nodeSet.Min(nd => nd.X);
            double maxY = nodeSet.Max(nd => nd.Y);
            double minY = nodeSet.Min(nd => nd.Y);

            double centerX = (maxX + minX) / 2.0;
            double centerY = (maxY + minY) / 2.0;
            
            Node node = nodeSet.ElementAt(0);
            double min = Distance(nodeSet.ElementAt(0), new Node(centerX, centerY, 0.0));
            for (int i = 0; i < nodeSet.Count; i++)
            {
                if (min > Distance(nodeSet.ElementAt(i), new Node(centerX, centerY, 0.0)))
                {
                    min = Distance(nodeSet.ElementAt(i), new Node(centerX, centerY, 0.0));
                    node = nodeSet.ElementAt(i);
                }
            }
            
            result.Add(node.GlobalIndex, new LoadVector(value, VectorDirection.Z));

            return result;
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
                double distance = distances.Average() / 2.0;
                if (item.Center.Z >= point.Z - distance && item.Center.Z <= point.Z + distance)
                {
                    list.Add(item);
                }
            }

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
            return Math.Sqrt(Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2)/* + Math.Pow(first.Z - second.Z, 2)*/);
        }
        #endregion

        #region Properties
        public double Value => value;

        public Dictionary<int, LoadVector> LoadVectors => loadVectors;
        public List<Triangle> LoadedTriangles { get; private set; } = new List<Triangle>(); //TODO remove after testing

        #endregion
    }
}
