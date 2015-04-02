
namespace CmisSync.UIConverter {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

    using CmisSync.Lib.FileTransmission;

    [ValueConversion(typeof(TransmissionStatus), typeof(bool))]
    class TransmissionStatusToDoneConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((TransmissionStatus)value == TransmissionStatus.ABORTED || (TransmissionStatus)value == TransmissionStatus.FINISHED);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) {
            return null;
        }
    }
}