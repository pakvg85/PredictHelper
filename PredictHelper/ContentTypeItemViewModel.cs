namespace PredictHelper
{
    public class ContentTypeItemViewModel : ViewModelBase
    {
        private int _Id;
        private string _Name;
        private bool _IsActive;

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
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name == value)
                    return;
                _Name = value;
                OnPropertyChanged();
            }
        }
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (_IsActive == value)
                    return;
                _IsActive = value;
                OnPropertyChanged();
            }
        }
    }
}