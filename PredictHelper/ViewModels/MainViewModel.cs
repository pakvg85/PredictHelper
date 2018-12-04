using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PredictHelper
{
    public class MainViewModel : BaseInpc
    {
        private ObservableCollectionExt<GroupItem> _PredicateGroups;
        private ObservableCollectionExt<PredicateItem> _CurrentPredicates;
        private ObservableCollectionExt<MappingItem> _CurrentPredicateMappings;
        private PredicateItem _CurrentPredicate;

        public Logger GlobalLogger = LogManager.GetCurrentClassLogger();

        public ObservableCollectionExt<GroupItem> PredicateGroups
        {
            get => _PredicateGroups;
            set => SetField(ref _PredicateGroups, value);
        }
        public ObservableCollectionExt<PredicateItem> CurrentPredicates
        {
            get => _CurrentPredicates;
            set => SetField(ref _CurrentPredicates, value);
        }
        public ObservableCollectionExt<MappingItem> CurrentPredicateMappings
        {
            get => _CurrentPredicateMappings;
            set => SetField(ref _CurrentPredicateMappings, value);
        }
        public PredicateItem CurrentPredicate
        {
            get => _CurrentPredicate;
            set
            {
                SetField(ref _CurrentPredicate, value);
                CurrentPredicateMappings = _CurrentPredicate?.MappingItems;
            }
        }

        private string _StatusBarText;
        public string StatusBarText
        {
            get => _StatusBarText;
            set => SetField(ref _StatusBarText, value);
        }

        private GroupItem _CurrentPredicateGroup;
        public GroupItem CurrentPredicateGroup
        {
            get => _CurrentPredicateGroup;
            set => SetField(ref _CurrentPredicateGroup, value);
        }

        private ICommand _command1;
        public ICommand Command1 => _command1 ?? (_command1 = new RelayCommand(o => ProcessAllPredicates()));
        private ICommand _command2;
        public ICommand Command2 => _command2 ?? (_command2 = new RelayCommand(o => DbSave()));
        private ICommand _command3;
        public ICommand Command3 => _command3 ?? (_command3 = new RelayCommand(o => OnDeleteButtonPressed(o)));
        private ICommand _command4;
        public ICommand Command4 => _command4 ?? (_command4 = new RelayCommand(o => ShowDialogAddPredicates()));
        private ICommand _command5;
        public ICommand Command5 => _command5 ?? (_command5 = new RelayCommand(o => DbLoadPredicatesAndMappings(CurrentPredicateGroup.Id)));

        public Dictionary<int, ContentType> ContentTypesDict;

        public MainViewModel()
        {
            PredicateGroups = new ObservableCollectionExt<GroupItem>();
            CurrentPredicates = new ObservableCollectionExt<PredicateItem>();

            try
            {
                DbLoad();
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void ProcessAllPredicates()
        {
            ProcessMessage("Поиск соответствий для предикатов...");

            try
            {
                foreach (var predicate in CurrentPredicates)
                {
                    predicate.Process(ContentTypesDict);
                }

                ProcessMessage("Поиск соответствий для предикатов завершен");
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void DbLoadContentTypes()
        {
            ProcessMessage("Считывание справочника из БД...");

            try
            {
                var connectionStringContentTypes = File.ReadAllText(@"connectionStringContentTypes.config");
                var dbContentTypes = new SqlProviderContentTypes(connectionStringContentTypes);

                ContentTypesDict = dbContentTypes.GetContentTypes()
                    .ToDictionary(x => x.Id, i => new ContentType
                    {
                        Id = i.Id,
                        Name = i.Name
                    }
                    );
                ProcessMessage("Считывание справочника из БД завершено");
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void DbLoadPredicateGroups()
        {
            ProcessMessage("Считывание групп предикатов из БД...");

            try
            {
                var connectionStringPredicates = File.ReadAllText(@"connectionStringPredicates.config");
                var SqlProvider = new SqlProviderPredicates(connectionStringPredicates);
                var predicateGroupsDto = SqlProvider.GetPredicateGroups();

                PredicateGroups.Clear();
                PredicateGroups.AddRange(predicateGroupsDto.Select(x => new GroupItem
                {
                    Text = x.Text,
                    Id = x.GroupId
                }).ToList());
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void DbLoadPredicatesAndMappings(int predicateGroupId)
        {
            ProcessMessage("Считывание предикатов из БД...");

            try
            {
                var connectionStringPredicates = File.ReadAllText(@"connectionStringPredicates.config");
                var SqlProvider = new SqlProviderPredicates(connectionStringPredicates);
                var predicatesDto = SqlProvider.GetPredicates(predicateGroupId);
                var predicatesMappingDto = SqlProvider.GetPredicateMappings(predicatesDto.Select(x => x.PredicateId));

                CurrentPredicates.Clear();
                CurrentPredicates.AddRange(predicatesDto.Select(x => new PredicateItem
                {
                    Text = x.Text,
                    Id = x.PredicateId
                }).ToList());
                foreach (var predicate in CurrentPredicates)
                {
                    var mappingList = predicatesMappingDto
                        .Where(x => x.PredicateId == predicate.Id)
                        .Select(x => new MappingItem
                        {
                            ContentTypesDict = ContentTypesDict,
                            ContentTypeId = x.ContentTypeId,
                            IsActive = x.IsActive,
                            ExistState = ExistState.Default
                        });
                    predicate.MappingItems.AddRange(mappingList);
                }

                // Выставление статуса Default делается из-за того что ранее был вызов MappingItems.AddRange()
                foreach (var predicate in CurrentPredicates)
                {
                    predicate.ExistState = ExistState.Default;
                }
                ProcessMessage("Считывание предикатов из БД завершено");
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void DbLoad()
        {
            ProcessMessage("Считывание данных из БД...");

            try
            {
                DbLoadContentTypes();
                DbLoadPredicateGroups();
                CurrentPredicateGroup = PredicateGroups.First();
                DbLoadPredicatesAndMappings(CurrentPredicateGroup.Id);
                ProcessMessage("Считывание данных из БД завершено");
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void DbSavePredicates()
        {
            var connectionString = File.ReadAllText(@"connectionStringPredicates.config");
            var SqlProvider = new SqlProviderPredicates(connectionString);
            var predicatesDtoWithExistState = CurrentPredicates
                .Where(x => x.ExistState != ExistState.Default)
                .Select(x => new PredicateDtoWithExistState
                {
                    ExistState = x.ExistState,
                    GroupId = CurrentPredicateGroup.Id,
                    PredicateId = x.Id, // !!!!!!!!!!!! не заполнен для новх записей (ExistState == New) !!!!!!!!!!!!!!
                    Text = x.Text
                });
            IEnumerable<int> newlyCreatedIds = null;
            SqlProvider.SavePredicates(predicatesDtoWithExistState, out newlyCreatedIds);
            int iter = -1;
            foreach (var predicate in CurrentPredicates)
            {
                if (predicate.ExistState != ExistState.New)
                    continue;

                iter++;
                predicate.Id = newlyCreatedIds.ElementAt(iter);
            }
        }

        private void DbSaveMappings()
        {
            var connectionString = File.ReadAllText(@"connectionStringPredicates.config");
            var SqlProvider = new SqlProviderPredicates(connectionString);
            var predicateMappingsDtoWithExistState = new List<MappingDtoWithExistState>();
            foreach (var predicate in CurrentPredicates)
            {
                predicateMappingsDtoWithExistState.AddRange(predicate.MappingItems
                    .Where(x => x.ExistState != ExistState.Default)
                    .Select(x => new MappingDtoWithExistState
                    {
                        ExistState = x.ExistState,
                        ContentTypeId = x.ContentTypeId,
                        IsActive = x.IsActive,
                        PredicateId = predicate.Id // !!!!!!!!!!!! не заполнен для новх записей (ExistState == New) !!!!!!!!!!!!!!
                    })
                );
            }
            SqlProvider.SavePredicateMappings(predicateMappingsDtoWithExistState);
        }

        //private void DbSaveContentTypes()
        //{
        //    var connectionString = File.ReadAllText(@"connectionStringContentTypes.config");
        //    var SqlProvider = new SqlProviderContentTypes(connectionString);
        //    var contentTypesDto = ContentTypesDict
        //        .Select(x => new ContentTypeDto
        //        {
        //            Id = x.Key,
        //            Name = x.Value.Name
        //        });
        //    SqlProvider.SaveContentTypes(contentTypesDto);
        //}

        private void DbSave()
        {
            ProcessMessage("Сохранение данных в БД...");

            try
            {
                DbSavePredicates();
                DbSaveMappings();
                //DbSaveContentTypes();

                ProcessMessage("Сохранение данных в БД завершено");

                DbLoadPredicatesAndMappings(CurrentPredicateGroup.Id);
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void OnDeleteButtonPressed(object SelectedItems)
        {
            var selectedPredicatesViewModels = (SelectedItems as IList)?.OfType<PredicateItem>();
            if (selectedPredicatesViewModels == null || !selectedPredicatesViewModels.Any())
                return;

            try
            {
                var predicatesToBeTerminated = new List<PredicateItem>();
                foreach (var predicate in selectedPredicatesViewModels)
                {
                    if (predicate.ExistState == ExistState.New)
                        predicatesToBeTerminated.Add(predicate);
                    else
                        predicate.ExistState = ExistState.ToBeDeleted;

                    var mappingsToBeTerminated = new List<MappingItem>();
                    foreach (var mappingItem in predicate.MappingItems)
                    {
                        if (mappingItem.ExistState == ExistState.New)
                            mappingsToBeTerminated.Add(mappingItem);
                        else
                            mappingItem.ExistState = ExistState.ToBeDeleted;
                    }
                    predicate.MappingItems.RemoveRange(mappingsToBeTerminated);
                }

                CurrentPredicates.RemoveRange(predicatesToBeTerminated);
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void LoadTestData()
        {
            try
            {
                LoadContentTypesFromFile();
                LoadTestDataFromFile();
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void ShowDialogAddPredicates()
        {
            var dialog = new MessageBoxDialog.MessageBoxDialogWindow();
            if (dialog.ShowDialog() == true)
            {
                var newPredicateTexts = Regex.Split(dialog.ResponseText, Environment.NewLine);

                CurrentPredicates.AddRange(newPredicateTexts
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => new PredicateItem { Text = x }));
            }
        }

        private void LoadTestDataFromFile()
        {
            ProcessMessage("Считывание новых предикатов из локального файла...");

            try
            {
                var lines = File.ReadLines(@"PredicatesInitial.txt");
                CurrentPredicates.AddRange(lines.Select((x, i) => new PredicateItem
                {
                    Text = x
                }).ToList());

                ProcessMessage("Считывание новых предикатов из локального файла завершено");
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void LoadContentTypesFromFile()
        {
            ProcessMessage("Считывание справочника из локального файла...");
            try
            {
                var lines = File.ReadLines(@"ContentTypesDb.txt");
                ContentTypesDict = new Dictionary<int, ContentType>();
                foreach (var line in lines)
                {
                    var item = line.Split('\t');
                    var contentTypeId = int.Parse(item[0]);
                    var contentTypeName = item[1];
                    ContentTypesDict.Add(contentTypeId, new ContentType
                    {
                        Id = contentTypeId,
                        Name = contentTypeName
                    });
                }

                ProcessMessage("Считывание справочника из локального файла завершено");
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        public void ProcessException(Exception ex, bool isFatal = false)
        {
            ProcessMessage(ex.Message + " (детальное инфо об ошибке - в файле лога в папке с программой)");

            if (isFatal)
                GlobalLogger.Fatal(ex, "UNHANDLED EXCEPTION");
            else
                GlobalLogger.Error(ex);
        }

        public void ProcessMessage(string msg)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StatusBarText = msg;
            });
        }
    }
}