using OpenCvSharp;
using System.Drawing;

namespace App
{
    public struct Layer
    {
        public DicomDecoder Dicom { get; set; }

        public Mat Origin { get; set; }

        public Mat SegmentMatrix { get; set; }

        public Mat SegmentMatrixJpg { get; set; }

        public Bitmap ImageFromJpg { get; set; }


        //public Mat LaplaceMatrix { get; set; }

        //public int InstanceNumber { get; set; }

        //public int SeriesNumber { get; set; }
    }
}