//-----------------------------------------------------------------------
// <copyright file="AbstractIgnoredEntity.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.SelectiveIgnore {
    using System;

    /// <summary>
    /// IIgnored entity holds the ignored remote object id and the local path.
    /// </summary>
    public abstract class AbstractIgnoredEntity : IEquatable<AbstractIgnoredEntity> {
        /// <summary>
        /// Gets or sets the remote object identifier of an ignored object.
        /// </summary>
        /// <value>The object identifier.</value>
        public string ObjectId { get; protected set; }

        /// <summary>
        /// Gets or sets the corresponding local path of the ignored remote object.
        /// </summary>
        /// <value>The local path.</value>
        public string LocalPath { get; protected set; }

        /// <summary>
        /// Determines whether the specified <see cref="CmisSync.Lib.SelectiveIgnore.AbstractIgnoredEntity"/> is equal to the
        /// current <see cref="CmisSync.Lib.SelectiveIgnore.AbstractIgnoredEntity"/>.
        /// </summary>
        /// <param name="other">The <see cref="CmisSync.Lib.SelectiveIgnore.AbstractIgnoredEntity"/> to compare with the current <see cref="CmisSync.Lib.SelectiveIgnore.AbstractIgnoredEntity"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="CmisSync.Lib.SelectiveIgnore.AbstractIgnoredEntity"/> is equal to the
        /// current <see cref="CmisSync.Lib.SelectiveIgnore.AbstractIgnoredEntity"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(AbstractIgnoredEntity other) {
            if (other == null) {
                return false;
            } else {
                return this.ObjectId == other.ObjectId;
            }
        }
    }
}