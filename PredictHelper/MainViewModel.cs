using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PredictHelper
{
    public class MainViewModel : ViewModelBaseWithStore
    {
        const string cEditPredicatesText = "Редактировать все предикты";
        const string cSavePredicatesText = "Сохранить все предикты";

        private ICommand _command1;
        private ICommand _command2;

        public ICommand Command1 => _command1 ?? (_command1 = new RelayCommand(o => EditAll()));
        public ICommand Command2 => _command2 ?? (_command2 = new RelayCommand(o => EditSingle()));

        public List<ContentType> ContentTypesDb;
        public ObservableCollectionExt<PredicateItemViewModel> Predicates { get { return Get<ObservableCollectionExt<PredicateItemViewModel>>(); } set { Set(value); } }
        public ObservableCollectionExt<FoundResultItemViewModel> FoundResults { get { return Get<ObservableCollectionExt<FoundResultItemViewModel>>(); } set { Set(value); } }
        public string PredicatesText { get { return Get<string>(); } set { Set(value); } }

        public string ButtonEditPredicatesText { get { return Get<string>(); } set { Set(value); } }
        public string FoundResultsHeader { get { return Get<string>(); } set { Set(value); } }
        public bool IsPredicatesEditing { get { return Get<bool>(); } set { Set(value); } }
        private int _currentEditing;
        public int PredicatesSelection
        {
            get
            {
                return Get<int>();
            }
            set
            {
                Set(value);
                var CancToken = new CancellationToken();
                Task.Run(() =>
                    SearchByPredict(Predicates.ElementAt(value).Value),
                    CancToken);
            }
        }

        public MainViewModel()
        {
            Predicates = new ObservableCollectionExt<PredicateItemViewModel>();
            FoundResults = new ObservableCollectionExt<FoundResultItemViewModel>();
            ContentTypesDb = new List<ContentType>();

            var lines = File.ReadLines(@"..\..\..\..\ContentTypesDb.txt");
            ContentTypesDb.Clear();
            foreach (var line in lines)
            {
                var item = line.Split('\t');
                ContentTypesDb.Add(new ContentType { Id = int.Parse(item[0]), Name = item[1] });
            }

            lines = File.ReadLines(@"..\..\..\..\PredicatesInitial.txt");
            Predicates.Clear();
            Predicates.AddRange(lines.Select(x => new PredicateItemViewModel { Value = x }).ToList());

            ButtonEditPredicatesText = cEditPredicatesText;
            IsPredicatesEditing = false;
        }

        public void EditAll()
        {
            IsPredicatesEditing = !IsPredicatesEditing;
            if (IsPredicatesEditing)
            {
                PredicatesText = string.Join(Environment.NewLine, Predicates.Select(x => x.Value));
                ButtonEditPredicatesText = cSavePredicatesText;
            }
            else
            {
                Predicates.Clear();
                var lines = Regex.Split(PredicatesText, Environment.NewLine);
                Predicates.AddRange(lines.Select(x => new PredicateItemViewModel { Value = x }).ToList());
                ButtonEditPredicatesText = cEditPredicatesText;
            }
        }

        public void EditSingle()
        {
            if (!Predicates.ElementAt(PredicatesSelection).IsEditing)
            {
                _currentEditing = PredicatesSelection;
                var item = Predicates.ElementAt(_currentEditing);
                item.IsEditing = !item.IsEditing;
                item.IsFocused = true;
            }
            else
            {
                Predicates.ElementAt(_currentEditing).IsEditing = !Predicates.ElementAt(_currentEditing).IsEditing;
                _currentEditing = -1;
            }
        }

        public void SearchByPredict(string currentPredict)
        {
            if (string.IsNullOrEmpty(currentPredict))
            {
                FoundResultsHeader = "Предикат должен быть не пустым";
                return;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                FoundResults.Clear();
            });

            FoundResultsHeader = "Поиск...";
            var result = new List<FoundResultItemViewModel>();
            int foundResults = 0;
            foreach (var item in ContentTypesDb)
            {
                if (item.Name.Contains(currentPredict))
                {
                    result.Add(new FoundResultItemViewModel { Value = item.Name, IsActive = true });
                    foundResults++;
                }
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                FoundResults.AddRange(result);
            });

            if (!FoundResults.Any())
                FoundResultsHeader = "Совпадений не найдено";
            else
                FoundResultsHeader = $"Найдено совпадений: {foundResults.ToString()}";
        }
    }
}