using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace PredictHelper
{
    /// <summary>
    /// Класс для считывания/сохранения предикатов в БД
    /// </summary>
    public class SqlProviderPredicates : SqlProviderBase
    {
        public SqlProviderPredicates(string connectionString = null)
            : base(connectionString)
        {
            var connectionStringDef = File.ReadAllText(@"connectionStringPredicates.config");
            _connectionString = connectionStringDef;
        }

        /// <summary>
        /// Сохраняет предикаты, возвращает список Id для новых созданных объектов
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public void SavePredicatesAndMappings(IEnumerable<PredicateDtoWithExistState> predicates, IEnumerable<MappingDtoWithExistState> mappings, out IEnumerable<int> newlyCreatedIds)
        {
            var dtPredicatesNew = predicates
                .Where(x => x.ExistState == ExistState.New)
                .Select(x => (PredicateDto)x)
                .ToDataTable();
            var dtPredicatesUpdated = predicates
                .Where(x => x.ExistState == ExistState.Updated)
                .Select(x => (PredicateDto)x)
                .ToDataTable();
            var dtPredicatesToBeDeleted = predicates
                .Where(x => x.ExistState == ExistState.ToBeDeleted)
                .Select(x => (PredicateDto)x)
                .ToDataTable();
            var dtMappingsNew = mappings
                .Where(x => x.ExistState == ExistState.New)
                .Select(x => (MappingDto)x)
                .ToDataTable();
            var dtMappingsUpdated = mappings
                .Where(x => x.ExistState == ExistState.Updated)
                .Select(x => (MappingDto)x)
                .ToDataTable();
            var dtMappingsToBeDeleted = mappings
                .Where(x => x.ExistState == ExistState.ToBeDeleted)
                .Select(x => (MappingDto)x)
                .ToDataTable();

            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    var result = ExecSpList(
                        conn,
                        "[dbo].[SavePredicatesAndMappings]",
                        0,
                        nameof(SavePredicatesAndMappings),
                        (x) =>
                        {
                            var newPredicateId = x.GetInt32(0);
                            return newPredicateId;
                        },
                        new SqlParameter("@PredicateListNew", dtPredicatesNew),
                        new SqlParameter("@PredicateListUpdated", dtPredicatesUpdated),
                        new SqlParameter("@PredicateListToBeDeleted", dtPredicatesToBeDeleted),
                        new SqlParameter("@MappingListNew", dtMappingsNew),
                        new SqlParameter("@MappingListUpdated", dtMappingsUpdated),
                        new SqlParameter("@MappingListToBeDeleted", dtMappingsToBeDeleted)
                    );

                    newlyCreatedIds = result;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public IEnumerable<MappingDto> GetMappings(IEnumerable<int> predicateIdList)
        {
            var dt = predicateIdList.ToDataTable();

            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetMappingsForPredicates]",
                        0,
                        nameof(GetMappings),
                        (x) =>
                        {
                            var ci = new MappingDto();
                            ci.ContentTypeId = x.GetInt32(0);
                            ci.PredicateGuid = x.GetGuid(1);
                            ci.IsActive = x.GetBoolean(2);
                            return ci;
                        },
                        new SqlParameter("@PredicateIdList", dt));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public IEnumerable<PredicateDto> GetPredicates(int predicatesGroupId)
        {
            return GetPredicates(new List<int> { predicatesGroupId });
        }

        public IEnumerable<PredicateDto> GetPredicates(IEnumerable<int> predicatesGroupIdList)
        {
            var dt = predicatesGroupIdList.ToDataTable();

            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetPredicatesForGroups]",
                        0,
                        nameof(GetPredicates),
                        (x) =>
                        {
                            var ci = new PredicateDto();
                            ci.Guid = x.GetGuid(0);
                            ci.GroupGuid = x.GetGuid(1);
                            ci.Id = x.GetInt32(2);
                            ci.Text = x.GetString(3);
                            return ci;
                        },
                        new SqlParameter("@GroupIdList", dt)
                    );
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public IEnumerable<GroupDto> GetGroups()
        {
            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetPredicateGroups]",
                        0,
                        nameof(GetGroups),
                        (x) =>
                        {
                            var ci = new GroupDto();
                            ci.Guid = x.GetGuid(0);
                            ci.Id = x.GetInt32(1);
                            ci.Text = x.GetString(2);
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

        // ====================================================================================

        public IEnumerable<MappingDto> GetMappingsTmp(int predicateId)
        {
            return GetMappingsTmp(new List<int> { predicateId });
        }

        public IEnumerable<MappingDto> GetMappingsTmp(IEnumerable<int> predicateIdList)
        {
            var dt = predicateIdList.ToDataTable();

            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetMappingsForPredicatesTmp]",
                        0,
                        nameof(GetMappingsTmp),
                        (x) =>
                        {
                            var ci = new MappingDto();
                            ci.ContentTypeId = x.GetInt32(0);
                            ci.IsActive = x.GetBoolean(2);
                            return ci;
                        },
                        new SqlParameter("@PredicateIdList", dt));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public IEnumerable<PredicateDto> GetPredicatesTmp(int predicatesGroupId)
        {
            return GetPredicatesTmp(new List<int> { predicatesGroupId });
        }

        public IEnumerable<PredicateDto> GetPredicatesTmp(IEnumerable<int> predicatesGroupIdList)
        {
            var dt = predicatesGroupIdList.ToDataTable();

            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetPredicatesForGroupsTmp]",
                        0,
                        nameof(GetPredicates),
                        (x) =>
                        {
                            var ci = new PredicateDto();
                            ci.Id = x.GetInt32(1);
                            ci.Text = x.GetString(2);
                            return ci;
                        },
                        new SqlParameter("@GroupIdList", dt)
                    );
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public IEnumerable<GroupDto> GetGroupsTmp()
        {
            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetPredicateGroupsTmp]",
                        0,
                        nameof(GetGroups),
                        (x) =>
                        {
                            var ci = new GroupDto();
                            ci.Id = x.GetInt32(0);
                            ci.Text = x.GetString(1);
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

        // ====================================================================================

    }
}