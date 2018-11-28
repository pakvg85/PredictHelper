using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PredictHelper
{
    public class MainViewModel : ViewModelBase
    {
        const int TestPredicateGroupId = 101;
        const string cEditPredicatesText = "Редактировать все предикты";
        const string cSavePredicatesText = "Сохранить";

        private ObservableCollectionExt<PredicateItemViewModel> _Predicates;
        private ObservableCollectionExt<MappingItemViewModel> _FoundResults;
        private string _ButtonPredicatesText;
        private string _FoundResultsHeader;
        private int _SelectedIndex;

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
        public string ButtonPredicatesText
        {
            get { return _ButtonPredicatesText; }
            set
            {
                if (_ButtonPredicatesText == value)
                    return;
                _ButtonPredicatesText = value;
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
                    return;

                var currentPredicate = Predicates.ElementAt(value);

                FoundResults = currentPredicate.MappingItems;
            }
        }

        private ICommand _command1;
        public ICommand Command1 => _command1 ?? (_command1 = new RelayCommand(o => ProcessAllPredicates()));
        private ICommand _command2;
        public ICommand Command2 => _command2 ?? (_command2 = new RelayCommand(o => SaveToDB()));
        private ICommand _command3;
        public ICommand Command3 => _command3 ?? (_command3 = new RelayCommand(o => OnDeleteButtonPressed(o)));

        public Dictionary<int, ContentType> ContentTypesDict;

        public MainViewModel()
        {
            Predicates = new ObservableCollectionExt<PredicateItemViewModel>();

            try
            {
                //var lines = File.ReadLines(@"..\..\..\..\ContentTypesDb.txt");
                //ContentTypesDict = new Dictionary<int, ContentType>();
                //foreach (var line in lines)
                //{
                //    var item = line.Split('\t');
                //    var contentTypeId = int.Parse(item[0]);
                //    var contentTypeName = item[1];
                //    ContentTypesDict.Add(contentTypeId, new ContentType
                //    {
                //        Id = contentTypeId,
                //        Name = contentTypeName
                //    });
                //}

                var connectionStringContentTypes = File.ReadAllText(@"..\..\..\..\connectionStringContentTypes.config");
                var dbContentTypes = new DBProviderContentTypes(connectionStringContentTypes);
                var ContentTypesDict = dbContentTypes.GetContentTypes()
                    .ToDictionary(x => x.Id, i => new ContentType
                    {
                        Id = i.Id,
                        Name = i.Name
                    }
                    );

                //lines = File.ReadLines(@"..\..\..\..\PredicatesInitial.txt");
                //Predicates.Clear();
                //Predicates.AddRange(lines.Select((x, i) => new PredicateItemViewModel
                //{
                //    Text = x,
                //}).ToList());

                var connectionStringPredicates = File.ReadAllText(@"..\..\..\..\connectionStringPredicates.config");
                var dbProvider = new DBProviderPredicates(connectionStringPredicates);
                var predicatesDto = dbProvider.GetPredicates(TestPredicateGroupId);
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

                foreach (var predicate in Predicates)
                {
                    predicate.ExistState = ExistState.Default;
                }
            }
            catch (Exception)
            {
            }

            ButtonPredicatesText = cSavePredicatesText;
        }

        public void ProcessAllPredicates()
        {
            if (SelectedIndex < 0)
                return;

            foreach (var predicate in Predicates)
            {
                //var CancToken = new CancellationToken();
                //Task.Run(() =>
                //{
                predicate.Process(ContentTypesDict);
                //},
                //CancToken);
            }
        }

        public void SaveToDB()
        {
            var connectionString = File.ReadAllText(@"..\..\..\..\connectionString.config");
            var dbProvider = new DBProviderPredicates(connectionString);
            var predicatesDtoWithExistState = Predicates
                .Where(x => x.ExistState != ExistState.Default)
                .Select(x => new PredicateDtoWithExistState
                {
                    ExistState = x.ExistState,
                    GroupId = TestPredicateGroupId,
                    PredicateId = x.Id,
                    Text = x.Text
                });
            dbProvider.SavePredicates(predicatesDtoWithExistState);

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
                        PredicateId = predicate.Id
                    })
                );
            }
            dbProvider.SavePredicateMappings(predicateMappingsDtoWithExistState);
        }

        public void OnDeleteButtonPressed(object SelectedItems)
        {
            var selectedPredicatesViewModels = (SelectedItems as IList)?.OfType<PredicateItemViewModel>();
            if (selectedPredicatesViewModels == null || !selectedPredicatesViewModels.Any())
                return;

            var toBeTerminated = new List<PredicateItemViewModel>();
            foreach (var predicate in selectedPredicatesViewModels)
            {
                if (predicate.ExistState == ExistState.New)
                    toBeTerminated.Add(predicate);
                else
                    predicate.ExistState = ExistState.ToBeDeleted;
            }

            Predicates.RemoveRange(toBeTerminated);
        }
    }
}