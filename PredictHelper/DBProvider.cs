using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredictHelper
{
    /// <summary>
    /// Контекст Sql запроса
    /// </summary>
    public class SqlExecutionContextInfo
        : AbstractContext
    {
        #region Поля и свойства

        public string ProcName { get; set; }
        public string Caller { get; set; }
        public string Params { get; set; }
        public int Timeout { get; set; }

        #endregion

        public void Read(Exception ex)
        {
            Exception = ex;
        }

        public void Start()
        {
            TimeStarted = DateTime.Now;
        }

        public void Stop()
        {
            TimeFinished = DateTime.Now;
        }
    }

    public abstract class AbstractContext
    {
        #region Поля и свойства

        public Exception Exception { get; protected set; }
        public string Message { get; set; }
        public DateTime? TimeStarted { get; protected set; }
        public DateTime? TimeFinished { get; protected set; }

        #endregion
    }

    /// <summary>
    /// Методы расширения, используемые при работе с SqlClient
    /// </summary>
    public static partial class Extensions
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

    /// <summary>
    /// Класс для выборки данных из БД по указанной строке
    /// </summary>
    public class DBProvider
    {
        private string _connectionString;

        public DBProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<PredicateMappingDTO> GetMappingsForPredicates(IEnumerable<int> predicateIdList)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(string));

            foreach (var item in predicateIdList)
            {
                var r = dt.NewRow();
                r["Id"] = item;
                dt.Rows.Add(r);
            }

            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetMappingsForPredicates]",
                        0,
                        nameof(GetMappingsForPredicates),
                        (x) =>
                        {
                            var PredicateMappingDTO = new PredicateMappingDTO();

                            PredicateMappingDTO.PredicateId = x.GetInt32(0);
                            PredicateMappingDTO.ContentTypeId = x.GetInt32(1);
                            PredicateMappingDTO.IsActive = x.GetBoolean(2);

                            return PredicateMappingDTO;
                        },
                        new SqlParameter("@PredicateIdList", dt));
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public IEnumerable<PredicateDTO> GetPredicatesForGroup(int predicatesGroupId)
        {
            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetPredicatesForGroup]",
                        0,
                        nameof(GetPredicatesForGroup),
                        (x) =>
                        {
                            var PredicateDTO = new PredicateDTO();

                            PredicateDTO.PredicateId = x.GetInt32(0);
                            PredicateDTO.Text = x.GetString(1);
                            PredicateDTO.GroupId = x.GetInt32(2);

                            return PredicateDTO;
                        },
                        new SqlParameter("@GroupId", predicatesGroupId));
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public SqlConnection GetNewConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Получает SQL команду
        /// </summary>
        /// <param name="conn">Соединение</param>
        /// <param name="name">Имя процедуры</param>
        /// <param name="timeout">Таймаут запроса</param>
        /// <param name="prms">Параметры запроса</param>
        /// <returns>Команда SQL</returns>
        protected SqlCommand GetSpCommand(SqlConnection conn, string name, int timeout, params SqlParameter[] prms)
        {
            SqlCommand retval = new SqlCommand()
            {
                Connection = conn,
                CommandText = name,
                CommandTimeout = timeout,
                CommandType = System.Data.CommandType.StoredProcedure
            };

            if (null != prms && prms.Length > 0)
                retval.Parameters.AddRange(prms);

            return retval;
        }

        /// <summary>
        /// Выполняет хранимую процедуру и парсит результат, переданный в резалтсете в виде списка элементов
        /// </summary>
        /// <typeparam name="T">Тип элемента в списке</typeparam>
        /// <param name="conn">Содинение</param>
        /// <param name="name">Имя процедуры</param>
        /// <param name="timeout">Таймаут запроса</param>
        /// <param name="callerName">Имя вызывающего метода</param>
        /// <param name="itemHandler">Обработчик элемента в резалтсете</param>
        /// <param name="prms">Параметры</param>
        /// <returns>Список элементов в резалтсете</returns>
        protected List<T> ExecSpList<T>(SqlConnection conn, string name, int timeout, string callerName, Func<SqlDataReader, T> itemHandler, params SqlParameter[] prms)
        {
            var context = new SqlExecutionContextInfo()
            {
                ProcName = name,
                Caller = callerName,
                Timeout = timeout,
                Params = prms.ToStringEx()
            };
            context.Start();

            List<T> retval = new List<T>();

            try
            {

                using (SqlCommand cmd = GetSpCommand(conn, name, timeout, prms))
                {
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            T item = itemHandler(rdr);
                            retval.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.Read(ex);
                throw;
            }
            finally
            {
                context.Stop();
                //Log(context);
            }

            return retval;
        }
    }
}