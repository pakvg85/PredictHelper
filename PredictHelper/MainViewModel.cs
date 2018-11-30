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

        private ObservableCollectionExt<PredicateItemViewModel> _Predicates;
        private ObservableCollectionExt<MappingItemViewModel> _FoundResults;
        private string _PredicatesMassiveEditButtonText;
        private string _FoundResultsHeader;
        private int _SelectedIndex;

        public Logger GlobalLogger = LogManager.GetCurrentClassLogger();

        public ObservableCollectionExt<PredicateItemViewModel> Predicates
        {
            get { return _Predicates; }
            set
            {
                if (_Predicates == value)
                    return;
                _Predicates = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollectionExt<MappingItemViewModel> FoundResults
        {
            get { return _FoundResults; }
            set
            {
                if (_FoundResults == value)
                    return;
                _FoundResults = value;
                OnPropertyChanged();
            }
        }
        public string PredicatesMassiveEditButtonText
        {
            get { return _PredicatesMassiveEditButtonText; }
            set
            {
                if (_PredicatesMassiveEditButtonText == value)
                    return;
                _PredicatesMassiveEditButtonText = value;
                OnPropertyChanged();
            }
        }
        public string FoundResultsHeader
        {
            get { return _FoundResultsHeader; }
            set
            {
                if (_FoundResultsHeader == value)
                    return;
                _FoundResultsHeader = value;
                OnPropertyChanged();
            }
        }
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                if (_SelectedIndex == value)
                    return;
                _SelectedIndex = value;
                OnPropertyChanged();

                if (value < 0 || value > Predicates.Count - 1)
                {
                    FoundResults = null;
                }
                else
                {
                    var currentPredicate = Predicates.ElementAt(value);
                    FoundResults = currentPredicate.MappingItems;
                }
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

        private string _PredicateGroupId;
        public string PredicateGroupId
        {
            get { return _PredicateGroupId; }
            set
            {
                if (_PredicateGroupId == value)
                    return;
                _PredicateGroupId = value;
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
        public ICommand Command5 => _command5 ?? (_command5 = new RelayCommand(o => DbLoadPredicatesAndMappings()));

        public Dictionary<int, ContentType> ContentTypesDict;

        public MainViewModel()
        {
            Predicates = new ObservableCollectionExt<PredicateItemViewModel>();

            PredicateGroupId = "101";

            try
            {
                //LoadTestData();
                LoadFromDb();
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void ProcessAllPredicates()
        {
            if (SelectedIndex < 0)
                return;

            ProcessMessage("Поиск соответствий для предикатов...");

            try
            {
                foreach (var predicate in Predicates)
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

        private void DbLoadPredicatesAndMappings()
        {
            ProcessMessage("Считывание предикатов из БД...");

            try
            {
                var connectionStringPredicates = File.ReadAllText(@"connectionStringPredicates.config");
                var dbProvider = new DBProviderPredicates(connectionStringPredicates);
                var predicatesDto = dbProvider.GetPredicates(int.Parse(PredicateGroupId));
                var predicatesMappingDto = dbProvider.GetPredicateMappings(predicatesDto.Select(x => x.PredicateId));

                Predicates.Clear();
                Predicates.AddRange(predicatesDto.Select(x => new PredicateItemViewModel
                {
                    Text = x.Text,
                    Id = x.PredicateId
                }).ToList());
                foreach (var predicate in Predicates)
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
                foreach (var predicate in Predicates)
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
                DbLoadPredicatesAndMappings();
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
            var predicatesDtoWithExistState = Predicates
                .Where(x => x.ExistState != ExistState.Default)
                .Select(x => new PredicateDtoWithExistState
                {
                    ExistState = x.ExistState,
                    GroupId = int.Parse(PredicateGroupId),
                    PredicateId = x.Id, // !!!!!!!!!!!! не заполнен для новх записей (ExistState == New) !!!!!!!!!!!!!!
                    Text = x.Text
                });
            IEnumerable<int> newlyCreatedIds = null;
            dbProvider.SavePredicates(predicatesDtoWithExistState, out newlyCreatedIds);
            int iter = -1;
            foreach (var predicate in Predicates)
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
            foreach (var predicate in Predicates)
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

                DbLoadPredicatesAndMappings();
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

                Predicates.RemoveRange(predicatesToBeTerminated);
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

                Predicates.AddRange(newPredicateTexts
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
                //Predicates.Clear();
                Predicates.AddRange(lines.Select((x, i) => new PredicateItemViewModel
                {
                    Text = x
                    //ExistState = ExistState.New
                }).ToList());

                //foreach (var predicate in Predicates)
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