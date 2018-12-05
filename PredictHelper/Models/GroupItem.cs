using System;

namespace PredictHelper
{
    public class GroupItem : BaseInpc
    {
        private int _Id;
        private string _Text;
        private ObservableCollectionExt<PredicateItem> _PredicateItems;
        private ExistState _ExistState;

        public int Id { get => _Id; set => SetField(ref _Id, value); }
        public string Text { get => _Text; set => SetField(ref _Text, value); }
        public ObservableCollectionExt<PredicateItem> PredicateItems { get => _PredicateItems; set => SetField(ref _PredicateItems, value); }
        public ExistState ExistState { get => _ExistState; set => SetField(ref _ExistState, value); }
        public Guid Guid { get; set; }

        override public string ToString() => Text + $" (Id {Id.ToString()})";

        public GroupItem()
        {
            PredicateItems = new ObservableCollectionExt<PredicateItem>();
        }
    }
}