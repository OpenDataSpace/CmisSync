
namespace CmisSync.UIConverter {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

    using CmisSync.Lib.FileTransmission;
    
    public class TypeToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((TransmissionType)value) {
                case TransmissionType.UPLOAD_NEW_FILE:
                    return UIHelpers.GetBitmap("Uploading");
                case TransmissionType.UPLOAD_MODIFIED_FILE:
                    return UIHelpers.GetImageSource("Downloading");
                default:
                    return UIHelpers.GetImageSource("Updating");
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) {
            return null;
        }
    }
}