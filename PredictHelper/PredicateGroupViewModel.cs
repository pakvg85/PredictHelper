using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredictHelper
{
    public class PredicateGroupViewModel : ViewModelBase
    {
        private int _Id;
        private string _Text;
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
        public string Text
        {
            get { return _Text; }
            set
            {
                if (_Text == value)
                    return;
                _Text = value;
                OnPropertyChanged();
            }
        }
        override public string ToString() => Text + $" (Id {Id.ToString()})";
    }
}