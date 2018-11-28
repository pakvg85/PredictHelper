using System;
using System.Collections.Generic;
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

    public class DBProviderBase
    {
        protected string _connectionString;

        public DBProviderBase(string connectionString)
        {
            _connectionString = connectionString;
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