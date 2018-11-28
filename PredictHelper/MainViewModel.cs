using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PredictHelper
{
    public class MainViewModel : ViewModelBaseWithStore
    {
        const string cEditPredicatesText = "Редактировать все предикты";
        const string cSavePredicatesText = "Сохранить";

        private ICommand _command1;
        public ICommand Command1 => _command1 ?? (_command1 = new RelayCommand(o => Command1Action()));
        //private ICommand _command2;
        //public ICommand Command2 => _command2 ?? (_command2 = new RelayCommand(o => EditSingle()));

        //public Dictionary<int, IEnumerable<MappingInfo>> PredicatesMappingDict = new Dictionary<int, IEnumerable<MappingInfo>>(); // Key - PredicateId
        //public Dictionary<MappingKey, MappingInfo> PredicatesMappingDict = new Dictionary<MappingKey, MappingInfo>();
        public Dictionary<int, ContentType> ContentTypesDict = new Dictionary<int, ContentType>();
        public ObservableCollectionExt<PredicateItemViewModel> Predicates { get { return Get<ObservableCollectionExt<PredicateItemViewModel>>(); } set { Set(value); } }
        public ObservableCollectionExt<ContentTypeItemViewModel> FoundResults { get { return Get<ObservableCollectionExt<ContentTypeItemViewModel>>(); } set { Set(value); } }
        public string PredicatesText { get { return Get<string>(); } set { Set(value); } }

        public string ButtonPredicatesText { get { return Get<string>(); } set { Set(value); } }
        public string FoundResultsHeader { get { return Get<string>(); } set { Set(value); } }
        //public bool IsPredicatesEditing { get { return Get<bool>(); } set { Set(value); } }
        public int PredicatesSelection
        {
            get
            {
                return Get<int>();
            }
            set
            {
                Set(value);

                if (value < 0 || value > Predicates.Count - 1)
                    return;

                var currentPredicate = Predicates.ElementAt(value);

                foreach (var foundResult in FoundResults)
                {
                    foundResult.PropertyChanged -= FoundResult_PropertyChanged;
                }

                FoundResults.Clear();
                FoundResults.AddRange(currentPredicate.Mapping
                    .Select(x => new ContentTypeItemViewModel
                    {
                        Id = x.Key.ContentTypeId,
                        Name = ContentTypesDict[x.Key.ContentTypeId].Name,
                        IsActive = x.IsActive
                    }));

                foreach (var foundResult in FoundResults)
                {
                    foundResult.PropertyChanged += FoundResult_PropertyChanged;
                }

                //var CancToken = new CancellationToken();
                //Task.Run(() =>
                //{
                //    foreach (var foundResult in FoundResults)
                //    {
                //        foundResult.PropertyChanged -= FoundResult_PropertyChanged;
                //    }

                //    App.Current.Dispatcher.Invoke(() =>
                //    {
                //        FoundResults.Clear();

                //        FoundResults.AddRange(currentPredicate.Mapping
                //            .Select(x => new ContentTypeItemViewModel
                //            {
                //                Id = x.Key.ContentTypeId,
                //                Name = ContentTypesDict[x.Key.ContentTypeId].Name,
                //                IsActive = x.IsActive
                //            }));
                //    });

                //    foreach (var foundResult in FoundResults)
                //    {
                //        foundResult.PropertyChanged += FoundResult_PropertyChanged;
                //    }
                //},
                //CancToken);
            }
        }

        private void FoundResult_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var foundContentType = (sender as ContentTypeItemViewModel);
            var currentPredicate = Predicates.ElementAt(PredicatesSelection);
            var mappingInfo = currentPredicate.Mapping
                .First(x => x.Key.ContentTypeId == foundContentType.Id);
            mappingInfo.IsActive = foundContentType.IsActive;
        }

        public MainViewModel()
        {
            Predicates = new ObservableCollectionExt<PredicateItemViewModel>();
            FoundResults = new ObservableCollectionExt<ContentTypeItemViewModel>();

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
                    ContentTypesDict.Add(contentTypeId, new ContentType { Id = contentTypeId, Name = contentTypeName });
                }

                lines = File.ReadLines(@"..\..\..\..\PredicatesInitial.txt");
                Predicates.Clear();
                Predicates.AddRange(lines.Select((x, i) => new PredicateItemViewModel { Text = x, Id = i + 101 }).ToList());

                // TODO: load PredicatesMappingDict
            }
            catch (Exception)
            {
            }

            foreach (var predict in Predicates)
            {
                var CancToken = new CancellationToken();
                Task.Run(() =>
                {
                    var mapping = GetMappingByPredict(predict);
                    predict.Mapping.AddRange(mapping);
                },
                CancToken);
            }

            ButtonPredicatesText = cSavePredicatesText;
            //IsPredicatesEditing = false;
        }

        public void Command1Action()
        {
            //IsPredicatesEditing = !IsPredicatesEditing;
            //if (IsPredicatesEditing)
            //{
            //    PredicatesText = string.Join(Environment.NewLine, Predicates.Select(x => x.Value));
            //    ButtonEditPredicatesText = cSavePredicatesText;
            //}
            //else
            //{
            //    Predicates.Clear();
            //    var lines = Regex.Split(PredicatesText, Environment.NewLine);
            //    Predicates.AddRange(lines.Select(x => new PredicateItemViewModel { Value = x }).ToList());
            //    ButtonEditPredicatesText = cEditPredicatesText;
            //}
        }

        public IEnumerable<MappingItemViewModel> GetMappingByPredict(PredicateItemViewModel currentPredicate)
        {
            if (string.IsNullOrEmpty(currentPredicate.Text))
                return null;

            var result = new List<MappingItemViewModel>();
            foreach (var item in ContentTypesDict)
            {
                if (item.Value.Name.Contains(currentPredicate.Text))
                {
                    var newMappingInfo = new MappingItemViewModel
                    {
                        Key = new MappingKey { ContentTypeId = item.Value.Id, PredicateId = currentPredicate.Id },
                        IsActive = true
                    };
                    result.Add(newMappingInfo);
                }
            }
            //if (!PredicatesMappingDict.ContainsKey(currentPredicate.Id))
            //    PredicatesMappingDict.Add(currentPredicate.Id, result);
            return result;
        }
    }
}