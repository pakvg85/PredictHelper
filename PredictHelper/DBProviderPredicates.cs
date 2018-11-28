﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredictHelper
{
    /// <summary>
    /// Класс для считывания/сохранения предикатов в БД
    /// </summary>
    public class DBProviderPredicates : DBProviderBase
    {
        public DBProviderPredicates(string connectionString)
            : base(connectionString)
        {
        }

        public void SavePredicates(IEnumerable<PredicateDtoWithExistState> list)
        {
            var dtNew = list
                .Where(x => x.ExistState == ExistState.New)
                .Select(x => (PredicateDto)x)
                .CreateDataTable();
            var dtUpdated = list
                .Where(x => x.ExistState == ExistState.Updated)
                .Select(x => (PredicateDto)x)
                .CreateDataTable();
            var dtToBeDeleted = list
                .Where(x => x.ExistState == ExistState.ToBeDeleted)
                .Select(x => (PredicateDto)x)
                .CreateDataTable();

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
                        (x) => { return true; },
                        new SqlParameter("@PredicateListNew", dtNew),
                        new SqlParameter("@PredicateListUpdated", dtUpdated),
                        new SqlParameter("@PredicateListToBeDeleted", dtToBeDeleted)
                    );
                }
                catch (Exception)
                {
                }
            }
        }

        public void SavePredicateMappings(IEnumerable<PredicateMappingDtoWithExistState> list)
        {
            var dtNew = list
                .Where(x => x.ExistState == ExistState.New)
                .Select(x => (PredicateMappingDto)x)
                .CreateDataTable();
            var dtUpdated = list
                .Where(x => x.ExistState == ExistState.Updated)
                .Select(x => (PredicateMappingDto)x)
                .CreateDataTable();
            var dtToBeDeleted = list
                .Where(x => x.ExistState == ExistState.ToBeDeleted)
                .Select(x => (PredicateMappingDto)x)
                .CreateDataTable();

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
                        new SqlParameter("@PredicateMappingListDeleted", dtToBeDeleted)
                    );
                }
                catch (Exception)
                {
                }
            }
        }

        public IEnumerable<PredicateMappingDto> GetPredicateMappings(IEnumerable<int> predicateIdList)
        {
            var dt = predicateIdList.CreateDataTable();

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
                            var ci = new PredicateMappingDto();
                            ci.PredicateId = x.GetInt32(0);
                            ci.ContentTypeId = x.GetInt32(1);
                            ci.IsActive = x.GetBoolean(2);
                            return ci;
                        },
                        new SqlParameter("@PredicateIdList", dt));
                }
                catch (Exception)
                {
                    return null;
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
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}