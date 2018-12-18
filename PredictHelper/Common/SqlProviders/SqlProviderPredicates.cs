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
        }

        /// <summary>
        /// Сохраняет предикаты, возвращает список Id для новых созданных объектов
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public void SaveEverything(IEnumerable<GroupDtoWithExistState> groups,
                                   IEnumerable<PredicateDtoWithExistState> predicates,
                                   IEnumerable<MappingDtoWithExistState> mappings,
                                   out IEnumerable<int> newlyCreatedPredicateIdList)
        {
            var dtGroupsNew = groups
                .Where(x => x.ExistState == ExistState.New)
                .Select(x => (GroupDto)x)
                .ToDataTable();
            var dtGroupsUpdated = groups
                .Where(x => x.ExistState == ExistState.Updated)
                .Select(x => (GroupDto)x)
                .ToDataTable();
            var dtGroupsToBeDeleted = groups
                .Where(x => x.ExistState == ExistState.ToBeDeleted)
                .Select(x => (GroupDto)x)
                .ToDataTable();
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
                        "Predicates.[dbo].[SaveAll]",
                        0,
                        nameof(SaveEverything),
                        (x) =>
                        {
                            var newPredicateId = x.GetInt32(0);
                            return newPredicateId;
                        },
                        new SqlParameter("@GroupListNew", dtGroupsNew),
                        new SqlParameter("@GroupListUpdated", dtGroupsUpdated),
                        new SqlParameter("@GroupListToBeDeleted", dtGroupsToBeDeleted),
                        new SqlParameter("@PredicateListNew", dtPredicatesNew),
                        new SqlParameter("@PredicateListUpdated", dtPredicatesUpdated),
                        new SqlParameter("@PredicateListToBeDeleted", dtPredicatesToBeDeleted),
                        new SqlParameter("@MappingListNew", dtMappingsNew),
                        new SqlParameter("@MappingListUpdated", dtMappingsUpdated),
                        new SqlParameter("@MappingListToBeDeleted", dtMappingsToBeDeleted)
                    );

                    newlyCreatedPredicateIdList = result;
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
                        "Predicates.[dbo].[GetMappingsForPredicates]",
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
                        "Predicates.[dbo].[GetPredicatesForGroups]",
                        0,
                        nameof(GetPredicates),
                        (x) =>
                        {
                            var ci = new PredicateDto();
                            ci.Guid = x.GetGuid(0);
                            ci.GroupGuid = x.GetGuid(1);
                            ci.Id = x.GetInt32(2);
                            ci.Text = x.GetString(3);
                            ci.SideGroupId = x.IsDBNull(4) ? (byte?)null : x.GetByte(4);
                            ci.AdviceGroupId = x.IsDBNull(5) ? (int?)null : x.GetInt32(5);
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
                        "Predicates.[dbo].[GetGroups]",
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

    }
}