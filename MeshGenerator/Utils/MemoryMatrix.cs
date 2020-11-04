using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Utils
{
    public class MemoryMatrix
    {

        public static void Init(ref IntPtr A, int length)
        {
            //A = Marshal.AllocCoTaskMem(sizeof(double) * length * length);
            A = Marshal.AllocHGlobal(sizeof(double) * length * length);
        }

        public static void Init(ref IntPtr A, ulong length)
        {
            ulong size = sizeof(double) * length * length;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ulong)));
            Marshal.StructureToPtr(size, ptr, false);
            A = Marshal.AllocHGlobal(ptr);
        }

        public static void Init(ref Dictionary<int, IntPtr> A, int length)
        {
            A = new Dictionary<int, IntPtr>(length);
            //var hHeap = Heap.HeapCreate(Heap.HeapFlags.HEAP_GENERATE_EXCEPTIONS, 0, sizeof(double) * (uint)length * (uint)length);
            for (int i = 0; i < length; i++)
            {
                A.Add(i, Marshal.AllocHGlobal(sizeof(double) * length));
                //A.Add(i, Marshal.AllocCoTaskMem(sizeof(double) * length));
                //A.Add(i, Heap.HeapAlloc(hHeap, 0, sizeof(double) * (uint)length));
            }
        }

        public static double GetDoubleValue(ref IntPtr A, int index)
        {
            double[] temp = new double[1];
            Marshal.Copy(A + index * sizeof(double), temp, 0, 1);
            return temp[0];
        }

        public static double GetDoubleValue(ref IntPtr A, int i, int j, int length)
        {
            double[] temp = new double[1];
            IntPtr ptr = IntPtr.Add(A, 0);
            for (int ind = 0; ind < i; ind++)
            {
                ptr = IntPtr.Add(ptr, length * sizeof(double));
            }
            ptr = IntPtr.Add(ptr, j * sizeof(double));
            Marshal.Copy(ptr, temp, 0, 1);
            return temp[0];
        }

        public static double GetDoubleValue(ref Dictionary<int, IntPtr> A, int i, int j)
        {
            double[] temp = new double[1];
            Marshal.Copy(A[i] + j * sizeof(double), temp, 0, 1);
            return temp[0];
        }

        public static void SetDoubleValue(ref IntPtr A, int index, double value)
        {
            double[] temp = new double[] { value };
            Marshal.Copy(temp, 0, A + index * sizeof(double), 1);
        }

        public static void SetDoubleValue(ref IntPtr A, int i, int j, int length, double value)
        {
            double[] temp = new double[] { value };

            IntPtr ptr = IntPtr.Add(A, 0);
            for (int ind = 0; ind < i; ind++)
            {
                ptr = IntPtr.Add(ptr, length * sizeof(double));
            }
            ptr = IntPtr.Add(ptr, j * sizeof(double));
            Marshal.Copy(temp, 0, ptr, 1);
        }

        public static void SetDoubleValue(ref Dictionary<int, IntPtr> A, int i, int j, double value)
        {
            double[] temp = new double[] { value };

            Marshal.Copy(temp, 0, A[i] + j * sizeof(double), 1);
        }

        public static void Clear(ref IntPtr A)
        {
            Marshal.FreeHGlobal(A);
        }

        public static void Clear(ref Dictionary<int, IntPtr> A)
        {
            for (int i = 0; i < A.Count; i++)
            {
                Marshal.FreeHGlobal(A[i]);
            }
        }
    }
}
