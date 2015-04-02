
namespace CmisSync.UIConverter {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

    [ValueConversion(typeof(long?), typeof(string))]
    class TranmissionPositionToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null) {
                if ((long)value == 0) {
                    return string.Empty;
                } else {
                    return string.Format("{0}/", CmisSync.Lib.Utils.FormatSize((long)value));
                }
            } else {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) {
            return null;
        }
    }
}