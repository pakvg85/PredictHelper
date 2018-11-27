using System.Windows.Input;

namespace PredictHelper
{
    public class PredicateItemViewModel : ViewModelBaseWithStore
    {
        public string Value { get { return Get<string>(); } set { Set(value); } }
        public bool IsEditing { get { return Get<bool>(); } set { Set(value); } }
        public bool IsFocused { get { return Get<bool>(); } set { Set(value); } }

        public ICommand Command3 { get; set; }

        public PredicateItemViewModel()
        {
            Command3 = new RelayCommand(o => EditSingle());
        }

        public void EditSingle()
        {
            IsEditing = false;
            IsFocused = false;
        }
    }
}