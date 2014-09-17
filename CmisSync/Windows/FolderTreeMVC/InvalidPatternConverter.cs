//-----------------------------------------------------------------------
// <copyright file="InvalidPatternConverter.cs" company="GRAU DATA AG">
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
using System.Windows;
using System.Windows.Data;

namespace CmisSync.CmisTree
{
    /// <summary>
    /// Converter to convert the invalid Character as italic font style
    /// </summary>
    [ValueConversion(typeof(bool), typeof(FontStyle))]
    public class InvalidPatternConverter : IValueConverter
    {
        /// <summary>
        /// Converts the given bool into the FontStyle italic, if it is true
        /// </summary>
        /// <param name="value">Is an invalid pattern found</param>
        /// <param name="targetType">FontStyle</param>
        /// <param name="parameter">parameter is ignored</param>
        /// <param name="culture">Culture will be ignored</param>
        /// <returns>FontStyles.Italic, if value is true, otherwise FontStyles.Normal</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool ignored = (bool)value;
            if (ignored)
                return FontStyles.Italic;
            else
                return FontStyles.Normal;
        }

        /// <summary>
        /// Not implemented, throws NotImplementedException
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
