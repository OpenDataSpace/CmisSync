
namespace CmisSync.UIConverter {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    
    class TransmissionExceptionToTextConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var exception = value as Exception;
            if (exception != null) {
                return string.Format("{0}:{1}{2}", exception.Message, Environment.NewLine, exception.StackTrace);
            } else {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}