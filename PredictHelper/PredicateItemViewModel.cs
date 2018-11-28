using System.Collections.Generic;
using System.Linq;

namespace PredictHelper
{
    public class PredicateItemViewModel : ViewModelBase
    {
        private int _Id;
        private string _Text;
        private ObservableCollectionExt<MappingItemViewModel> _MappingItems;
        private ExistState _ExistState;

        public int Id
        {
            get { return _Id; }
            set
            {
                if (_Id == value)
                    return;
                _Id = value;
                OnPropertyChanged();
            }
        }
        public string Text
        {
            get { return _Text; }
            set
            {
                if (_Text == value)
                    return;
                _Text = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollectionExt<MappingItemViewModel> MappingItems
        {
            get { return _MappingItems; }
            set
            {
                if (_MappingItems == value)
                    return;
                _MappingItems = value;
                OnPropertyChanged();
            }
        }
        public ExistState ExistState
        {
            get { return _ExistState; }
            set
            {
                if (_ExistState == value)
                    return;
                _ExistState = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(ExistStateText));
            }
        }

        public int MappingItemsCount => MappingItems.Where(x => x.ExistState != ExistState.ToBeDeleted).Count();
        public string ExistStateText => ExistState == ExistState.Default ? " "
                                      : ExistState == ExistState.New ? "New"
                                      : ExistState == ExistState.Updated ? "Upd"
                                      : ExistState == ExistState.ToBeDeleted ? "Del"
                                      : throw new System.Exception("Invalid ExisState value");

        public PredicateItemViewModel()
        {
            ExistState = ExistState.Initializing;
            MappingItems = new ObservableCollectionExt<MappingItemViewModel>();

            MappingItems.CollectionChanged += MappingItems_CollectionChanged;
            this.PropertyChanged += PredicateItemViewModel_PropertyChanged;
        }

        private void MappingItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(MappingItems));
            OnPropertyChanged(nameof(MappingItemsCount));
        }

        private void PredicateItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExistState) || e.PropertyName == nameof(ExistStateText))
                return;

            switch (ExistState)
            {
                case ExistState.Initializing:
                    break;
                case ExistState.Default:
                    ExistState = ExistState.Updated;
                    break;
                case ExistState.New:
                    break;
                case ExistState.Updated:
                    break;
                case ExistState.ToBeDeleted:
                    ExistState = ExistState.Updated;
                    break;
                default:
                    ExistState = ExistState.Updated;
                    break;
            }
        }

        public void Process(Dictionary<int, ContentType> ContentTypesDict)
        {
            var processedMappings = GetMappingsForPredicate(ContentTypesDict)
                .ToList();
            var existentMappings = this.MappingItems
                .Select(x => x.ContentTypeId)
                .ToList();

            var removeMappings = new SortedSet<int>(existentMappings.Except(processedMappings));
            var removeMappingItemsVM = this.MappingItems
                .Where(x => removeMappings.Contains(x.ContentTypeId))
                .ToList();

            var terminateItems = removeMappingItemsVM
                .Where(x => x.ExistState == ExistState.New)
                .ToList();
            var toBeDeletedItems = removeMappingItemsVM
                .Where(x => x.ExistState != ExistState.New)
                .ToList();

            foreach (var toBeTerminatedItem in terminateItems)
            {
                toBeTerminatedItem.PropertyChanged -= MappingItem_PropertyChanged;
            }
            this.MappingItems.RemoveRange(terminateItems);

            foreach (var item in toBeDeletedItems)
            {
                item.ExistState = ExistState.ToBeDeleted;
            }

            var overrideMappings = new SortedSet<int>(processedMappings.Intersect(existentMappings));
            var overrideMappingsViewModel = this.MappingItems
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
                .Select(x => new MappingItemViewModel
                {
                    ContentTypeId = x,
                    IsActive = true,
                    ContentTypesDict = ContentTypesDict,
                    ExistState = ExistState.New
                })
                .ToList();

            this.MappingItems.AddRange(newMappingItemsVM);

            foreach (var newMappingItem in newMappingItemsVM)
            {
                newMappingItem.PropertyChanged += MappingItem_PropertyChanged;
            }
        }

        private void MappingItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MappingItemViewModel.ExistState))
            //&& (sender as MappingItemViewModel).ExistState == ExistState.ToBeDeleted)
            {
                OnPropertyChanged(nameof(MappingItemsCount));
            }
        }

        public IEnumerable<int> GetMappingsForPredicate(Dictionary<int, ContentType> ContentTypesDict)
        {
            if (string.IsNullOrEmpty(this.Text))
                return null;

            var result = new List<int>();
            foreach (var item in ContentTypesDict)
            {
                if (item.Value.Name.Contains(this.Text))
                {
                    result.Add(item.Value.Id);
                }
            }
            return result;
        }
    }
}