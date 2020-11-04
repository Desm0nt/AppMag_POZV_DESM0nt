using MeshGenerator.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Triangulation
{
    public class Triangulating
    {
        private int step;
        /// <summary>
        /// Triangulation of the area
        /// </summary>
        /// <param name="step">Step of the finite element mesh</param>
        public Triangulating(int step = 10)
        {
            this.step = step;
        }

        #region triangulation, working good with regular mesh (not so universal like Delone)
        /// <summary>
        /// Iterative triangles generation
        /// </summary>
        /// <param name="points">Nodes of an area</param>
        /// <returns>List of triangles</returns>
        public Dictionary<int, Triangle> GenerateTriangles(List<Node> points)
        {
            Dictionary<int, Triangle> triangles = new Dictionary<int, Triangle>();
            Dictionary<int, List<Node>> layersByY = new Dictionary<int, List<Node>>();
            for (int i = points[0].PY; i <= points.Max(y => y.PY); i += step)
            {
                layersByY.Add(i, points.Where(p => p.PY == i).ToList());
            }
            Node center = null;
            Node basePoint = null;
            Triangle baseTriangle = null;

            if (points.Count == 2 && Distance(points[0], points[1]) <= step * Math.Sqrt(2))
            {
                center = CenterOfSide(points[0], points[1]);
                basePoint = new Node(center.PX + (points[0].PY - points[1].PY), center.PY + step, points[0].PZ, points[0].IdMaterial);
                baseTriangle = new Triangle(points[0], points[1], basePoint);
                triangles.Add(baseTriangle.GetHashCode(), baseTriangle);
            }
            if (points.Count > 2)
            {
                Triangulate(ref triangles, points);
            }

            return triangles;
        }

        /// <summary>
        /// Triangulation is performed
        /// </summary>
        /// <param name="triangles">Reference on the returned result of triangulation</param>
        /// <param name="points">Nodes of an area</param>
        void Triangulate(ref Dictionary<int, Triangle> triangles, List<Node> points)
        {
            Triangle baseTriangle = null;
            for (int i = 0; i < points.Count; i++)
            {
                if ((points[i].PX - points[0].PX) % step == 0 || Math.Abs(points[i].PX - points[0].PX) % (step) >= 5)  // think about remove it
                {
                    List<Node> tPoints = new List<Node>();
                    var second = points
                            .Where(p => p.PX >= points[i].PX - step / 2 && p.PX <= points[i].PX + step / 2 && p.PY <= points[i].PY + step && p.PY > points[i].PY);
                    var third = points
                            .Where(p => p.PX > points[i].PX + step / 2 && p.PX <= points[i].PX + step && p.PY <= points[i].PY + step / 2 && p.PY >= points[i].PY - step / 2);
                    var forth = points
                            .Where(p => p.PX > points[i].PX + step / 2 && p.PX <= points[i].PX + step && p.PY <= points[i].PY + step + step / 2 && p.PY > points[i].PY + step - step / 2);

                    tPoints.Add(points[i]);
                    if (second.Count() > 0 && Distance(points[i], second.Last()) <= step * Math.Sqrt(2)) tPoints.Add(second.Last());
                    if (third.Count() > 0 && Distance(points[i], third.Last()) <= step * Math.Sqrt(2)) tPoints.Add(third.Last());
                    if (forth.Count() > 0 && Distance(points[i], forth.Last()) <= step * Math.Sqrt(2)) tPoints.Add(forth.Last());

                    if (tPoints.Count == 4)
                    {
                        baseTriangle = new Triangle(tPoints[0], tPoints[2], tPoints[1]);
                        //if (!triangles.ContainsKey(baseTriangle.GetHashCode()))
                        //{
                            triangles.Add(baseTriangle.GetHashCode(), baseTriangle);
                        //}
                        baseTriangle = new Triangle(tPoints[1], tPoints[2], tPoints[3]);
                        //if (!triangles.ContainsKey(baseTriangle.GetHashCode()))
                        //{
                            triangles.Add(baseTriangle.GetHashCode(), baseTriangle);
                        //}
                    }
                    if (tPoints.Count == 3)
                    {
                        baseTriangle = new Triangle(tPoints);
                        //if (!triangles.ContainsKey(baseTriangle.GetHashCode()))
                        //{
                            triangles.Add(baseTriangle.GetHashCode(), baseTriangle);
                        //}
                    }
                    if (second.Count() > 0)
                    {
                        var canInsert = points
                            .Where(p => p.PX < points[i].PX && p.PY >= points[i].PY - step / 2 && p.PY <= points[i].PY + step / 2);
                        if (canInsert.Count() == 0) // if current element is first in this layer
                        {
                            var query = points
                           .Where(p => p.PX < points[i].PX && p.PX >= points[i].PX - step && p.PY == points[i].PY + step);
                            if (query.Count() > 0)
                            {
                                baseTriangle = new Triangle(tPoints[0], tPoints[1], query.First());
                                if (!triangles.ContainsKey(baseTriangle.GetHashCode())) triangles.Add(baseTriangle.GetHashCode(), baseTriangle);
                            }
                        }
                    }
                }

            }
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
        /// Calculate center of a side
        /// </summary>
        /// <param name="first">First node of a side</param>
        /// <param name="second">Second node of a side</param>
        /// <returns>Center of a side</returns>
        Node CenterOfSide(Node first, Node second)
        {
            return new Node((first.PX + second.PX) / 2, (first.PY + second.PY) / 2, first.PZ, first.IdMaterial);
        }
        #endregion
    }
}
