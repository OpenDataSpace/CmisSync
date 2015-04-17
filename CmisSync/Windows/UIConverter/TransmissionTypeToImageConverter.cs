//-----------------------------------------------------------------------
// <copyright file="TransmissionTypeToImageConverter.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
﻿
namespace CmisSync.UIConverter {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

    using CmisSync.Lib.FileTransmission;
    using System.Windows.Media.Imaging;

    [ValueConversion(typeof(TransmissionType), typeof(BitmapFrame))]
    public class TypeToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((TransmissionType)value) {
                case TransmissionType.UPLOAD_NEW_FILE:
                    return UIHelpers.GetImageSource("Uploading");
                case TransmissionType.DOWNLOAD_NEW_FILE:
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