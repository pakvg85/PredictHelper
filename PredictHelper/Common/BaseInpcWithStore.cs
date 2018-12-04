using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PredictHelper
{
    public abstract class BaseInpcWithStore : BaseInpc
    {
        private Dictionary<string, object> _valueStore = new Dictionary<string, object>();

        protected T Get<T>([CallerMemberName]string property = null)
        {
            object value = null;
            if (!_valueStore.TryGetValue(property, out value))
                return default(T);
            return (T)value;
        }

        protected bool Set<T>(T value, [CallerMemberName]string property = null)
        {
            if (_valueStore.ContainsKey(property) && EqualityComparer<T>.Default.Equals((T)_valueStore[property], value))
                return false;

            _valueStore[property] = value;
            OnPropertyChanged(property);
            return true;
        }
    }
}