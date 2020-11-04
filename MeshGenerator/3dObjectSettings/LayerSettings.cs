using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator._3dObjectSettings
{
    public class LayerSettings
    {
        private int step;
        private Dictionary<int, List<Node>> bounds;

        #region Properties
        /// <summary>
        /// Bounds of the areas at the current layer
        /// </summary>
        public List<Area> Areas
        {
            get; private set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Works with the bitmap image and find on it isolated areas
        /// </summary>
        /// <param name="layerNodes">Bounds of layer</param>
        /// <param name="step">step of the mesh</param>
        public LayerSettings(List<Node> layerNodes, int step = 10)
        {
            this.step = step;
            bounds = InitBounds(layerNodes);
            Areas = GetAllAreas();
        }
        #endregion

        #region Methods
        Dictionary<int, List<Node>> InitBounds(List<Node> layerNodes)
        {
            Dictionary<int, List<Node>> result = new Dictionary<int, List<Node>>();
            layerNodes.Sort(new NodesComparer());
            int startY = layerNodes[0].PY;
            int endY = layerNodes[layerNodes.Count - 1].PY;
            for (int py = startY; py < endY; py++)
            {
                List<Node> tmpList = new List<Node>();
                tmpList.AddRange(layerNodes.Where(node => node.PY == py).ToList());
                if(tmpList.Count > 0)
                {
                    result.Add(py, tmpList);
                    layerNodes.RemoveAll(node => node.PY == py);
                }
            }
            return result;
        }
        /// <summary>
        /// Find isolated areas at the binary representstion of the picture
        /// </summary>
        /// <returns>List of isolated areas</returns>
        public List<List<Node>> FindIsolatedAreas()
        {
            List<List<Node>> result = new List<List<Node>>
            {
                bounds.Values.First()
            };
            for (int i = 1; i < bounds.Count; i++)
            {
                int current = bounds.Keys.ToArray()[i];
                int previous = bounds.Keys.ToArray()[i - 1];
                if (!IsCurrentBound(bounds[previous], bounds[current]))
                {
                    List<List<Node>> areas = new List<List<Node>>();
                    if (IsAreasByX(result.Last(), ref areas))
                    {
                        result.Remove(result.Last());
                        result.AddRange(areas);
                    }
                    result.Add(bounds.Values.ToArray()[i]);
                }
                else
                {
                    result.Last().AddRange(bounds.Values.ToArray()[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// Still current area or not
        /// </summary>
        /// <param name="previous">Previous layer of nodes</param>
        /// <param name="current">Current layer of nodes</param>
        /// <returns>Curents bound or not</returns>
        bool IsCurrentBound(List<Node> previous, List<Node> current)
        {
            int count = 0;
            bool flag = false;
            foreach (Node curr in current)
            {
                flag = false;
                foreach (Node prev in previous)
                {
                    if (Distance(prev, curr) <= 1)
                    {
                        flag = true;
                    }
                }
                count = (flag) ? count + 1 : count;
                if (count > 1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Is there a few areas by x
        /// </summary>
        /// <param name="area">Current area</param>
        /// <param name="areas">List of areas at the current layer</param>
        /// <returns>Same areas is true, one area is false</returns>
        bool IsAreasByX(List<Node> area, ref List<List<Node>> areas)
        {
            bool flag = false;
            int previous = area.Min(x => x.PX) - 1;
            for (int i = area.Min(x => x.PX); i < area.Max(x => x.PX); i++)
            {
                if (area.Count(x => x.PX == i) == 0)
                {
                    areas.Add(area.Where(x => x.PX > previous && x.PX < i).ToList());
                    previous = i;
                    flag = true;
                }
            }
            areas.Add(area.Where(x => x.PX > previous).ToList());
            areas.RemoveAll(x => x.Count == 0);
            return flag;
        }

        /// <summary>
        /// Get all found areas at the picture
        /// </summary>
        /// <returns>List of the areas</returns>
        List<Area> GetAllAreas()
        {
            List<Area> areas = new List<Area>();
            List<List<Node>> isolatedAreas = FindIsolatedAreas();
            foreach (var item in isolatedAreas)
            {
                if (item.Count > 0)
                    areas.Add(new Area(item, step));
            }
            return areas;
        }

        /// <summary>
        /// Get the nearest to base point node 
        /// </summary>
        /// <param name="points">List of the nodes</param>
        /// <param name="basePoint">Base point</param>
        /// <returns>The nearest to base point node</returns>
        Node MinimalDistance(List<Node> points, Node basePoint)
        {
            Node result = null;
            if (points.Count > 0)
            {
                double minDistance = Math.Pow(points[0].PX - basePoint.PX, 2) + Math.Pow(points[0].PY - basePoint.PY, 2);
                result = points[0];
                points.ForEach(x =>
                {
                    if (minDistance > Math.Pow(x.PX - basePoint.PX, 2) + Math.Pow(x.PY - basePoint.PY, 2))
                    {
                        minDistance = Math.Pow(x.PX - basePoint.PX, 2) + Math.Pow(x.PY - basePoint.PY, 2);
                        result = x;
                    }
                });
            }
            else
                result = null;
            return result;
        }

        /// <summary>
        /// Calculate distance between two nodes
        /// </summary>
        /// <param name="first">First node</param>
        /// <param name="second">Second node</param>
        /// <returns>Distance between nodes</returns>
        double Distance(Node first, Node second)
        {
            return Math.Sqrt(Math.Pow(first.PX - second.PX, 2) + Math.Pow(first.PY - second.PY, 2));
        }
        #endregion
    }
}
