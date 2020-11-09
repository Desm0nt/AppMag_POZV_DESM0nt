using OpenCvSharp;
using System.Drawing;
using System.IO;

namespace AppLib
{
    public class NewLayer
    {
        public DicomDecoder Dicom { get; private set; }

        public Mat Origin { get; private set; }

        public Mat SegmentMatrix { get; private set; }

        public Mat SegmentMatrixJpg { get; private set; }

        public Bitmap ImageFromJpg { get; private set; }

        public NewLayer(string file)
        {
            //Декодирование Dicom файла
            Dicom = new DicomDecoder { DicomFileName = file };
            var filepath = Path.GetDirectoryName(file);
            var filename = Path.GetFileNameWithoutExtension(file);
            ImageFromJpg = new Bitmap(filepath + "\\jpg\\" + filename + ".jpg");

            SegmentMatrixJpg = OpenCvSharp.Extensions.BitmapConverter.ToMat(ImageFromJpg);

            //Усредняющий фильтр
            //toDo вернуть фильтр назад!
            //Dicom.Pixels16 = AveragingFilter(Dicom.Pixels16Origin, Dicom.width, Dicom.height, 5);

            //Сегментация
            Origin = Mat.Zeros(new OpenCvSharp.Size(Dicom.width, Dicom.height), MatType.CV_16UC1);
            SegmentMatrix = Mat.Zeros(new OpenCvSharp.Size(Dicom.width, Dicom.height), MatType.CV_16UC1);
            Mat laplace = Mat.Zeros(new OpenCvSharp.Size(Dicom.width, Dicom.height), MatType.CV_16UC1);
            for (int w = 0; w < Dicom.width; ++w)
            {
                for (int h = 0; h < Dicom.height; ++h)
                {
                    SegmentMatrix.Set(w, h, Dicom.Pixels16[w * Dicom.height + h] >= 32900 ? (ushort)40000 : (ushort)10);
                    Origin.Set(w, h, Dicom.Pixels16[w * Dicom.height + h]);
                }
            }

            //Обработка изображения оператором Лапласа
            //Cv2.Laplacian(segment, laplace, MatType.CV_16UC1);

        }
    }
}
