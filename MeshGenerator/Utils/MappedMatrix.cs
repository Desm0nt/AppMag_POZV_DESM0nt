using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Utils
{
    public class MappedMatrix
    {
        public static MemoryMappedFile Init(string path, string name, int length)
        {
            return MemoryMappedFile
                .CreateFromFile(path, System.IO.FileMode.Create, name, (uint)length * (uint)length * sizeof(double));
        }

        public static MemoryMappedFile Open(string path, string name)
        {
            return MemoryMappedFile
            .CreateFromFile(path, System.IO.FileMode.Open, name);
        }

        public static MemoryMappedFile OpenOrCreate(string path, string name, int length)
        {
            return MemoryMappedFile
            .CreateFromFile(path, System.IO.FileMode.OpenOrCreate, name, length * sizeof(double));
        }

        public static double GetDoubleValue(MemoryMappedViewAccessor accessor, int i, int j, int length)
        {
            //accessor.ReadArray<double>(0, temp, (i * length + j), 1);
            accessor.Read(((uint)i * (uint)length + (uint)j) * sizeof(double), out double value);
            return value;
        }

        public static void SetDoubleValue(MemoryMappedViewAccessor accessor, int i, int j, int length, double value)
        {
            accessor.Write(((uint)i * (uint)length + (uint)j) * sizeof(double), value);
        }

        public static double GetDoubleValue(MemoryMappedViewAccessor accessor, int j)
        {
            accessor.Read((uint)j * sizeof(double), out double value);
            return value;
        }

        public static double[] GetDoubleArray(MemoryMappedViewAccessor accessor, int length)
        {
            double[] result = new double[length];
            accessor.ReadArray(0, result, 0, length);
            return result;
        }

        public static void SetDoubleValue(MemoryMappedViewAccessor accessor, int j, double value)
        {
            accessor.Write((uint)j * sizeof(double), value);
        }

        public static void Close(MemoryMappedFile mappedFile)
        {
            mappedFile.Dispose();
        }
    }
}
