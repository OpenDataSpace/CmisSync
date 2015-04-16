using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmisSync.UIConverter {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;

    using CmisSync.Lib.FileTransmission;

    [ValueConversion(typeof(TransmissionStatus), typeof(Visibility))]
    public class TransmissionStatusToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((TransmissionStatus)value == TransmissionStatus.ABORTED || (TransmissionStatus)value == TransmissionStatus.FINISHED) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) {
            return null;
        }
    }
}