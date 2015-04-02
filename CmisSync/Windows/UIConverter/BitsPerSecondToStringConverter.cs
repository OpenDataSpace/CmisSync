
namespace CmisSync.UIConverter {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

    class BitsPerSecondToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return string.Empty;
            } else {
                return CmisSync.Lib.Utils.FormatBandwidth((double)((long)value));
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) {
            return null;
        }
    }
}