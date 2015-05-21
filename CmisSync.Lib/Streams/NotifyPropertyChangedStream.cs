//-----------------------------------------------------------------------
// <copyright file="NotifyPropertyChangedStream.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Streams {
    using System;
    using System.ComponentModel;
    using System.IO;

    /// <summary>
    /// Notify property changed stream wrapps the given stream and notifies about property changes.
    /// </summary>
    public abstract class NotifyPropertyChangedStream : StreamWrapper, INotifyPropertyChanged {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Streams.NotifyPropertyChangedStream"/> class.
        /// </summary>
        /// <param name="s">Stream to be wrapped.</param>
        public NotifyPropertyChangedStream(Stream s) : base(s) {
        }

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This method is called by the Set accessor of each property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected virtual void NotifyPropertyChanged(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("propertyName");
            }

            var handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}