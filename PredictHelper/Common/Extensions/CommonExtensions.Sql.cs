using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace PredictHelper
{
    /// <summary>
    /// Методы расширения, используемые при работе с SqlClient
    /// </summary>
    public static partial class CommonExtensions
    {
        /// <summary>
        /// Подрезает строку до необходимого количества символов
        /// </summary>
        /// <param name="src">Исходная строка</param>
        /// <param name="maxlength">Максимальная длинна строки</param>
        /// <param name="suffix">Добавляемый в конце суффикс</param>
        /// <returns></returns>
        public static string TrimLength(this string src, int maxlength, string suffix = null)
        {
            if (String.IsNullOrEmpty(src))
                return String.Empty;
            else if (src.Length < maxlength)
                return src;
            else
                return String.Format("{0}{1}", src.Substring(0, maxlength), suffix);
        }

        /// <summary>
        /// Расширение. Строковое представление коллекции SQL параметров
        /// </summary>
        /// <param name="src">Исходная коллекция параметров</param>
        /// <param name="maxlength">Масимальная длина сохраняемого значения</param>
        /// <returns>Строковое представление коллекции параметров SQL</returns>
        public static string ToStringEx(this IEnumerable<SqlParameter> src, int maxlength = 256)
        {
            if (null != src && src.Count() > 0)
            {
                List<string> prms =
                    src.Select(a => String.Format("{0}={1}", a.ParameterName, null == a.Value ? "NULL" : a.Value.ToString().TrimLength(maxlength, "...")))
                    .ToList();

                return String.Join("&", prms);

            }
            else
                return null;
        }
    }
}