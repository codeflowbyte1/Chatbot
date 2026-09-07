using System;
using System.Collections.Generic;
using System.Data;

namespace ChatbotSqlApi.Helpers
{
    public static class JsonHelper
    {
        /// <summary>
        /// Converts a DataTable to a List of Dictionary (key = column name, value = row value)
        /// preserving exact SQL column names and mapping DBNull to null.
        /// </summary>
        public static List<Dictionary<string, object>> DataTableToList(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            if (table == null)
            {
                return list;
            }

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    object cellValue = row[col];
                    dict[col.ColumnName] = (cellValue == DBNull.Value) ? null : cellValue;
                }
                list.Add(dict);
            }

            return list;
        }
    }
}
