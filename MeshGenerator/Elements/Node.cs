using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Elements
{
    /// <summary>
    /// Consist data about node of the model
    /// </summary>
    [Serializable]
    public class Node
    {
        #region Fields and constants
        /// <summary>
        /// Constant for convert pixels to milimeters (by DICOM)
        /// </summary>
        private const double DICOM_PX_TO_COORDINATE = 1.0; //1.0 / 1000;
        private double x;
        private double y;
        private double z;
        private int px;
        private int py;
        private int pz;
        #endregion

        #region Constructors
        /// <summary>
        /// Consist data about node of the model
        /// </summary>
        /// <param name="x">Coordinate x</param>
        /// <param name="y">Coordinate y</param>
        /// <param name="z">Coordinate z</param>
        /// <param name="idMaterial">Material ID</param>
        /// <param name="isBound">Node on the border of the layer or not (false by default)</param>
        public Node(double x, double y, double z, int idMaterial = -1, double defColor = 0, bool isBound = false)
        {
            X = x;
            Y = y;
            Z = z;

            IdMaterial = idMaterial;
            DefColor = defColor;
            IsBound = isBound;
        }

        /// <summary>
        /// Consist data about node of the model
        /// </summary>
        /// <param name="px">Coordinate by pixels x</param>
        /// <param name="py">Coordinate by pixels y</param>
        /// <param name="pz">Coordinate by pixels z</param>
        /// <param name="idMaterial">Material ID</param>
        /// <param name="isBound">Node on the border of the layer or not (false by default)</param>
        public Node(int px, int py, int pz, int idMaterial = -1, double defColor = 0, bool isBound = false)
        {
            PX = px;
            PY = py;
            PZ = pz;

            x = px * DICOM_PX_TO_COORDINATE;
            y = py * DICOM_PX_TO_COORDINATE;
            z = pz * DICOM_PX_TO_COORDINATE;

            IdMaterial = idMaterial;
            DefColor = defColor;
            IsBound = isBound;
        }

        /// <summary>
        /// Consist data about node of the model
        /// </summary>
        /// <param name="px">Coordinate by pixels x</param>
        /// <param name="py">Coordinate by pixels y</param>
        /// <param name="z">Coordinate z</param>
        /// <param name="isBound">Node on the border of the layer or not (false by default)</param>
        public Node(int px, int py, double z, int idMaterial = -1, double defColor = 0, bool isBound = false)
        {
            PX = px;
            PY = py;
            Z = z;

            x = px * DICOM_PX_TO_COORDINATE;
            y = py * DICOM_PX_TO_COORDINATE;

            IdMaterial = idMaterial;
            DefColor = defColor;
            IsBound = isBound;
        }

        /// <summary>
        /// Consist data about node of the model
        /// </summary>
        /// <param name="node">Node</param>
        public Node(Node node)
        {
            X = node.X;
            Y = node.Y;
            Z = node.Z;

            GlobalIndex = node.GlobalIndex;
            IdMaterial = node.IdMaterial;
            IsBound = node.IsBound;
            DefColor = node.DefColor;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Global node ID
        /// </summary>
        public int GlobalIndex { get; set; } = -1;
        
        /// <summary>
        /// ID Material of the element
        /// </summary>
        public int IdMaterial { get; set; }
        public double DefColor { get; set; }
        /// <summary>
        /// Coordinate x
        /// </summary>
        public double X
        {
            get => x;
            set
            {
                x = value;
                PX = (int)(value / DICOM_PX_TO_COORDINATE);
            }
        }

        /// <summary>
        /// Coordinate y
        /// </summary>
        public double Y
        {
            get => y;
            set
            {
                y = value;
                PY = (int)(value / DICOM_PX_TO_COORDINATE);
            }
        }

        /// <summary>
        /// Coordinate z
        /// </summary>
        public double Z
        {
            get => z;
            set
            {
                z = value;
                PZ = (int)(value / DICOM_PX_TO_COORDINATE);
            }
        }

        /// <summary>
        /// Pixel by coordinate x
        /// </summary>
        public int PX
        {
            get => px;
            private set
            {
                px = value;
            }
        }

        /// <summary>
        /// Pixel by coordinate y
        /// </summary>
        public int PY
        {
            get => py;
            private set
            {
                py = value;
            }
        }

        /// <summary>
        /// Pixel by coordinate z
        /// </summary>
        public int PZ
        {
            get => pz;
            private set
            {
                pz = value;
            }
        }

        
        /// <summary>
        /// Node on the border of the layer or not
        /// </summary>
        public bool IsBound { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets hashcode of node
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (31 * X.GetHashCode())
                ^ Y.GetHashCode()
                ^ ((int)Z).GetHashCode();
        }

        /// <summary>
        /// Comparation for nodes
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Node point = obj as Node;
            if (point == null)
            {
                return false;
            }
            return (X == point.X) && (Y == point.Y) && (Z == point.Z);
        }

        /// <summary>
        /// Converts node to string format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $" ID: {GlobalIndex}, x: {X}, y: {Y}, z: {Z}";
        }
        #endregion
    }
}
