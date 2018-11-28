namespace PredictHelper
{
    public class ContentTypeItemViewModel : ViewModelBaseWithStore
    {
        public int Id { get { return Get<int>(); } set { Set(value); } }
        public string Name { get { return Get<string>(); } set { Set(value); } }
        public bool IsActive { get { return Get<bool>(); } set { Set(value); } }
    }
}