namespace PredictHelper
{
    public class PredicateGroupViewModel : ViewModelBase
    {
        private int _Id;
        private string _Text;

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

        override public string ToString() => Text + $" (Id {Id.ToString()})";
    }
}