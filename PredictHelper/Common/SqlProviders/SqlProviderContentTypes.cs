using System;
using System.Collections.Generic;
using System.IO;

namespace PredictHelper
{
    public class SqlProviderContentTypes : SqlProviderBase
    {
        public SqlProviderContentTypes(string connectionString)
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
                        "Predicates.[dbo].[GetContentTypes]",
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

    }
}