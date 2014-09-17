//-----------------------------------------------------------------------
// <copyright file="LoadingStatusConverter.cs" company="GRAU DATA AG">
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace CmisSync.CmisTree
{
    /// <summary>
    /// Converts the LoadingStatus to a Color and gives the possibility to change the default colors
    /// </summary>
    [ValueConversion(typeof(LoadingStatus), typeof(Brush))]
    public class LoadingStatusToBrushConverter : IValueConverter
    {
        private Brush startBrush = Brushes.LightGray;
        /// <summary>
        /// Color of the LoadingStatus.START status
        /// </summary>
        public Brush StartBrush { get { return startBrush; } set { startBrush = value; } }
        private Brush loadingBrush = Brushes.Gray;
        /// <summary>
        /// Color of the LoadingStatus.LOADING status
        /// </summary>
        public Brush LoadingBrush { get { return loadingBrush; } set { loadingBrush = value; } }
        private Brush abortBrush = Brushes.DarkGray;
        /// <summary>
        /// Color of the LoadingStatus.ABORT status
        /// </summary>
        public Brush AbortBrush { get { return abortBrush; } set { abortBrush = value; } }
        private Brush failureBrush = Brushes.Red;
        /// <summary>
        /// Color of the LoadingStatus.FAILURE status
        /// </summary>
        public Brush FailureBrush { get { return failureBrush; } set { failureBrush = value; } }
        private Brush doneBrush = Brushes.Black;
        /// <summary>
        /// Color of the LoadingStatus.DONE status
        /// </summary>
        public Brush DoneBrush { get { return doneBrush; } set { doneBrush = value; } }

        /// <summary>
        /// Converts the given LoadingStatus to a Brush with the selected Color
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LoadingStatus status = (LoadingStatus)value;
            switch (status)
            {
                case LoadingStatus.START:
                    return startBrush;
                case LoadingStatus.LOADING:
                    return loadingBrush;
                case LoadingStatus.ABORTED:
                    return abortBrush;
                case LoadingStatus.REQUEST_FAILURE:
                    return failureBrush;
                case LoadingStatus.DONE:
                    return doneBrush;
                default:
                    return Brushes.Black;
            }
        }
        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { throw new NotSupportedException(); }
    }

    /// <summary>
    /// Converter for the LoadingStatus to a cultrue depending string
    /// </summary>
    [ValueConversion(typeof(LoadingStatus), typeof(string))]
    public class LoadingStatusToTextConverter : IValueConverter
    {
        /// <summary>
        /// Converts the LoadingStatus to a translated string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LoadingStatus status = (LoadingStatus)value;
            switch (status)
            {
                case LoadingStatus.LOADING:
                    return Properties_Resources.LoadingStatusLOADING;
                case LoadingStatus.START:
                    return Properties_Resources.LoadingStatusSTART;
                case LoadingStatus.ABORTED:
                    return Properties_Resources.LoadingStatusABORTED;
                default:
                    return "";
            }
        }
        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { throw new NotSupportedException(); }
    }

}
