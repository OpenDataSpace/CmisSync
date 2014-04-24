//-----------------------------------------------------------------------
// <copyright file="FolderTypeConverter.cs" company="GRAU DATA AG">
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
    /// Converter for FolderType to Color convertion
    /// </summary>
    [ValueConversion(typeof(Folder.NodeLocationType), typeof(Brush))]
    public class FolderTypeToBrushConverter : IValueConverter
    {
        private Brush noneFolderBrush = Brushes.Red;
        /// <summary>
        /// Color of FolderType.NONE
        /// </summary>
        public Brush NocalFolderBrush { get { return noneFolderBrush; } set { localFolderBrush = value; } }
        private Brush localFolderBrush = Brushes.Gray;
        /// <summary>
        /// Color of FolderType.LOCAL
        /// </summary>
        public Brush LocalFolderBrush { get { return localFolderBrush; } set { localFolderBrush = value; } }
        private Brush remoteFolderBrush = Brushes.Blue;
        /// <summary>
        /// Color of FolderType.REMOTE
        /// </summary>
        public Brush RemoteFolderBrush { get { return remoteFolderBrush; } set { remoteFolderBrush = value; } }
        private Brush bothFolderBrush = Brushes.Green;
        /// <summary>
        /// Color of FolderType.BOTH
        /// </summary>
        public Brush BothFolderBrush { get { return bothFolderBrush; } set { bothFolderBrush = value; } }
        /// <summary>
        /// Converts the given FolderType to a Brush with the selected color
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Folder.NodeLocationType type = (Folder.NodeLocationType)value;
            switch (type)
            {
                case Folder.NodeLocationType.NONE:
                    return noneFolderBrush;
                case Folder.NodeLocationType.LOCAL:
                    return localFolderBrush;
                case Folder.NodeLocationType.REMOTE:
                    return remoteFolderBrush;
                case Folder.NodeLocationType.BOTH:
                    return bothFolderBrush;
                default:
                    return Brushes.White;
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
