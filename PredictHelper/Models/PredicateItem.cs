using System;
using System.Linq;

namespace PredictHelper
{
    public class PredicateItem : BaseInpc
    {
        private int _Id;
        private string _Text;
        private byte? _SideGroupId;
        private int? _AdviceGroupId;
        private ObservableCollectionExt<MappingItem> _MappingItems;
        private ExistState _ExistState;

        public int Id { get => _Id; set => SetField(ref _Id, value); }
        public string Text { get => _Text; set => SetField(ref _Text, value); }
        public byte? SideGroupId { get => _SideGroupId; set => SetField(ref _SideGroupId, value); }
        public int? AdviceGroupId { get => _AdviceGroupId; set => SetField(ref _AdviceGroupId, value); }
        public ObservableCollectionExt<MappingItem> MappingItems { get => _MappingItems; set => SetField(ref _MappingItems, value); }
        public ExistState ExistState { get => _ExistState; set => SetField(ref _ExistState, value); }

        public Guid Guid { get; set; }
        public Guid GroupGuid { get; set; }

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

    }
}