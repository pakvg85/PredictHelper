using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PredictHelper
{
    public abstract class ViewModelBaseWithStore : INotifyPropertyChanged
    {
        private Dictionary<string, object> _valueStore = new Dictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected T Get<T>([CallerMemberName]string property = null)
        {
            object value = null;
            if (!_valueStore.TryGetValue(property, out value))
                return default(T);
            return (T)value;
        }

        protected void Set<T>(T value, [CallerMemberName]string property = null)
        {
            _valueStore[property] = value;
            OnPropertyChangedInternal(property);
        }

        protected void OnPropertyChanged([CallerMemberName]string property = null)
        {
            OnPropertyChangedInternal(property);
        }

        private void OnPropertyChangedInternal(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}