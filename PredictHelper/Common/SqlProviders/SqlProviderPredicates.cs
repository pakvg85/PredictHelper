using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PredictHelper
{
    /// <summary>
    /// Класс для считывания/сохранения предикатов в БД
    /// </summary>
    public class SqlProviderPredicates : SqlProviderBase
    {
        public SqlProviderPredicates(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Сохраняет предикаты, возвращает список Id для новых созданных объектов
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public void SavePredicates(IEnumerable<PredicateDtoWithExistState> list, out IEnumerable<int> newlyCreatedIds)
        {
            var dtNew = list
                .Where(x => x.ExistState == ExistState.New)
                .Select(x => (PredicateDto)x)
                .ToDataTable();
            var dtUpdated = list
                .Where(x => x.ExistState == ExistState.Updated)
                .Select(x => (PredicateDto)x)
                .ToDataTable();
            var dtToBeDeleted = list
                .Where(x => x.ExistState == ExistState.ToBeDeleted)
                .Select(x => (PredicateDto)x)
                .ToDataTable();

            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    var result = ExecSpList(
                        conn,
                        "[dbo].[SavePredicates]",
                        0,
                        nameof(SavePredicates),
                        (x) =>
                        {
                            var newPredicateId = x.GetInt32(0);
                            return newPredicateId;
                        },
                        new SqlParameter("@PredicateListNew", dtNew),
                        new SqlParameter("@PredicateListUpdated", dtUpdated),
                        new SqlParameter("@PredicateListToBeDeleted", dtToBeDeleted)
                    );

                    newlyCreatedIds = result;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void SavePredicateMappings(IEnumerable<MappingDtoWithExistState> list)
        {
            var dtNew = list
                .Where(x => x.ExistState == ExistState.New)
                .Select(x => (MappingDto)x)
                .ToDataTable();
            var dtUpdated = list
                .Where(x => x.ExistState == ExistState.Updated)
                .Select(x => (MappingDto)x)
                .ToDataTable();
            var dtToBeDeleted = list
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
                        "[dbo].[SavePredicateMappings]",
                        0,
                        nameof(SavePredicateMappings),
                        (x) => { return true; },
                        new SqlParameter("@PredicateMappingListNew", dtNew),
                        new SqlParameter("@PredicateMappingListUpdated", dtUpdated),
                        new SqlParameter("@PredicateMappingListToBeDeleted", dtToBeDeleted)
                    );
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public IEnumerable<MappingDto> GetPredicateMappings(IEnumerable<int> predicateIdList)
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
                        nameof(GetPredicateMappings),
                        (x) =>
                        {
                            var ci = new MappingDto();
                            ci.PredicateId = x.GetInt32(0);
                            ci.ContentTypeId = x.GetInt32(1);
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
            using (var conn = GetNewConnection())
            {
                try
                {
                    conn.Open();

                    return ExecSpList(
                        conn,
                        "[dbo].[GetPredicatesForGroup]",
                        0,
                        nameof(GetPredicates),
                        (x) =>
                        {
                            var ci = new PredicateDto();
                            ci.PredicateId = x.GetInt32(0);
                            ci.Text = x.GetString(1);
                            ci.GroupId = x.GetInt32(2);
                            return ci;
                        },
                        new SqlParameter("@GroupId", predicatesGroupId));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public IEnumerable<GroupDto> GetPredicateGroups()
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
                        nameof(GetPredicateGroups),
                        (x) =>
                        {
                            var ci = new GroupDto();
                            ci.GroupId = x.GetInt32(0);
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

    }
}