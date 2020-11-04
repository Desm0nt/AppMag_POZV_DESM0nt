using MeshGenerator.Elements;
using MeshGenerator.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator._3dObjectSettings
{
    public class Area
    {
        List<Node> areaBounds;
        int step;

        #region Constructors of class Area
        /// <summary>
        /// Information about area and nodes which it consists 
        /// </summary>
        /// <param name="areaBounds">Bounds of the area</param>
        /// <param name="step">Step of the mesh (in pixels)</param>
        public Area(List<Node> areaBounds, int step = 10)
        {
            this.areaBounds = areaBounds;
            if (areaBounds.Count > 1)
            {
                this.step = step;
                Nodes = GeneratePoints();
                Nodes.Sort(new NodesComparer());
            }
            else
                Nodes = areaBounds;

        }
        #endregion

        #region Methods
        /// <summary>
        /// Generating points which consists the area
        /// </summary>
        /// <returns>Nodes of the area</returns>
        List<Node> GeneratePoints()
        {
            List<Node> points = new List<Node>
            {
                areaBounds[0]
            };
            bool isNearTop = false;
            bool isNearBottom = false;
            Node bottomPoint = areaBounds.Last();
            int endY = (bottomPoint.PY - points[0].PY) % step;
            for (int py = points[0].PY + step; py < bottomPoint.PY; py += step)
            {
                isNearTop = (py == points[0].PY + step); // set true if this is first layer under zero point
                isNearBottom = (py + step >= bottomPoint.PY); // set true if this is last layer before bottom point

                Node leftPoint = areaBounds.Where(p => p.PY == py).First();
                if ((points[0].PX - leftPoint.PX) % step >= step / 2 && (points[0].PX - leftPoint.PX) % step < step)
                    points.Add(leftPoint);

                int left = ((points[0].PX - leftPoint.PX) % step == 0)
                    ? leftPoint.PX
                    : leftPoint.PX + (points[0].PX - leftPoint.PX) % step;
                Node rightPoint = areaBounds.Where(p => p.PY == py).Last();

                for (int px = left; px < rightPoint.PX; px += step)
                {
                    if (isNearTop)
                    {
                        var topQuery = areaBounds.Where(x => x.PX == px && x.PY <= py - step / 2);
                        if (topQuery.Count() > 0)
                        {
                            if (topQuery.First().PX != points[0].PX) points.Add(topQuery.First());
                        }

                    }
                    if (isNearBottom)
                    {
                        var bottomQuery = areaBounds.Where(x => x.PY >= py + step / 2);
                        if (bottomQuery.Count() > 0) points.Add(bottomQuery.Last());
                    }
                    if (IsInsideBounds(px, py))
                    {
                        Node point = new Node(px, py, points[0].PZ, MaterialStorage.Materials[0].Id)
                        {
                            IsBound = (px - step / 2 < left || px + step / 2 > rightPoint.PX || py + step > bottomPoint.PY)
                        };
                        points.Add(point);
                    }
                }
                if (points.Last().PX + step / 2 <= rightPoint.PX) points.Add(rightPoint);
            }
            return points;
        }

        /// <summary>
        /// Check point inside the area
        /// </summary>
        /// <param name="px">Pixel by x</param>
        /// <param name="py">Pixel by y</param>
        /// <returns>Returns true if point inside area</returns>
        bool IsInsideBounds(int px, int py)
        {
            List<Node> xLayer = areaBounds.Where(y => py == y.PY).ToList();
            List<Node> yLayer = areaBounds.Where(x => px == x.PX).ToList();
            if (IsInsideLayer(px, xLayer, "x") && IsInsideLayer(py, yLayer, "y"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check point is inside the layer
        /// </summary>
        /// <param name="current">Current coordinate by x or y</param>
        /// <param name="layer">List nodes of a layer</param>
        /// <param name="nameLayer">Name of layer (x or y)</param>
        /// <returns>Returns true if point inside layer</returns>
        bool IsInsideLayer(int current, List<Node> layer, string nameLayer)
        {
            if (layer.Count > 0)
            {
                int min = (nameLayer.CompareTo("x") == 0) ? layer.First().PX : layer.First().PY;
                int max = (nameLayer.CompareTo("x") == 0) ? layer.Last().PX : layer.Last().PY;
                if (current >= min && current <= max)
                    return true;
                else
                    return false;
            }
            return false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Nodes of the area
        /// </summary>
        public List<Node> Nodes { get; set; } = new List<Node>();
        #endregion
    }
}
