using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PredictHelper
{
    public class DBProviderContentTypes : DBProviderBase
    {
        public DBProviderContentTypes(string connectionString)
            : base(connectionString)
        {
        }

        public IEnumerable<ContentTypeDto> GetContentTypes()
        {
            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetContentTypes]",
                        0,
                        nameof(GetContentTypes),
                        (x) =>
                        {
                            var ci = new ContentTypeDto();
                            ci.Id = x.GetInt32(0);
                            ci.Name = x.GetString(1);
                            return ci;
                        }
                    );
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        //public void SaveContentTypes(IEnumerable<ContentTypeDto> list)
        //{
        //    var dt = list.ToDataTable();

        //    using (var conn = GetNewConnection())
        //    {
        //        try
        //        {
        //            conn.Open();

        //            var result = ExecSpList(
        //                conn,
        //                "[dbo].[SaveContentTypes]",
        //                0,
        //                nameof(SaveContentTypes),
        //                (x) => { return true; },
        //                new SqlParameter("@ContentTypeListNew", dt)
        //            );
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }
        //}

    }
}