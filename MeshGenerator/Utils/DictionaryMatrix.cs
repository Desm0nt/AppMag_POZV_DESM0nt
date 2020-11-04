using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Utils
{
    public class DictionaryMatrix
    {
        private int length;
        private Dictionary<int, MatrixRow> rows;

        #region Constructors
        public DictionaryMatrix(int length)
        {
            Length = length;
            rows = new Dictionary<int, MatrixRow>(Length);

            for (int i = 0; i < Length; i++)
            {
                rows.Add(i, new MatrixRow());
            }
        }
        #endregion

        #region Properties
        public int Length
        {
            get => length;
            set { length = (value > 0) ? value : 1; }
        }

        public Dictionary<int, MatrixRow> Rows => rows;
        #endregion

        #region Methods
        public void SetValue(int i, int j, double value)
        {
            rows[i].SetValue(j, value);
            //MatrixRow newRow = new MatrixRow();
            //newRow.SetValue(j, value);
            //rows.AddOrUpdate(i, newRow, (key, val) => 
            //{
            //    val.SetValue(j, value);
            //    return val;
            //});
            //if (!rows.ContainsKey(i))
            //{
            //    rows.Add(i, new MatrixRow<T>());
            //}
        }

        public double GetValue(int i, int j)
        {
            return rows[i].GetValue(j);
            //return (rows.ContainsKey(i)) ? rows[i].GetValue(j) : 0.0;
        }

        public bool Exists(int i, int j)
        {
            return rows[i].Columns.ContainsKey(j);
            //if (rows.ContainsKey(i))
            //{
            //    return rows[i].Columns.ContainsKey(j);
            //}
            //else
            //{
            //    return false;
            //}
        }

        public void SetRow(int i, MatrixRow row)
        {
            rows[i] = row;
            //if(rows.ContainsKey(i))
            //{
            //    rows[i] = row;
            //}
            //else
            //{
            //    rows.AddOrUpdate(i, row, (key, val) => row);
            //    rows.Add(i, row);
            //}
        }

        public MatrixRow GetRow(int i)
        {
            return rows[i];
            //return rows.GetOrAdd(i, (key) => new MatrixRow());
            //return (rows.ContainsKey(i)) ? rows[i] : new MatrixRow();
        }
        #endregion
    }
}
