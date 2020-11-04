using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshGenerator.Utils
{
    public class MatrixRow
    {
        ConcurrentDictionary<int, double> column = new ConcurrentDictionary<int, double>();

        public ConcurrentDictionary<int, double> Columns => column;
        #region Methods
        public void SetValue(int j, double value)
        {
            column.AddOrUpdate(j, value, (key, val) => value);
            //if (column.ContainsKey(j))
            //{
            //    column[j] = value;
            //}
            //else
            //{
            //    column.Add(j, value);
            //}
        }

        public double GetValue(int j)
        {
            return (column.ContainsKey(j)) ? column[j] : 0.0;
        }
        #endregion
    }
}
