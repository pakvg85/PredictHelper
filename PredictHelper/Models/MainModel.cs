using PredictHelper.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PredictHelper.Models
{
    public class MainModel : BaseInpc
    {
        private ObservableCollectionExt<GroupItem> _GroupItems;
        private SqlProviderPredicates _sqlProviderPredicates;
        private SqlProviderContentTypes _sqlProviderContentTypes;

        public Dictionary<int, ContentType> ContentTypesDict { get; set; }
        public Dictionary<int, AdviceGroupItem> AdviceGroupItemsDict { get; set; }

        public ObservableCollectionExt<GroupItem> GroupItems { get => _GroupItems; set => SetField(ref _GroupItems, value); }

        public MainModel(string ConnectionPredicates, string ConnectionContentTypes)
        {
            GroupItems = new ObservableCollectionExt<GroupItem>();
            _sqlProviderPredicates = new SqlProviderPredicates(ConnectionPredicates);
            _sqlProviderContentTypes = new SqlProviderContentTypes(ConnectionContentTypes);
        }

        public void DbLoad(bool loadEverything = true)
        {
            ProcessMessage("Считывание данных из БД...");

            try
            {
                if (loadEverything)
                {
                    DbLoadContentTypes();
                    DbLoadAdviceGroups();
                }

                DbLoadPredicateGroups();

                ProcessMessage("Считывание данных из БД завершено");
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void DbLoadContentTypes()
        {
            ContentTypesDict = _sqlProviderContentTypes.GetContentTypes()
                .ToDictionary(x => x.Id, i => new ContentType
                {
                    Id = i.Id,
                    Name = i.Name
                }
            );
        }

        private void DbLoadAdviceGroups()
        {
            AdviceGroupItemsDict = _sqlProviderPredicates.GetAllAdvices()
                .ToDictionary(x => x.Id, i => new AdviceGroupItem
                {
                    Id = i.Id,
                    PredicateGroupId = i.PredicateGroupId,
                    ShortDescription = i.ShortDescription
                }
            );
        }

        private void DbLoadPredicateGroups()
        {
            var GroupsDto = _sqlProviderPredicates.GetGroups();
            var groupItems = GroupsDto.Select(x => new GroupItem
            {
                Guid = x.Guid,
                Id = x.Id,
                Text = x.Text
            });
            GroupItems.Clear();
            GroupItems.AddRange(groupItems);

            DbLoadPredicates(GroupItems);
        }

        private void DbLoadPredicates(IEnumerable<GroupItem> groupList)
        {
            var predicatesDto = _sqlProviderPredicates.GetPredicates(groupList.Select(x => x.Id));

            foreach (var groupItem in groupList)
            {
                groupItem.PredicateItems.Clear();
                groupItem.PredicateItems.AddRange(predicatesDto
                    .Where(x => x.GroupGuid == groupItem.Guid)
                    .Select(x => new PredicateItem
                    {
                        Guid = x.Guid,
                        Id = x.Id,
                        GroupGuid = x.GroupGuid,
                        Text = x.Text,
                        //SideGroupId = x.SideGroupId,
                        SideGroup = (x.SideGroupId == null) ? (SideGroup?)null
                                  : (x.SideGroupId == 1) ? SideGroup.Claimant
                                  : (x.SideGroupId == 2) ? SideGroup.Defendant
                                  : (x.SideGroupId == 3) ? SideGroup.ThirdParty
                                  : SideGroup.Undefined,
                        AdviceGroupId = x.AdviceGroupId,
                        AdviceGroupItemsDict = AdviceGroupItemsDict,
                        ExistState = ExistState.Default
                    })
                    .ToList()
                );

                DbLoadMappings(groupItem.PredicateItems);
            }
        }

        private void DbLoadMappings(IEnumerable<PredicateItem> predicateList)
        {
            var predicatesMappingDto = _sqlProviderPredicates.GetMappings(predicateList.Select(x => x.Id));

            foreach (var predicate in predicateList)
            {
                var savedPredicateExistState = predicate.ExistState; // загрузка маппингов из БД не должна влиять на сам предикат
                var mappingList = predicatesMappingDto
                    .Where(x => x.PredicateGuid == predicate.Guid)
                    .Select(x => new MappingItem
                    {
                        ContentTypeId = x.ContentTypeId,
                        PredicateGuid = x.PredicateGuid,
                        IsActive = x.IsActive,
                        ContentTypesDict = ContentTypesDict, // ссылка на ContentTypeDict нужна чтобы формировать поле Name
                        ExistState = ExistState.Default
                    });
                predicate.MappingItems.Clear();
                predicate.MappingItems.AddRange(mappingList);
                predicate.ExistState = savedPredicateExistState;
            }
        }

        public void DbSave()
        {
            ProcessMessage("Сохранение данных в БД...");

            try
            {
                var groupsDtoWithExistState = GroupItems
                    .Where(x => x.ExistState != ExistState.Default)
                    .Select(x => new GroupDtoWithExistState
                    {
                        Guid = x.Guid,
                        Id = x.Id, // Id не нужен для сохранения - связка с предикатами сделана через Guid
                        Text = x.Text,
                        ExistState = x.ExistState
                    });

                var predicatesDtoWithExistState = new List<PredicateDtoWithExistState>();
                var mappingsDtoWithExistState = new List<MappingDtoWithExistState>();

                foreach (var groupItem in GroupItems)
                {
                    var groupPredicatesDtoWithExistState = groupItem.PredicateItems
                        .Where(x => x.ExistState != ExistState.Default)
                        .Select(x => new PredicateDtoWithExistState
                        {
                            Guid = x.Guid,
                            GroupGuid = x.GroupGuid,
                            Id = x.Id, // Id не нужен для сохранения - связка с маппингами сделана через Guid
                            Text = x.Text,
                            //SideGroupId = x.SideGroupId,
                            SideGroupId = (byte)x.SideGroup,
                            AdviceGroupId = x.AdviceGroupId,
                            ExistState = x.ExistState
                        });
                    predicatesDtoWithExistState.AddRange(groupPredicatesDtoWithExistState);

                    foreach (var predicate in groupItem.PredicateItems)
                    {
                        var predicateMappingsDtoWithExistState = predicate.MappingItems
                            .Where(x => x.ExistState != ExistState.Default)
                            .Select(x => new MappingDtoWithExistState
                            {
                                ContentTypeId = x.ContentTypeId,
                                PredicateGuid = predicate.Guid,
                                IsActive = x.IsActive,
                                ExistState = x.ExistState
                            });
                        mappingsDtoWithExistState.AddRange(predicateMappingsDtoWithExistState);
                    }

                }

                _sqlProviderPredicates.SaveEverything(groupsDtoWithExistState,
                                                      predicatesDtoWithExistState,
                                                      mappingsDtoWithExistState,
                                                      out var newlyCreatedPredicateIdList);

                ProcessMessage("Сохранение данных в БД завершено");

                DbLoad(false);
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        public void ProcessPredicates(GroupItem singleGroupItem = null)
        {
            ProcessMessage("Поиск соответствий для предикатов...");

            try
            {
                if (singleGroupItem == null)
                {
                    foreach (var groupItem in GroupItems)
                    {
                        foreach (var predicate in groupItem.PredicateItems)
                        {
                            Process(predicate, ContentTypesDict);
                        }
                    }
                }
                else
                {
                    foreach (var predicate in singleGroupItem.PredicateItems)
                    {
                        Process(predicate, ContentTypesDict);
                    }
                }

                ProcessMessage("Поиск соответствий для предикатов завершен");
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        public void Process(PredicateItem predicate, Dictionary<int, ContentType> ContentTypesDict)
        {
            var processedMappings = new List<int>();
            foreach (var item in ContentTypesDict)
            {
                if (item.Value.Name.Contains(predicate.Text))
                {
                    processedMappings.Add(item.Value.Id);
                }
            }

            var existentMappings = predicate.MappingItems
                .Select(x => x.ContentTypeId)
                .ToList();

            var removeMappings = new SortedSet<int>(existentMappings.Except(processedMappings));
            var removeMappingItemsVM = predicate.MappingItems
                .Where(x => removeMappings.Contains(x.ContentTypeId))
                .ToList();

            var terminateItems = removeMappingItemsVM
                .Where(x => x.ExistState == ExistState.New)
                .ToList();
            var toBeDeletedItems = removeMappingItemsVM
                .Where(x => x.ExistState != ExistState.New)
                .ToList();

            predicate.MappingItems.RemoveRange(terminateItems);

            foreach (var item in toBeDeletedItems)
            {
                item.ExistState = ExistState.ToBeDeleted;
            }

            var overrideMappings = new SortedSet<int>(processedMappings.Intersect(existentMappings));
            var overrideMappingsViewModel = predicate.MappingItems
                .Where(x => overrideMappings.Contains(x.ContentTypeId))
                .ToList();
            foreach (var item in overrideMappingsViewModel)
            {
                if (item.ExistState == ExistState.ToBeDeleted)
                    item.ExistState = ExistState.Updated;
            }

            var newMappings = processedMappings.Except(existentMappings)
                .ToList();
            var newMappingItemsVM = newMappings
                .Select(x => new MappingItem
                {
                    ContentTypeId = x,
                    PredicateGuid = predicate.Guid,
                    IsActive = true,
                    ContentTypesDict = ContentTypesDict,
                    ExistState = ExistState.New
                })
                .ToList();

            predicate.MappingItems.AddRange(newMappingItemsVM);
        }

        public event EventHandler<MessageOccuredEventArgs> EventMessageOccured;
        public void ProcessException(Exception ex, MessageImportance msgImportance = MessageImportance.Error)
        {
            //ProcessMessage(ex.Message + " (детальное инфо об ошибке - в файле лога в папке с программой)", msgImportance);
            EventMessageOccured?.Invoke(this, new MessageOccuredEventArgs(ex.Message + " (детальное инфо об ошибке - в файле лога в папке с программой)", msgImportance, ex));
        }
        public void ProcessMessage(string msg, MessageImportance msgImportance = MessageImportance.Info)
        {
            EventMessageOccured?.Invoke(this, new MessageOccuredEventArgs(msg, msgImportance));
        }

    }
}