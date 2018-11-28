using System;
using System.Collections.Generic;

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
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}