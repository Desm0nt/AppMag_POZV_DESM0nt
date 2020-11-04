using MathNet.Numerics.LinearAlgebra.Double;
using MeshGenerator.Elements;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeshGenerator.Utils
{
    /// <summary>
    /// Mathematical and matrix operations
    /// </summary>
    public class MathOps
    {
        /// <summary>
        /// Inverse matrix
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <returns>Inversed matrix</returns>
        public static double[,] Inverse(double[,] A)
        {
            double[,] inv = new double[A.GetLength(0), A.GetLength(1)];
            double[,] Ab = new double[A.GetLength(0), A.GetLength(1)];
            for (int i = 0; i < Ab.GetLength(0); i++)
            {
                for (int j = 0; j < Ab.GetLength(1); j++)
                    Ab[i, j] = A[i, j];
            }
            for (int i = 0; i < inv.GetLength(0); i++)
            {
                inv[i, i] = 1;
            }
            for (int i = 0; i < Ab.GetLength(0); i++)
            {
                if (Math.Abs(Ab[i, i]) < 0.000000000001)
                {
                    double max = 0;
                    int ind = 0;
                    for (int j = i; j < Ab.GetLength(0); j++)
                    {
                        if (Math.Abs(Ab[j, i]) > max)
                        {
                            max = Math.Abs(Ab[j, i]);
                            ind = j;
                        }
                    }
                    if (ind != 0)
                    {
                        for (int j = 0; j < Ab.GetLength(0); j++)
                        {
                            double tmp = 0;
                            tmp = Ab[i, j];
                            Ab[i, j] = Ab[ind, j];
                            Ab[ind, j] = tmp;
                            tmp = inv[i, j];
                            inv[i, j] = inv[ind, j];
                            inv[ind, j] = tmp;
                        }
                    }
                }
                double kaf = Ab[i, i];
                for (int j = 0; j < Ab.GetLength(0); j++)
                {
                    Ab[i, j] = Ab[i, j] / kaf;
                    inv[i, j] = inv[i, j] / kaf;
                }
                for (int j = 0; j < Ab.GetLength(0); j++)
                {
                    if (i != j && Math.Abs(Ab[j, i]) > 0.000000000001)
                    {
                        double lok = Ab[j, i] / Ab[i, i];
                        for (int k = 0; k < Ab.GetLength(0); k++)
                        {
                            Ab[j, k] = Ab[j, k] - Ab[i, k] * lok;
                            inv[j, k] = inv[j, k] - inv[i, k] * lok;
                        }
                    }
                }
            }
            return inv;
        }

        static public double[,] Inv(double[,] A, Tetrahedron tetrahedron)
        {
            double[,] B = new double[A.GetLength(0), A.GetLength(1)];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                B[i, i] = 1;
            }

            for (int i = 0; i < A.GetLength(0); i++)
            {
                if ((Math.Abs(A[i, i]) < 0.000000000001))
                {
                    double maxEl = A[i, i];
                    int ind = 0;
                    for (int j = i + 1; j < A.GetLength(1); j++)
                    {
                        if (Math.Abs(A[j, i]) > maxEl)
                        {
                            maxEl = Math.Abs(A[j, i]);
                            ind = j;
                        }
                    }
                    if (ind != 0)
                    {
                        for (int j = 0; j < A.GetLength(0); j++)
                        {
                            double tmp = 0;
                            tmp = A[i, j];
                            A[i, j] = A[ind, j];
                            A[ind, j] = tmp;
                            tmp = B[i, j];
                            B[i, j] = B[ind, j];
                            B[ind, j] = tmp;
                        }
                    }
                }
                double kaf = A[i, i];
                if (Math.Abs(kaf) < 0.0000000000000001)
                {
                    throw new Exception("Do not get the inversion of this matrix");
                }

                for (int j = 0; j < A.GetLength(0); j++)
                {
                    A[i, j] = A[i, j] / kaf;
                    B[i, j] = B[i, j] / kaf;
                }

                for (int j = 0; j < A.GetLength(0); j++)
                {
                    if (i != j && Math.Abs(A[j, i]) > 0.0000000000000001)
                    {
                        double lokKaf = A[j, i] / A[i, i];
                        for (int k = 0; k < A.GetLength(0); k++)
                        {
                            A[j, k] = A[j, k] - A[i, k] * lokKaf;
                            B[j, k] = B[j, k] - B[i, k] * lokKaf;
                        }
                    }
                }
            }

            return B;
        }
        /// <summary>
        /// Transponating of matrix
        /// </summary>
        /// <param name="B">Matrix</param>
        /// <returns>Transponated matrix</returns>
        public static double[,] Transponate(double[,] B)
        {
            double[,] BT = new double[B.GetLength(1), B.GetLength(0)];
            for (int i = 0; i < B.GetLength(0); i++)
            {
                for (int j = 0; j < B.GetLength(1); j++)
                { BT[j, i] = B[i, j]; }
            }
            return BT;
        }

        /// <summary>
        /// Multiplying of matrices
        /// </summary>
        /// <param name="A">First matrix</param>
        /// <param name="B">Second matrix</param>
        /// <returns>Result of multiplying</returns>
        public static double[,] Multiply(double[,] A, double[,] B)
        {
            double[,] c = new double[A.GetLength(0), B.GetLength(1)];
            for (int i = 0; i < c.GetLength(0); i++)
            {
                for (int j = 0; j < c.GetLength(1); j++)
                {
                    for (int r = 0; r < B.GetLength(0); r++)
                        c[i, j] += A[i, r] * B[r, j];
                }
            }
            return c;
        }

        /// <summary>
        /// Multiply matrix on number
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Number</param>
        /// <returns>Result of multiplying</returns>
        public static double[,] Multiply(double[,] A, double B)
        {
            double[,] C = new double[A.GetLength(0), A.GetLength(1)];
            for (int ii = 0; ii < C.GetLength(0); ii++)
            {
                for (int jj = 0; jj < C.GetLength(1); jj++)
                    C[ii, jj] = A[ii, jj] * B;
            }
            return C;
        }

        /// <summary>
        /// Multiply sparse matrix on number
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Number</param>
        /// <returns>Result of multiplying</returns>
        public static void Multiply(ref SparseMatrix A, double B)
        {
            for (int ii = 0; ii < A.RowCount; ii++)
            {
                for (int jj = 0; jj < A.ColumnCount; jj++)
                    A[ii, jj] = A[ii, jj] * B;
            }
        }

        /// <summary>
        /// Multiplying of matrices
        /// </summary>
        /// <param name="A">Multiplying matrix (multiply A on AT)</param>
        /// <returns>Result of multiplying</returns>
        public static DictionaryMatrix Multiply(ref DictionaryMatrix A)
        {
            DictionaryMatrix C = new DictionaryMatrix(A.Length);

            foreach (var row in A.Rows)
            {
                double value = 0;
                for (int i = 0; i < A.Length; i++)
                {
                    foreach (var column in row.Value.Columns)
                    {
                        value += column.Value * A.GetValue(i, column.Key);
                    }
                    if (Math.Abs(value) > 0.000000000001)
                    {
                        C.SetValue(row.Key, i, value);
                    }
                }
            }
            return C;
        }

        /// <summary>
        /// Multiply matrix on vector
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Vector</param>
        /// <returns>Result of multiplying</returns>
        public static double[] Multiply(double[,] A, double[] B)
        {
            double[] C = new double[A.GetLength(0)];
            for (int ii = 0; ii < A.GetLength(0); ii++)
            {
                for (int jj = 0; jj < A.GetLength(1); jj++)
                    C[ii] += A[ii, jj] * B[jj];
            }
            return C;
        }

        /// <summary>
        /// Multiply vector on matrix
        /// </summary>
        /// <param name="B">Vector</param>
        /// <param name="A">Matrix</param>
        /// <returns></returns>
        public static double[] Multiply(double[] B, double[,] A)
        {
            double[] C = new double[A.GetLength(1)];
            for (int ii = 0; ii < A.GetLength(1); ii++)
            {
                for (int jj = 0; jj < A.GetLength(0); jj++)
                    C[ii] += A[jj, ii] * B[jj];
            }
            return C;
        }

        /// <summary>
        /// Multiply vector on vector
        /// </summary>
        /// <param name="B">First vector</param>
        /// <param name="A">Second vector</param>
        /// <returns>Result of multiplying</returns>
        public static double[] Multiply(double[] B, double[] A)
        {
            double[] C = new double[A.Length];
            for (int ii = 0; ii < C.Length; ii++)
            {
                C[ii] = A[ii] * B[ii];
            }
            return C;
        }

        /// <summary>
        /// Mulptiply vector on number
        /// </summary>
        /// <param name="V">Vector</param>
        /// <param name="per">number</param>
        /// <returns>Result on multiplying</returns>
        public static double[] Multiply(double[] V, double per)
        {
            double[] R = new double[V.Length];
            for (int i = 0; i < R.Length; i++)
                R[i] = V[i] * per;
            return R;
        }

        /// <summary>
        /// Summ of matrices
        /// </summary>
        /// <param name="A">First matrix</param>
        /// <param name="B">Second matrix</param>
        /// <returns>Result of summ</returns>
        public static double[,] Summ(double[,] A, double[,] B)
        {
            double[,] C = new double[A.GetLength(0), A.GetLength(1)];
            for (int ii = 0; ii < A.GetLength(0); ii++)
            {
                for (int jj = 0; jj < A.GetLength(1); jj++)
                    C[ii, jj] = A[ii, jj] + B[ii, jj];
            }
            return C;
        }

        /// <summary>
        /// Substract of marices
        /// </summary>
        /// <param name="A">Initial matrix</param>
        /// <param name="B">Subtractable matrix</param>
        /// <returns>Result of substraction</returns>
        public static double[,] Substract(double[,] A, double[,] B)
        {
            double[,] C = new double[A.GetLength(0), A.GetLength(1)];
            for (int ii = 0; ii < A.GetLength(0); ii++)
            {
                for (int jj = 0; jj < A.GetLength(1); jj++)
                    C[ii, jj] = A[ii, jj] - B[ii, jj];
            }
            return C;
        }

        /// <summary>
        /// Subtract of sparse martices
        /// </summary>
        /// <param name="A">Initial matrix</param>
        /// <param name="B">Subtractable matrix</param>
        /// <returns>Result of substraction</returns>
        public static void Substract(ref SparseMatrix A, SparseMatrix B)
        {
            for (int ii = 0; ii < A.RowCount; ii++)
            {
                for (int jj = 0; jj < A.ColumnCount; jj++)
                    A[ii, jj] = A[ii, jj] - B[ii, jj];
            }
        }
        /// <summary>
        /// Substract vectors
        /// </summary>
        /// <param name="A">Initial vector</param>
        /// <param name="B">Substractable vector</param>
        /// <returns>Result of substraction</returns>
        public static double[] Substract(double[] A, double[] B)
        {
            double[] R = new double[A.Length];
            for (int i = 0; i < R.Length; i++)
                R[i] = A[i] - B[i];
            return R;
        }

        /// <summary>
        /// Summ of vectors
        /// </summary>
        /// <param name="A">First vector</param>
        /// <param name="B">Second vector</param>
        /// <returns>Result of summ</returns>
        public static double[] Summ(double[] A, double[] B)
        {
            double[] R = new double[A.Length];
            for (int i = 0; i < R.Length; i++)
                R[i] = A[i] + B[i];
            return R;
        }

        /// <summary>
        /// Sets all zeros to the selected row
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="rowIndex">Selected row index</param>
        public static void SetZerosRow(ref double[,] A, int rowIndex)
        {
            for (int j = 0; j < A.GetLength(1); j++)
            {
                A[rowIndex, j] = 0;
            }
        }

        /// <summary>
        /// Sets all zeros to the selected row
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="rowIndex">Selected row and column index</param>
        public static void SetZerosRow(ref IntPtr A, int length, int rowIndex)
        {
            for (int j = 0; j < length; j++)
            {
                MemoryMatrix.SetDoubleValue(ref A, rowIndex * length + j, 0);
            }
        }

        /// <summary>
        /// Sets all zeros to the selected row
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="rowIndex">Selected row and column index</param>
        public static void SetZerosRow(MemoryMappedViewAccessor accessor, int length, int rowIndex)
        {
            for (int j = 0; j < length; j++)
            {
                MappedMatrix.SetDoubleValue(accessor, rowIndex, j, length, 0);
            }
        }

        /// <summary>
        /// Sets all zeros to the selected row
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="rowIndex">Selected row and column index</param>
        public static void SetZerosRow(MemoryMappedViewAccessor accessor, int length)
        {
            for (int j = 0; j < length; j++)
            {
                MappedMatrix.SetDoubleValue(accessor, j, 0);
            }
        }

        /// <summary>
        /// Sets all zeros to the selected row
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="rowIndex">Selected row and column index</param>
        public static void SetZerosRow(ref Dictionary<int, IntPtr> A, int rowIndex)
        {
            for (int j = 0; j < A.Count; j++)
            {
                MemoryMatrix.SetDoubleValue(ref A, rowIndex, j, 0);
            }
        }
        /// <summary>
        /// Sets all zeros to the selected row
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="rowIndex">Selected row and column index</param>
        public static void SetZerosRowColumn(ref double[,] A, int rowIndex)
        {
            for (int j = 0; j < A.GetLength(1); j++)
            {
                A[rowIndex, j] = 0;
                A[j, rowIndex] = 0;
            }
        }



        /// <summary>
        /// Sets all zeros to the selected row
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="rowIndex">Selected row and column index</param>
        public static void SetZerosRowColumn(ref DictionaryMatrix A, int rowIndex, int length)
        {
            A.Rows[rowIndex].Columns.Clear();
        }

        /// <summary>
        /// Checks all element in row of matrix are zeros or not.
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="rowIndex">Selected row index</param>
        /// <returns></returns>
        public static bool IsZeroRow(double[,] A, int rowIndex)
        {
            for (int j = 0; j < A.GetLength(1); j++)
            {
                if (A[rowIndex, j] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generate vector non zero rows indexes
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <returns></returns>
        public static LinkedList<int> VectorNonZeroRow(double[,] A)
        {
            LinkedList<int> vector = new LinkedList<int>();
            for (int i = 0; i < A.GetLength(0); i++)
            {
                if (!IsZeroRow(A, i))
                    vector.AddLast(i);
            }
            return vector;
        }

        /// <summary>
        /// Conjugate gradient method
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Vector</param>
        /// <param name="epsilon">accuracy of calculations</param>
        /// <returns>Vector of results</returns>
        public static double[] CGMethod(double[,] A, double[] B, double epsilon)
        {
            int length = B.Length;
            double[] X = new double[B.Length];
            double alpha0 = 0, pq = 0;
            double beta0 = 0;
            double rho0 = 0, rho1 = 0;
            double[] p0 = new double[length];
            double[] p1 = new double[length];
            double[] q0 = new double[length];
            double[] q1 = new double[length];
            double[] r0 = new double[length];
            double[] r1 = new double[length];
            double[] x1 = new double[length];
            double[] z0 = new double[length];
            double[] z1 = new double[length];
            int k = 1;

            double residual = 0;

            for (int i = 0; i < length; i++)
            {
                double sum = 0;

                for (int j = 0; j < length; j++)
                    sum += A[i, j] * X[j];

                r0[i] = sum - B[i];
                residual += r0[i] * r0[i];
            }

            residual = Math.Sqrt(residual / length);

            if (residual < epsilon)
                return X;

            while (true)
            {

                // choose the point-preconditioner 

                for (int i = 0; i < length; i++)
                    z0[i] = r0[i] / A[i, i];

                rho1 = 0;

                for (int i = 0; i < length; i++)
                    rho1 += r0[i] * z0[i];

                if (k == 1)
                {
                    for (int i = 0; i < length; i++)
                        p0[i] = -z0[i];

                    rho0 = rho1;
                }

                else
                {
                    beta0 = rho1 / rho0;

                    for (int i = 0; i < length; i++)
                    {
                        p1[i] = -z0[i] - beta0 * p0[i];
                        p0[i] = p1[i];
                    }

                    rho0 = rho1;
                }

                for (int i = 0; i < length; i++)
                {
                    double sum = 0;

                    for (int j = 0; j < length; j++)
                        sum += A[i, j] * p0[j];

                    q1[i] = sum;
                }

                pq = 0;

                for (int i = 0; i < length; i++)
                    pq += p0[i] * q1[i];

                alpha0 = rho0 / pq;

                if (k == 1)
                {
                    for (int i = 0; i < length; i++)
                        x1[i] = X[i] + alpha0 * p0[i];
                }

                else
                {
                    for (int i = 0; i < length; i++)
                        x1[i] = X[i] + alpha0 * p1[i];
                }

                for (int i = 0; i < length; i++)
                    r1[i] = r0[i] + alpha0 * q1[i];

                residual = 0;

                for (int i = 0; i < length; i++)
                {
                    double sum = 0;

                    for (int j = 0; j < length; j++)
                        sum += A[i, j] * x1[j];

                    r0[i] = sum - B[i];
                    X[i] = x1[i];
                    residual += Math.Pow(r0[i], 2);
                }

                residual /= length;

                if (residual < epsilon)
                    break;

                k++;
            }

            return X;
        }

        /// <summary>
        /// Conjugate gradient method with Jacobi preconditioner (optimized by memory mapped)
        /// </summary>
        /// <param name="accessor"> Accessor to matrix mapped to memory</param>
        /// <param name="B">Vector</param>
        /// <param name="epsilon">accuracy of calculations</param>
        /// <returns>Vector of results</returns>
        public static double[] CGMethod(MemoryMappedViewAccessor accessor, double[] B, double epsilon)
        {
            int length = B.Length;
            double[] X = new double[B.Length];
            double alpha0 = 0, pq = 0;
            double beta0 = 0;
            double rho0 = 0, rho1 = 0;
            double[] p0 = new double[length];
            double[] p1 = new double[length];
            double[] q0 = new double[length];
            double[] q1 = new double[length];
            double[] r0 = new double[length];
            double[] r1 = new double[length];
            double[] x1 = new double[length];
            double[] z0 = new double[length];
            double[] z1 = new double[length];
            int k = 1;

            double residual = 0;

            for (int i = 0; i < length; i++)
            {
                double sum = 0;

                for (int j = 0; j < length; j++)
                    sum += MappedMatrix.GetDoubleValue(accessor, i, j, length) * X[j];

                r0[i] = sum - B[i];
                residual += r0[i] * r0[i];
            }

            residual = Math.Sqrt(residual / length);

            if (residual < epsilon)
                return X;

            while (true)
            {

                // choose the point-preconditioner 

                for (int i = 0; i < length; i++)
                    z0[i] = r0[i] / MappedMatrix.GetDoubleValue(accessor, i, i, length);

                rho1 = 0;

                for (int i = 0; i < length; i++)
                    rho1 += r0[i] * z0[i];

                if (k == 1)
                {
                    for (int i = 0; i < length; i++)
                        p0[i] = -z0[i];

                    rho0 = rho1;
                }

                else
                {
                    beta0 = rho1 / rho0;

                    for (int i = 0; i < length; i++)
                    {
                        p1[i] = -z0[i] - beta0 * p0[i];
                        p0[i] = p1[i];
                    }

                    rho0 = rho1;
                }

                for (int i = 0; i < length; i++)
                {
                    double sum = 0;

                    for (int j = 0; j < length; j++)
                        sum += MappedMatrix.GetDoubleValue(accessor, i, j, length) * p0[j];

                    q1[i] = sum;
                }

                pq = 0;

                for (int i = 0; i < length; i++)
                    pq += p0[i] * q1[i];

                alpha0 = rho0 / pq;

                if (k == 1)
                {
                    for (int i = 0; i < length; i++)
                        x1[i] = X[i] + alpha0 * p0[i];
                }

                else
                {
                    for (int i = 0; i < length; i++)
                        x1[i] = X[i] + alpha0 * p1[i];
                }

                for (int i = 0; i < length; i++)
                    r1[i] = r0[i] + alpha0 * q1[i];

                residual = 0;

                for (int i = 0; i < length; i++)
                {
                    double sum = 0;

                    for (int j = 0; j < length; j++)
                        sum += MappedMatrix.GetDoubleValue(accessor, i, j, length) * x1[j];

                    r0[i] = sum - B[i];
                    X[i] = x1[i];
                    residual += Math.Pow(r0[i], 2);
                }

                residual /= length;

                if (residual < epsilon)
                    break;

                k++;
            }

            return X;
        }

        /// <summary>
        /// Conjugate gradient method with Cholesky preconditioner (optimized by storing in dictionary)
        /// </summary>
        /// <param name="A">Matrix which storing in dictionaries by rows</param>
        /// <param name="B">Vector of results (vector of forces)</param>
        /// <param name="epsilon">accuracy of calculations</param>
        /// <returns>Vector of results</returns>
        public static double[] CGMethod(DictionaryMatrix A, double[] B, double epsilon)
        {
            int length = B.Length;
            double[] X = new double[length];
            double[] r = new double[length];
            double[] p = new double[length];


            double rsold = 0, alpha = 0;
            Array.Copy(B, r, length);
            Array.Copy(B, p, length);

            for (int i = 0; i < length; i++)
            {
                rsold += r[i] * r[i];
            }

            for (int i = 0; i < length; i++)
            {
                double[] Ap = new double[length];

                foreach (var row in A.Rows)
                {
                    foreach (var column in row.Value.Columns)
                    {
                        Ap[row.Key] += column.Value * p[column.Key];
                    }
                }

                double betha = 0;
                for (int k = 0; k < length; k++)
                {
                    betha += p[k] * Ap[k];
                }

                alpha = rsold / betha;

                for (int k = 0; k < length; k++)
                {
                    X[k] += alpha * p[k];
                    r[k] -= alpha * Ap[k];
                }

                double rsnew = 0;

                for (int k = 0; k < length; k++)
                {
                    rsnew += r[k] * r[k];
                }

                if (Math.Sqrt(rsnew) < epsilon)
                {
                    break;
                }

                for (int k = 0; k < length; k++)
                {
                    p[k] = r[k] + (rsnew / rsold) * p[k];
                }

                rsold = rsnew;
            }

            return X;
        }

        /// <summary>
        /// Incomplete Cholesky Factorization
        /// </summary>
        /// <param name="A">Matrix which storing in dictionaries by rows</param>
        public static void IncompleteCholeskyFactorization(ref DictionaryMatrix A)
        {
            int length = A.Length;

            foreach (var row in A.Rows)
            {
                int k = row.Key;
                double diagonalValue = A.GetValue(k, k);

                if (diagonalValue > 0)
                {
                    diagonalValue = Math.Sqrt(diagonalValue);
                }
                else
                {
                    diagonalValue = Math.Sqrt(Math.Abs(diagonalValue));
                }
                A.SetValue(k, k, diagonalValue);

                for (int i = k + 1; i < length; i++)
                {
                    double element = A.GetValue(i, k);
                    if (Math.Abs(element) > 0.0000000001)
                    {
                        A.SetValue(i, k, element / diagonalValue);
                    }
                }


                for (int j = k + 1; j < length; j++)
                {
                    for (int i = j; i < length; i++)
                    {
                        MatrixRow rowTmp = A.GetRow(i);
                        if (Math.Abs(rowTmp.GetValue(j)) > 0.0000000001)
                        {
                            double value = rowTmp.GetValue(j) - rowTmp.GetValue(k) * A.GetValue(j, k);
                            A.SetValue(i, j, value);
                        }
                    }
                }
            }
            foreach (var row in A.Rows)
            {
                foreach (var column in row.Value.Columns)
                {
                    if (row.Key < column.Key)
                    {
                        //A.SetValue(row.Key, column.Key, 0);
                        A.Rows[row.Key].Columns.TryRemove(column.Key, out double value);
                    }
                }
            }
        }

        /// <summary>
        /// Conjugate gradient method with Jacobi preconditioner (optimized by memory mapped)
        /// </summary>
        /// <param name="A">Pathes of matrix's rows</param>
        /// <param name="B">Vector</param>
        /// <param name="epsilon">accuracy of calculations</param>
        /// <returns>Vector of results</returns>
        public static double[] CGMethod(string directoryPath, string[] A, double[] B, double epsilon)
        {
            int length = B.Length;
            double[] X = new double[B.Length];
            double alpha0 = 0, pq = 0;
            double beta0 = 0;
            double rho0 = 0, rho1 = 0;
            double[] p0 = new double[length];
            double[] p1 = new double[length];
            double[] q0 = new double[length];
            double[] q1 = new double[length];
            double[] r0 = new double[length];
            double[] r1 = new double[length];
            double[] x1 = new double[length];
            double[] z0 = new double[length];
            double[] z1 = new double[length];
            int k = 1;

            double residual = 0;

            for (int i = 0; i < length; i++)
            {
                double sum = 0;
                double[] row = new double[length];
                using (var mappedFile = MappedMatrix.Open($"{directoryPath}{A[i]}.matrix", A[i]))
                {
                    using (var accessor = mappedFile.CreateViewAccessor())
                    {
                        row = MappedMatrix.GetDoubleArray(accessor, length);
                    }
                }
                for (int j = 0; j < length; j++)
                {
                    sum += row[j] * X[j];
                }

                r0[i] = sum - B[i];
                residual += r0[i] * r0[i];
            }

            residual = Math.Sqrt(residual / length);

            if (residual < epsilon)
                return X;

            while (true)
            {

                // choose the point-preconditioner 

                for (int i = 0; i < length; i++)
                {
                    double diagonal = 0;
                    using (var mappedFile = MappedMatrix.Open($"{directoryPath}{A[i]}.matrix", A[i]))
                    {
                        using (var accessor = mappedFile.CreateViewAccessor())
                        {
                            diagonal = MappedMatrix.GetDoubleValue(accessor, i);
                        }
                    }
                    z0[i] = r0[i] / diagonal;
                }

                rho1 = 0;

                for (int i = 0; i < length; i++)
                    rho1 += r0[i] * z0[i];

                if (k == 1)
                {
                    for (int i = 0; i < length; i++)
                        p0[i] = -z0[i];

                    rho0 = rho1;
                }
                else
                {
                    beta0 = rho1 / rho0;

                    for (int i = 0; i < length; i++)
                    {
                        p1[i] = -z0[i] - beta0 * p0[i];
                        p0[i] = p1[i];
                    }

                    rho0 = rho1;
                }

                for (int i = 0; i < length; i++)
                {
                    double sum = 0;

                    double[] row = new double[length];
                    using (var mappedFile = MappedMatrix.Open($"{directoryPath}{A[i]}.matrix", A[i]))
                    {
                        using (var accessor = mappedFile.CreateViewAccessor())
                        {
                            row = MappedMatrix.GetDoubleArray(accessor, length);
                        }
                    }
                    for (int j = 0; j < length; j++)
                        sum += row[j] * p0[j];

                    q1[i] = sum;
                }

                pq = 0;

                for (int i = 0; i < length; i++)
                    pq += p0[i] * q1[i];

                alpha0 = rho0 / pq;

                if (k == 1)
                {
                    for (int i = 0; i < length; i++)
                        x1[i] = X[i] + alpha0 * p0[i];
                }
                else
                {
                    for (int i = 0; i < length; i++)
                        x1[i] = X[i] + alpha0 * p1[i];
                }

                for (int i = 0; i < length; i++)
                    r1[i] = r0[i] + alpha0 * q1[i];

                residual = 0;

                for (int i = 0; i < length; i++)
                {
                    double sum = 0;
                    double[] row = new double[length];
                    using (var mappedFile = MappedMatrix.Open($"{directoryPath}{A[i]}.matrix", A[i]))
                    {
                        using (var accessor = mappedFile.CreateViewAccessor())
                        {
                            row = MappedMatrix.GetDoubleArray(accessor, length);
                        }
                    }
                    for (int j = 0; j < length; j++)
                        sum += row[j] * x1[j];

                    r0[i] = sum - B[i];
                    X[i] = x1[i];
                    residual += Math.Pow(r0[i], 2);
                }

                residual /= length;

                if (residual < epsilon)
                    break;

                k++;
            }

            return X;
        }

        /// <summary>
        /// Method of Gauss (diagonal optimized)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Vector</param>
        /// <returns>Vector of results</returns>
        public static double[] MethodOfGauss(double[,] A, double[] B)
        {
            double[] X = new double[B.Length];
            double[,] trA = Transponate(A);

            int rows = A.GetLength(0);
            int columns = A.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                if (Math.Abs(A[i, 0]) < 0.0000000000000001)
                {
                    double maxEl = 0;
                    int ind = 0;
                    for (int j = 0; j < columns; j++)
                    {
                        if (Math.Abs(A[i, j]) > maxEl)
                        {
                            maxEl = Math.Abs(A[i, j]);
                            ind = j + i;  // column index plus row index because j that is diagonal starts from 0
                        }
                        if (Math.Abs(trA[j, i]) > maxEl)
                        {
                            maxEl = Math.Abs(trA[j, i]);
                            ind = j + i;  // column index plus row index because j that is diagonal starts from 0
                        }
                    }

                    if (ind != 0)
                    {
                        double tmp = 0;
                        for (int j = 0; j < columns; j++)
                        {
                            tmp = A[i, j];
                            A[i, j] = A[ind, j];
                            A[ind, j] = tmp;
                        }
                        for (int j = 0; j < rows; j++) // bottom general diagonal
                        {
                            int dif = i - j;
                            if (dif < columns && dif >= 0)
                            {
                                int dind = ind - i;
                                if (dind < columns && dind >= 0)
                                {
                                    tmp = trA[dif, j];
                                    trA[dif, j] = trA[dind, j];
                                    trA[dind, j] = tmp;
                                }
                                else if (dind < 0) trA[dind, j] = 0;
                            }
                            else
                                break;
                        }
                        tmp = B[i];
                        B[i] = B[ind];
                        B[ind] = tmp;
                    }
                }
                int colzeros = columns;// koefficient drop out zeros at end line
                for (int j = i + 1; j < rows; j++)
                {
                    if (j - i < columns)
                    {
                        if (Math.Abs(trA[j - i, i]) > 0.0000000000001 && A[i, 0] > 0.0000000000001)
                        {
                            double lokKaf = trA[j - i, i] / A[i, 0];
                            for (int k = 0; k < colzeros - 1; k++)
                            {
                                A[j, k] -= A[i, k + 1] * lokKaf;
                            }
                            trA[j - i, i] -= A[i, 0] * lokKaf;

                            colzeros--;
                            for (int k = 0; k < rows; k++)
                            {
                                if (j - k < columns)
                                {
                                    if (j - k > 0 && i - k > 0)
                                    {
                                        trA[j - k, k] -= trA[i - k, k] * lokKaf;
                                    }
                                    else
                                        break;
                                }
                            }
                            B[j] -= B[i] * lokKaf;

                        }
                    }
                }
            }

            X = new double[rows];
            for (int i = rows - 1; i >= 0; i--)
            {
                double h = B[i];
                for (int j = 0; j < columns; j++)
                {
                    if (j + i < rows)
                    {
                        h -= X[j + i] * A[i, j];
                    }
                }

                for (int j = 0; j < rows; j++)
                {
                    if (i - j >= 0 && i - j < columns)
                    {
                        h -= X[j] * trA[i - j, j];
                    }
                }
                X[i] = (A[i, 0] > 0.00000000000001) ? h / A[i, 0] : 0;
            }

            return X;
        }

        /// <summary>
        /// Method of Gauss (optimized by memory)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Vector</param>
        /// <returns>Vector of results</returns>
        public static double[] MethodOfGauss(ref IntPtr A, double[] B)
        {
            double[] X = new double[B.Length];

            int length = B.Length;

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(MemoryMatrix.GetDoubleValue(ref A, i * length + i)) < 0.0000000000000001)
                {
                    double maxEl = 0;
                    int ind = 0;
                    for (int j = i; j < length; j++)
                    {
                        if (Math.Abs(MemoryMatrix.GetDoubleValue(ref A, j * length + i)) > maxEl)
                        {
                            maxEl = Math.Abs(MemoryMatrix.GetDoubleValue(ref A, j * length + i));
                            ind = j;
                        }
                    }
                    if (ind != 0)
                    {
                        double tmp = 0;
                        for (int j = 0; j < length; j++)
                        {
                            tmp = MemoryMatrix.GetDoubleValue(ref A, i * length + j);
                            MemoryMatrix.SetDoubleValue(ref A, i * length + j, MemoryMatrix.GetDoubleValue(ref A, ind * length + j));
                            MemoryMatrix.SetDoubleValue(ref A, ind * length + j, tmp);
                        }
                        tmp = B[i];
                        B[i] = B[ind];
                        B[ind] = tmp;
                    }
                }

                for (int j = i + 1; j < length; j++)
                {
                    if (Math.Abs(MemoryMatrix.GetDoubleValue(ref A, j * length + i)) > 0.0000000000001 && MemoryMatrix.GetDoubleValue(ref A, i * length + i) > 0.0000000000001)
                    {
                        double lokKaf =
                            MemoryMatrix.GetDoubleValue(ref A, j * length + i)
                            / MemoryMatrix.GetDoubleValue(ref A, i * length + i);
                        for (int k = 0; k < length; k++)
                        {
                            double tmp = MemoryMatrix.GetDoubleValue(ref A, j * length + k) - MemoryMatrix.GetDoubleValue(ref A, i * length + k) * lokKaf;
                            MemoryMatrix.SetDoubleValue(ref A, j * length + k, tmp);
                        }
                        B[j] = B[j] - B[i] * lokKaf;
                    }
                }
            }

            X = new double[length];
            for (int i = length - 1; i >= 0; i--)
            {
                double h = B[i];
                for (int j = 0; j < length; j++)
                {
                    h = h - X[j] * MemoryMatrix.GetDoubleValue(ref A, i * length + j);
                }
                double diagonalValue = MemoryMatrix.GetDoubleValue(ref A, i * length + i);
                X[i] = (diagonalValue > 0.00000000000001) ? h / diagonalValue : 0;
            }


            return X;
        }

        /// <summary>
        /// Method of Gauss (optimized by memory mapped)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Vector</param>
        /// <returns>Vector of results</returns>
        public static double[] MethodOfGauss(MemoryMappedViewAccessor accessor, double[] B)
        {
            double[] X = new double[B.Length];

            int length = B.Length;

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(MappedMatrix.GetDoubleValue(accessor, i, i, length)) < 0.0000000000000001)
                {
                    double maxEl = 0;
                    int ind = 0;
                    for (int j = i; j < length; j++)
                    {
                        if (Math.Abs(MappedMatrix.GetDoubleValue(accessor, j, i, length)) > maxEl)
                        {
                            maxEl = Math.Abs(MappedMatrix.GetDoubleValue(accessor, j, i, length));
                            ind = j;
                        }
                    }
                    if (ind != 0)
                    {
                        double tmp = 0;
                        for (int j = 0; j < length; j++)
                        {
                            tmp = MappedMatrix.GetDoubleValue(accessor, i, j, length);
                            MappedMatrix.SetDoubleValue(accessor, i, j, length, MappedMatrix.GetDoubleValue(accessor, ind, j, length));
                            MappedMatrix.SetDoubleValue(accessor, ind, j, length, tmp);
                        }
                        tmp = B[i];
                        B[i] = B[ind];
                        B[ind] = tmp;
                    }
                }

                for (int j = i + 1; j < length; j++)
                {
                    if (Math.Abs(MappedMatrix.GetDoubleValue(accessor, j, i, length)) > 0.0000000000001 && MappedMatrix.GetDoubleValue(accessor, i, i, length) > 0.0000000000001)
                    {
                        double lokKaf =
                            MappedMatrix.GetDoubleValue(accessor, j, i, length)
                            / MappedMatrix.GetDoubleValue(accessor, i, i, length);
                        for (int k = 0; k < length; k++)
                        {
                            double tmp = MappedMatrix.GetDoubleValue(accessor, j, k, length) - MappedMatrix.GetDoubleValue(accessor, i, k, length) * lokKaf;
                            MappedMatrix.SetDoubleValue(accessor, j, k, length, tmp);
                        }
                        B[j] = B[j] - B[i] * lokKaf;
                    }
                }
            }

            X = new double[length];
            for (int i = length - 1; i >= 0; i--)
            {
                double h = B[i];
                for (int j = 0; j < length; j++)
                {
                    h = h - X[j] * MappedMatrix.GetDoubleValue(accessor, i, j, length);
                }
                double diagonalValue = MappedMatrix.GetDoubleValue(accessor, i, i, length);
                X[i] = (diagonalValue > 0.00000000000001) ? h / diagonalValue : 0;
            }

            return X;
        }

        /// <summary>
        /// Method of Gauss (optimized by memory mapped)
        /// </summary>
        /// <param name="A">Pathes of matrix's rows</param>
        /// <param name="B">Vector</param>
        /// <returns>Vector of results</returns>
        public static double[] MethodOfGauss(string directoryPath, string[] A, double[] B)
        {
            double[] X = new double[B.Length];

            int length = B.Length;

            for (int i = 0; i < length; i++)
            {
                double diagonalValue = 0;
                using (var mappedFile = MappedMatrix.Open($"{directoryPath}{A[i]}.matrix", A[i]))
                {
                    using (var accessor = mappedFile.CreateViewAccessor())
                    {
                        diagonalValue = MappedMatrix.GetDoubleValue(accessor, i);

                        if (Math.Abs(diagonalValue) < 0.0000000000000001)
                        {
                            double maxEl = diagonalValue;
                            int ind = 0;
                            for (int j = i + 1; j < length; j++)
                            {
                                using (var mmFile = MappedMatrix.Open($"{directoryPath}{A[j]}.matrix", A[j]))
                                {
                                    using (var accessorJ = mmFile.CreateViewAccessor())
                                    {
                                        double tmpValue = MappedMatrix.GetDoubleValue(accessorJ, i);
                                        if (Math.Abs(tmpValue) > maxEl)
                                        {
                                            maxEl = Math.Abs(tmpValue);
                                            ind = j;
                                        }
                                    }
                                }
                            }
                            if (ind != 0)
                            {
                                double tmp = 0;

                                string tmpName = A[i];
                                A[i] = A[ind];
                                A[ind] = tmpName;
                                //using (var mappedInd = MappedMatrix.Open($"{directoryPath}{A[ind]}.matrix", A[ind]))
                                //{
                                //    using (var accesInd = mappedInd.CreateViewAccessor())
                                //    {
                                //        for (int j = 0; j < length; j++)
                                //        {
                                //            tmp = MappedMatrix.GetDoubleValue(accessor, j);
                                //            MappedMatrix.SetDoubleValue(accessor, j, MappedMatrix.GetDoubleValue(accesInd, j));
                                //            MappedMatrix.SetDoubleValue(accesInd, j, tmp);
                                //        }
                                //    }
                                //}

                                tmp = B[i];
                                B[i] = B[ind];
                                B[ind] = tmp;
                            }
                        }

                        double[] rowValues = MappedMatrix.GetDoubleArray(accessor, B.Length);
                        for (int j = i + 1; j < length; j++)
                        {
                            using (var mmFile = MappedMatrix.Open($"{directoryPath}{A[j]}.matrix", A[j]))
                            {
                                using (var accessorJ = mmFile.CreateViewAccessor())
                                {
                                    if (Math.Abs(MappedMatrix.GetDoubleValue(accessorJ, i)) > 0.0000000000001 && Math.Abs(diagonalValue) > 0.0000000000001)
                                    {
                                        double lokKaf =
                                            MappedMatrix.GetDoubleValue(accessorJ, i)
                                            / diagonalValue;
                                        for (int k = 0; k < length; k++)
                                        {
                                            //double tmp = MappedMatrix.GetDoubleValue(accessorJ, k) - MappedMatrix.GetDoubleValue(accessor, k) * lokKaf;
                                            double tmp = MappedMatrix.GetDoubleValue(accessorJ, k) - rowValues[k] * lokKaf;
                                            MappedMatrix.SetDoubleValue(accessorJ, k, tmp);
                                        }

                                        B[j] = B[j] - B[i] * lokKaf;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            X = new double[length];
            for (int i = length - 1; i >= 0; i--)
            {
                using (var mappedFile = MappedMatrix.Open($"{directoryPath}{A[i]}.matrix", A[i]))
                {
                    using (var accessor = mappedFile.CreateViewAccessor())
                    {
                        double h = B[i];
                        for (int j = 0; j < length; j++)
                        {
                            h = h - X[j] * MappedMatrix.GetDoubleValue(accessor, j);
                        }
                        double diagonalValue = MappedMatrix.GetDoubleValue(accessor, i);
                        X[i] = (diagonalValue > 0.00000000000001) ? h / diagonalValue : 0;
                    }
                }
            }

            return X;
        }

        /// <summary>
        /// Method of Gauss (optimized by memory with dictionary)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Vector</param>
        /// <returns>Vector of results</returns>
        public static double[] MethodOfGauss(ref Dictionary<int, IntPtr> A, double[] B)
        {
            double[] X = new double[B.Length];

            int length = B.Length;

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(MemoryMatrix.GetDoubleValue(ref A, i, i)) < 0.0000000000000001)
                {
                    double maxEl = 0;
                    int ind = 0;
                    for (int j = i; j < length; j++)
                    {
                        if (Math.Abs(MemoryMatrix.GetDoubleValue(ref A, j, i)) > maxEl)
                        {
                            maxEl = Math.Abs(MemoryMatrix.GetDoubleValue(ref A, j, i));
                            ind = j;
                        }
                    }
                    if (ind != 0)
                    {
                        double tmp = 0;
                        for (int j = 0; j < length; j++)
                        {
                            tmp = MemoryMatrix.GetDoubleValue(ref A, i, j);
                            MemoryMatrix.SetDoubleValue(ref A, i, j, MemoryMatrix.GetDoubleValue(ref A, ind, j));
                            MemoryMatrix.SetDoubleValue(ref A, ind, j, tmp);
                        }
                        tmp = B[i];
                        B[i] = B[ind];
                        B[ind] = tmp;
                    }
                }

                for (int j = i + 1; j < length; j++)
                {
                    if (Math.Abs(MemoryMatrix.GetDoubleValue(ref A, j, i)) > 0.0000000000001 && MemoryMatrix.GetDoubleValue(ref A, i, i) > 0.0000000000001)
                    {
                        double lokKaf =
                            MemoryMatrix.GetDoubleValue(ref A, j, i)
                            / MemoryMatrix.GetDoubleValue(ref A, i, i);
                        for (int k = 0; k < length; k++)
                        {
                            double tmp = MemoryMatrix.GetDoubleValue(ref A, j, k) - MemoryMatrix.GetDoubleValue(ref A, i, k) * lokKaf;
                            MemoryMatrix.SetDoubleValue(ref A, j, k, tmp);
                        }
                        B[j] = B[j] - B[i] * lokKaf;
                    }
                }
            }

            X = new double[length];
            for (int i = length - 1; i >= 0; i--)
            {
                double h = B[i];
                for (int j = 0; j < length; j++)
                {
                    h = h - X[j] * MemoryMatrix.GetDoubleValue(ref A, i, j);
                }
                double diagonalValue = MemoryMatrix.GetDoubleValue(ref A, i, i);
                X[i] = (diagonalValue > 0.00000000000001) ? h / diagonalValue : 0;
            }


            return X;
        }

        /// <summary>
        /// Method of Gauss. Modificated to work with zeros rows and columns (for symmetric matrix)
        /// </summary>
        /// <param name="K">Matrix</param>
        /// <param name="F">Vector</param>
        /// <param name="vector">Vector non-zero rows</param>
        /// <returns>Vector of results</returns>
        public static double[] MethodOfGauss(double[,] K, double[] F, LinkedList<int> vector)
        {
            double[] vectsil = F;
            double[] R = new double[K.GetLength(0)];
            for (int i = 0; i < K.GetLength(0); i++)
            {
                if (vector.Contains(i))
                {
                    for (int j = i + 1; j < K.GetLength(0); j++)
                    {
                        double lokKaf = K[j, i] / K[i, i];
                        for (int l = 0; l < K.GetLength(0); l++)
                        {
                            K[j, l] = K[j, l] - K[i, l] * lokKaf;
                        }
                        vectsil[j] = vectsil[j] - vectsil[i] * lokKaf;
                    }
                }
            }
            for (int i = K.GetLength(0) - 1; i >= 0; i--)
            {
                if (vector.Contains(i))
                {
                    double rez = vectsil[i];
                    for (int j = 0; j < K.GetLength(0); j++)
                        rez = rez - R[j] * K[i, j];
                    R[i] = rez / K[i, i];
                }
            }
            return R;
        }

        /// <summary>
        /// Method of Gauss (storing non-zero elements of matrix in dictionary)
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Vector</param>
        /// <returns>Vector of results</returns>
        public static double[] MethodOfGauss(DictionaryMatrix A, double[] B)
        {
            A.Rows.Keys.OrderBy(k => k);
            A.Rows.AsParallel()
                .ForAll(row => row.Value.Columns.Keys.OrderBy(k => k));

            double[] X = new double[B.Length];

            int length = B.Length;

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(A.GetValue(i, i)) < 0.0000000000000001)
                {
                    double maxEl = 0;
                    int ind = 0;
                    for (int j = i; j < length; j++)
                    {
                        double element = Math.Abs(A.GetValue(j, i));
                        if (element > maxEl)
                        {
                            maxEl = element;
                            ind = j;
                        }
                    }
                    if (ind != 0)
                    {
                        MatrixRow tmpRow = A.GetRow(i);
                        A.SetRow(i, A.GetRow(ind));
                        A.SetRow(ind, tmpRow);

                        double tmp = 0;
                        tmp = B[i];
                        B[i] = B[ind];
                        B[ind] = tmp;
                    }
                }

                double diagonalValue = A.GetValue(i, i);
                MatrixRow currentRow = A.GetRow(i);
                Parallel.ForEach(A.Rows.Where(r => r.Key > i), row =>
                {
                    double tmpValue = row.Value.GetValue(i);
                    if (Math.Abs(tmpValue) > 0.0000000000001 && Math.Abs(diagonalValue) > 0.0000000000001)
                    {
                        double lokKaf = tmpValue / diagonalValue;
                        int j = row.Key;
                        foreach (var k in A.GetRow(j).Columns.Keys)
                        {
                            A.SetValue(j, k, A.GetValue(j, k) - currentRow.GetValue(k) * lokKaf);
                        }
                        //foreach (var k in currentRow.Columns.Keys)
                        //{
                        //    A.SetValue(j, k, A.GetValue(j, k) - currentRow.GetValue(k) * lokKaf);
                        //}
                        B[row.Key] = B[row.Key] - B[i] * lokKaf;
                    }
                });
                //for (int j = i + 1; j < length; j++)
                //{
                //    if (A.Exists(j, i))
                //    {
                //        double tmpValue = A.GetValue(j, i);
                //        if (Math.Abs(tmpValue) > 0.0000000000001 && Math.Abs(diagonalValue) > 0.0000000000001)
                //        {
                //            double lokKaf = tmpValue / diagonalValue;
                //            for (int k = 0; k < length; k++)
                //            {
                //                if (A.Exists(i, k) /*&& A.Exists(j, k)*/)
                //                {
                //                    A.SetValue(j, k, A.GetValue(j, k) - A.GetValue(i, k) * lokKaf);
                //                }
                //            }
                //            B[j] = B[j] - B[i] * lokKaf;
                //        }
                //    }
                //}
            }

            X = new double[length];
            for (int i = length - 1; i >= 0; i--)
            {
                double h = B[i];
                for (int j = 0; j < length; j++)
                {
                    h = h - X[j] * A.GetValue(i, j);
                }
                double tmpDiagonal = A.GetValue(i, i);
                X[i] = (tmpDiagonal > 0.00000000000001) ? h / tmpDiagonal : 0;
            }

            return X;
        }
        /// <summary>
        /// Method of Gauss
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <param name="B">Vector</param>
        /// <returns>Vector of results</returns>
        public static double[] GausSlau(double[,] A, double[] B)
        {
            double[] X = new double[B.Length];

            int length = B.Length;

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(A[i, i]) < 0.0000000000000001)
                {
                    double maxEl = 0;
                    int ind = 0;
                    for (int j = i; j < length; j++)
                    {
                        if (Math.Abs(A[j, i]) > maxEl)
                        {
                            maxEl = Math.Abs(A[j, i]);
                            ind = j;
                        }
                    }
                    if (ind != 0)
                    {
                        double tmp = 0;
                        for (int j = 0; j < length; j++)
                        {
                            tmp = A[i, j];
                            A[i, j] = A[ind, j];
                            A[ind, j] = tmp;
                        }
                        tmp = B[i];
                        B[i] = B[ind];
                        B[ind] = tmp;
                    }
                }

                for (int j = i + 1; j < length; j++)
                {
                    if (Math.Abs(A[j, i]) > 0.0000000000001 && A[i, i] > 0.0000000000001)
                    {
                        double lokKaf = A[j, i] / A[i, i];
                        for (int k = 0; k < length; k++)
                        {
                            A[j, k] = A[j, k] - A[i, k] * lokKaf;
                        }
                        B[j] = B[j] - B[i] * lokKaf;
                    }
                }
            }

            X = new double[length];
            for (int i = length - 1; i >= 0; i--)
            {
                double h = B[i];
                for (int j = 0; j < length; j++)
                    h = h - X[j] * A[i, j];
                X[i] = (A[i, i] > 0.00000000000001) ? h / A[i, i] : 0;
            }


            return X;
        }

        /// <summary>
        /// Matrix determinant
        /// </summary>
        /// <param name="A">Matrix</param>
        /// <returns>Determinant</returns>
        public static double Det(double[,] A)
        {
            int length = A.GetLength(0) < A.GetLength(1) ? A.GetLength(0) : A.GetLength(1);

            if (length == 1)
                return A[0, 0];

            if (length == 2)
            {
                return A[0, 0] * A[1, 1] - A[0, 1] * A[1, 0];
            }

            if (length == 3)
            {
                double det = A[0, 0] * A[1, 1] * A[2, 2] -
                             A[0, 0] * A[1, 2] * A[2, 1] -
                             A[0, 1] * A[1, 0] * A[2, 2] +
                             A[0, 1] * A[1, 2] * A[2, 0] +
                             A[0, 2] * A[1, 0] * A[2, 1] -
                             A[0, 2] * A[1, 1] * A[2, 0];
                return det;
            }

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(A[i, i]) < 0.000000001)
                {
                    double maxEl = 0;
                    int ind = 0;
                    for (int j = i; j < length; j++)
                    {
                        if (Math.Abs(A[j, i]) > maxEl)
                        {
                            maxEl = Math.Abs(A[j, i]);
                            ind = j;
                        }
                    }
                    if (ind != 0)
                    {
                        double tmp = 0;
                        for (int j = 0; j < length; j++)
                        {
                            tmp = A[i, j];
                            A[i, j] = A[ind, j];
                            A[ind, j] = tmp;
                        }

                    }
                }

                for (int j = i + 1; j < length; j++)
                {
                    if (Math.Abs(A[j, i]) > 0.0000000001)
                    {
                        double lokKaf = A[j, i] / A[i, i];
                        for (int k = 0; k < length; k++)
                        {
                            A[j, k] = A[j, k] - A[i, k] * lokKaf;
                        }

                    }
                }
            }

            double X = 1;

            for (int i = 0; i < length; i++)
                X *= A[i, i];

            return X;
        }

        public static double Integral(double xn, double xk, double epsilon)
        {
            int n = 2;
            var z1 = Integral(xn, xk, n);
            double curEps = 0;
            do
            {
                n *= 2;
                var z2 = Integral(xn, xk, n);
                curEps = Math.Abs(z1 - z2);
                z1 = z2;
            } while (curEps >= epsilon);
            return z1;
        }

        public static double Integral(double xn, double xk, int n = 10)
        {
            double h = (xk - xn) / n;
            double s = xn + xk;
            double step = 0.0;
            for (int j = 0; j < n; j++)
            {
                step += h;
                s += step + (step - h);
            }
            return s * h / 2.0;
        }
    }
}
