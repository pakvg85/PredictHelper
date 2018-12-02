namespace PredictHelper
{
    public class ContentTypeItemViewModel : ViewModelBase
    {
        private int _Id;
        private string _Name;
        private bool _IsActive;

        public int Id
        {
            get => _Id;
            set => SetField(ref _Id, value);
        }
        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value);
        }
        public bool IsActive
        {
            get => _IsActive;
            set => SetField(ref _IsActive, value);
        }
    }
}