using EdgeDetectorsLib;

namespace EdgeDetectors
{
    class Program
    {
        static void Main(string[] args)
        {
            DicomFolderManager dicomFolderManager = new DicomFolderManager(@"Dicom", @"DetectorsResults");

        }
    }
}
