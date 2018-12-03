using System;
using System.Globalization;
using System.Windows.Data;

namespace PredictHelper
{
    class ExistStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (ExistState)value;
            return source == ExistState.Default ? " "
                : source == ExistState.New ? "New"
                : source == ExistState.Updated ? "Upd"
                : source == ExistState.ToBeDeleted ? "Del"
                : "<INVALID>"; // : throw new System.Exception($"Invalid ExistState value: {source}"); // DO.NOT.THROW.EXCEPTIONS. 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}