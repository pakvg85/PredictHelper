namespace PredictHelper
{
    public class FoundResultItemViewModel : ViewModelBaseWithStore
    {
        public string Value { get { return Get<string>(); } set { Set(value); } }
        public bool IsActive { get { return Get<bool>(); } set { Set(value); } }
    }
}