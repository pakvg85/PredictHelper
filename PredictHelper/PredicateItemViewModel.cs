namespace PredictHelper
{
    public class PredicateItemViewModel : ViewModelBaseWithStore
    {
        public int Id { get { return Get<int>(); } set { Set(value); } }
        public string Text { get { return Get<string>(); } set { Set(value); } }
        public ObservableCollectionExt<MappingItemViewModel> Mapping { get { return Get<ObservableCollectionExt<MappingItemViewModel>>(); } set { Set(value); } }
        public int MatchesCount { get { return Mapping?.Count ?? -1; } private set { } }

        public PredicateItemViewModel()
        {
            Mapping = new ObservableCollectionExt<MappingItemViewModel>();
        }
    }
}