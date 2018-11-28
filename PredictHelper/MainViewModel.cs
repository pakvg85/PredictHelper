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
        public ICommand Command1 => _command1 ?? (_command1 = new RelayCommand(o => Command1Action()));
        private ICommand _command2;
        public ICommand Command2 => _command2 ?? (_command2 = new RelayCommand(o => Command2Action()));
        private ICommand _command3;
        public ICommand Command3 => _command3 ?? (_command3 = new RelayCommand(o => Command3Action(o)));

        public Dictionary<int, ContentType> ContentTypesDict = new Dictionary<int, ContentType>();

        public MainViewModel()
        {
            Predicates = new ObservableCollectionExt<PredicateItemViewModel>();

            try
            {
                var lines = File.ReadLines(@"..\..\..\..\ContentTypesDb.txt");
                ContentTypesDict.Clear();
                foreach (var line in lines)
                {
                    var item = line.Split('\t');
                    var contentTypeId = int.Parse(item[0]);
                    var contentTypeName = item[1];
                    //ContentTypes.Add(new ContentType { Id = contentTypeId, Name = contentTypeName });
                    ContentTypesDict.Add(contentTypeId, new ContentType
                    {
                        Id = contentTypeId,
                        Name = contentTypeName
                    });
                }

                lines = File.ReadLines(@"..\..\..\..\PredicatesInitial.txt");
                Predicates.Clear();
                Predicates.AddRange(lines.Select((x, i) => new PredicateItemViewModel
                {
                    Text = x,
                    //ExistState = ExistState.Default
                }).ToList());

                // TODO: load PredicatesMappingDict
            }
            catch (Exception)
            {
            }

            ButtonPredicatesText = cSavePredicatesText;
            //IsPredicatesEditing = false;
        }

        public void Command1Action()
        {
            if (SelectedIndex < 0)
                return;

            //var predicate = Predicates[SelectedIndex];
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

        public void Command2Action()
        {
        }

        public void Command3Action(object SelectedItems)
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