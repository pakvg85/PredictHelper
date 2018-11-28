namespace PredictHelper
{
    public class MappingItemViewModel : ViewModelBaseWithStore
    {
        public MappingKey Key { get { return Get<MappingKey>(); } set { Set(value); } }
        public bool IsActive { get { return Get<bool>(); } set { Set(value); } }
    }
}