using System.Collections.Generic;
using System.Linq;

namespace PredictHelper
{
    public class PredicateItem : BaseInpc
    {
        private int _Id;
        private string _Text;
        private ObservableCollectionExt<MappingItem> _MappingItems;
        private ExistState _ExistState;

        public int Id
        {
            get => _Id;
            set => SetField(ref _Id, value);
        }
        public string Text
        {
            get => _Text;
            set => SetField(ref _Text, value);
        }
        public ObservableCollectionExt<MappingItem> MappingItems
        {
            get => _MappingItems;
            set => SetField(ref _MappingItems, value); //OnPropertyChanged("MappingItemsCount");
        }
        public ExistState ExistState
        {
            get => _ExistState;
            set => SetField(ref _ExistState, value);
        }

        public int MappingItemsCount => MappingItems.Where(x => x.ExistState != ExistState.ToBeDeleted).Count();

        public PredicateItem()
        {
            ExistState = ExistState.New;
            MappingItems = new ObservableCollectionExt<MappingItem>();

            MappingItems.CollectionChanged += MappingItems_CollectionChanged;
            this.PropertyChanged += PredicateItem_PropertyChanged;
        }

        private void MappingItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(MappingItems));
            OnPropertyChanged(nameof(MappingItemsCount));
        }

        private void PredicateItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExistState))
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

            //foreach (var toBeTerminatedItem in terminateItems)
            //{
            //    toBeTerminatedItem.PropertyChanged -= MappingItem_PropertyChanged;
            //}
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
                .Select(x => new MappingItem
                {
                    ContentTypeId = x,
                    IsActive = true,
                    ContentTypesDict = ContentTypesDict,
                    ExistState = ExistState.New
                })
                .ToList();

            this.MappingItems.AddRange(newMappingItemsVM);

            //foreach (var newMappingItem in newMappingItemsVM)
            //{
            //    newMappingItem.PropertyChanged += MappingItem_PropertyChanged;
            //}
        }

        // TODO: при изменении статуса маппинга должно изменяться количество у предиката
        //private void MappingItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(MappingItem.ExistState))
        //    //&& (sender as MappingItem).ExistState == ExistState.ToBeDeleted)
        //    {
        //        OnPropertyChanged(nameof(MappingItemsCount));
        //    }
        //}

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