using System;
using System.Collections.Generic;
using System.Data;

namespace PredictHelper
{
    public partial class CommonExtensions
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            var dataTable = new DataTable();

            if (!typeof(T).IsPrimitive)
            {
                var properties = typeof(T).GetProperties();
                foreach (var info in properties)
                {
                    dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
                }

                foreach (T entity in source)
                {
                    object[] values = new object[properties.Length];
                    for (int i = 0; i < properties.Length; i++)
                    {
                        values[i] = properties[i].GetValue(entity);
                    }

                    dataTable.Rows.Add(values);
                }
            }
            else
            {
                dataTable.Columns.Add("Value", typeof(T));

                foreach (T entity in source)
                {
                    dataTable.Rows.Add(entity);
                }
            }

            return dataTable;
        }
    }
}