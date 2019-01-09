namespace PredictHelper
{
    public class AdviceGroupItem : BaseInpc
    {
        public int _Id;
        public int _PredicateGroupId;
        public string _ShortDescription;
        private ExistState _ExistState;

        public int Id { get => _Id; set => SetField(ref _Id, value); }
        public int PredicateGroupId { get => _PredicateGroupId; set => SetField(ref _PredicateGroupId, value); }
        public string ShortDescription { get => _ShortDescription; set => SetField(ref _ShortDescription, value); }
        public ExistState ExistState { get => _ExistState; set => SetField(ref _ExistState, value); }
    }
}