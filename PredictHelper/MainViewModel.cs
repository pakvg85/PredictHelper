using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PredictHelper
{
    public class MainViewModel : ViewModelBase
    {
        const string PredicatesMassiveEditTextEdit = "Редактировать все предикты";
        const string PredicatesMassiveEditTextApply = "Применить";

        private ObservableCollectionExt<PredicateGroupViewModel> _PredicateGroups;
        private ObservableCollectionExt<PredicateItemViewModel> _CurrentPredicates;
        private ObservableCollectionExt<MappingItemViewModel> _CurrentPredicateMappings;
        private PredicateItemViewModel _CurrentPredicate;

        public Logger GlobalLogger = LogManager.GetCurrentClassLogger();

        public ObservableCollectionExt<PredicateGroupViewModel> PredicateGroups
        {
            get { return _PredicateGroups; }
            set
            {
                if (_PredicateGroups == value)
                    return;
                _PredicateGroups = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollectionExt<PredicateItemViewModel> CurrentPredicates
        {
            get { return _CurrentPredicates; }
            set
            {
                if (_CurrentPredicates == value)
                    return;
                _CurrentPredicates = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollectionExt<MappingItemViewModel> CurrentPredicateMappings
        {
            get { return _CurrentPredicateMappings; }
            set
            {
                if (_CurrentPredicateMappings == value)
                    return;
                _CurrentPredicateMappings = value;
                OnPropertyChanged();
            }
        }
        public PredicateItemViewModel CurrentPredicate
        {
            get
            {
                return _CurrentPredicate;
            }
            set
            {
                if (_CurrentPredicate == value)
                    return;
                _CurrentPredicate = value;
                OnPropertyChanged();

                CurrentPredicateMappings = _CurrentPredicate?.MappingItems;
            }
        }

        private string _StatusBarText;
        public string StatusBarText
        {
            get { return _StatusBarText; }
            set
            {
                if (_StatusBarText == value)
                    return;
                _StatusBarText = value;
                OnPropertyChanged();
            }
        }

        private PredicateGroupViewModel _CurrentPredicateGroup;
        public PredicateGroupViewModel CurrentPredicateGroup
        {
            get { return _CurrentPredicateGroup; }
            set
            {
                if (_CurrentPredicateGroup == value)
                    return;
                _CurrentPredicateGroup = value;
                OnPropertyChanged();
            }
        }

        private ICommand _command1;
        public ICommand Command1 => _command1 ?? (_command1 = new RelayCommand(o => ProcessAllPredicates()));
        private ICommand _command2;
        public ICommand Command2 => _command2 ?? (_command2 = new RelayCommand(o => SaveToDB()));
        private ICommand _command3;
        public ICommand Command3 => _command3 ?? (_command3 = new RelayCommand(o => OnDeleteButtonPressed(o)));
        private ICommand _command4;
        public ICommand Command4 => _command4 ?? (_command4 = new RelayCommand(o => ShowDialogAddPredicates()));
        private ICommand _command5;
        public ICommand Command5 => _command5 ?? (_command5 = new RelayCommand(o => DbLoadPredicatesAndMappings(CurrentPredicateGroup.Id)));

        public Dictionary<int, ContentType> ContentTypesDict;

        public MainViewModel()
        {
            PredicateGroups = new ObservableCollectionExt<PredicateGroupViewModel>();
            CurrentPredicates = new ObservableCollectionExt<PredicateItemViewModel>();

            try
            {
                LoadFromDb();
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
                    //var CancToken = new CancellationToken();
                    //Task.Run(() =>
                    //{
                    predicate.Process(ContentTypesDict);
                    //},
                    //CancToken);
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
                var dbContentTypes = new DBProviderContentTypes(connectionStringContentTypes);

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

        private void DBLoadPredicateGroups()
        {
            ProcessMessage("Считывание групп предикатов из БД...");

            try
            {
                var connectionStringPredicates = File.ReadAllText(@"connectionStringPredicates.config");
                var dbProvider = new DBProviderPredicates(connectionStringPredicates);
                var predicateGroupsDto = dbProvider.GetPredicateGroups();

                PredicateGroups.Clear();
                PredicateGroups.AddRange(predicateGroupsDto.Select(x => new PredicateGroupViewModel
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
                var dbProvider = new DBProviderPredicates(connectionStringPredicates);
                var predicatesDto = dbProvider.GetPredicates(predicateGroupId);
                var predicatesMappingDto = dbProvider.GetPredicateMappings(predicatesDto.Select(x => x.PredicateId));

                CurrentPredicates.Clear();
                CurrentPredicates.AddRange(predicatesDto.Select(x => new PredicateItemViewModel
                {
                    Text = x.Text,
                    Id = x.PredicateId
                }).ToList());
                foreach (var predicate in CurrentPredicates)
                {
                    var mappingList = predicatesMappingDto
                        .Where(x => x.PredicateId == predicate.Id)
                        .Select(x => new MappingItemViewModel
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

        private void LoadFromDb()
        {
            ProcessMessage("Считывание данных из БД...");

            try
            {
                DbLoadContentTypes();
                DBLoadPredicateGroups();
                CurrentPredicateGroup = PredicateGroups.First();
                DbLoadPredicatesAndMappings(CurrentPredicateGroup.Id);
                ProcessMessage("Считывание данных из БД завершено");
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void DBSavePredicates()
        {
            var connectionString = File.ReadAllText(@"connectionStringPredicates.config");
            var dbProvider = new DBProviderPredicates(connectionString);
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
            dbProvider.SavePredicates(predicatesDtoWithExistState, out newlyCreatedIds);
            int iter = -1;
            foreach (var predicate in CurrentPredicates)
            {
                if (predicate.ExistState != ExistState.New)
                    continue;

                iter++;
                predicate.Id = newlyCreatedIds.ElementAt(iter);
            }
        }

        private void DBSaveMappings()
        {
            var connectionString = File.ReadAllText(@"connectionStringPredicates.config");
            var dbProvider = new DBProviderPredicates(connectionString);
            var predicateMappingsDtoWithExistState = new List<PredicateMappingDtoWithExistState>();
            foreach (var predicate in CurrentPredicates)
            {
                predicateMappingsDtoWithExistState.AddRange(predicate.MappingItems
                    .Where(x => x.ExistState != ExistState.Default)
                    .Select(x => new PredicateMappingDtoWithExistState
                    {
                        ExistState = x.ExistState,
                        ContentTypeId = x.ContentTypeId,
                        IsActive = x.IsActive,
                        PredicateId = predicate.Id // !!!!!!!!!!!! не заполнен для новх записей (ExistState == New) !!!!!!!!!!!!!!
                    })
                );
            }
            dbProvider.SavePredicateMappings(predicateMappingsDtoWithExistState);
        }

        //private void DBSaveContentTypes()
        //{
        //    var connectionString = File.ReadAllText(@"connectionStringContentTypes.config");
        //    var dbProvider = new DBProviderContentTypes(connectionString);
        //    var contentTypesDto = ContentTypesDict
        //        .Select(x => new ContentTypeDto
        //        {
        //            Id = x.Key,
        //            Name = x.Value.Name
        //        });
        //    dbProvider.SaveContentTypes(contentTypesDto);
        //}

        private void SaveToDB()
        {
            ProcessMessage("Сохранение данных в БД...");

            try
            {
                DBSavePredicates();
                DBSaveMappings();
                //DBSaveContentTypes();

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
            var selectedPredicatesViewModels = (SelectedItems as IList)?.OfType<PredicateItemViewModel>();
            if (selectedPredicatesViewModels == null || !selectedPredicatesViewModels.Any())
                return;

            try
            {
                var predicatesToBeTerminated = new List<PredicateItemViewModel>();
                foreach (var predicate in selectedPredicatesViewModels)
                {
                    if (predicate.ExistState == ExistState.New)
                        predicatesToBeTerminated.Add(predicate);
                    else
                        predicate.ExistState = ExistState.ToBeDeleted;

                    var mappingsToBeTerminated = new List<MappingItemViewModel>();
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
                    .Select(x => new PredicateItemViewModel { Text = x }));
            }
        }

        private void LoadTestDataFromFile()
        {
            ProcessMessage("Считывание новых предикатов из локального файла...");

            try
            {
                var lines = File.ReadLines(@"PredicatesInitial.txt");
                //CurrentPredicates.Clear();
                CurrentPredicates.AddRange(lines.Select((x, i) => new PredicateItemViewModel
                {
                    Text = x
                    //ExistState = ExistState.New
                }).ToList());

                //foreach (var predicate in CurrentPredicates)
                //{
                //    predicate.ExistState = ExistState.Default;
                //}

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