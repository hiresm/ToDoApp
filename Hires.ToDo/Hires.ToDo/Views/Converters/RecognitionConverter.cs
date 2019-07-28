using System;
using Windows.UI.Xaml.Data;

namespace Hires.ToDo.Views.Converters
{
    public class RecognitionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
                return "Stop";

            return "Start";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }
    }
}
