//-----------------------------------------------------------------------
// <copyright file="ContentChangeEvent.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events {
    using System;
    using System.IO;

    using CmisSync.Lib.Cmis;

    using DotCMIS.Client;

    /// <summary>
    /// Events Created By ContentChange Eventhandler
    /// </summary>
    public class ContentChangeEvent : ISyncEvent {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.ContentChangeEvent"/> class.
        /// </summary>
        /// <param name='type'>
        /// Type of the change.
        /// </param>
        /// <param name='objectId'>
        /// Object identifier.
        /// </param>
        public ContentChangeEvent(DotCMIS.Enums.ChangeType? type, string objectId) {
            if (objectId == null) {
                throw new ArgumentNullException("Argument null in ContenChangeEvent Constructor", "path");
            }

            if (type == null) {
                throw new ArgumentNullException("Argument null in ContenChangeEvent Constructor", "type");
            }

            this.Type = (DotCMIS.Enums.ChangeType)type;
            this.ObjectId = objectId;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public DotCMIS.Enums.ChangeType Type { get; private set; }

        /// <summary>
        /// Gets the object identifier.
        /// </summary>
        /// <value>
        /// The object identifier.
        /// </value>
        public string ObjectId { get; private set; }

        /// <summary>
        /// Gets the cmis object.
        /// </summary>
        /// <value>
        /// The cmis object.
        /// </value>
        public ICmisObject CmisObject { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.ContentChangeEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.ContentChangeEvent"/>.
        /// </returns>
        public override string ToString() {
            return string.Format("ContenChangeEvent with type \"{0}\" and ID \"{1}\"", this.Type, this.ObjectId);
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name='session'>
        /// Session from where the object should be requested.
        /// </param>
        public void UpdateObject(ISession session) {
           this.CmisObject = session.GetObject(this.ObjectId, OperationContextFactory.CreateNonCachingPathIncludingContext(session));
        }
    }
}