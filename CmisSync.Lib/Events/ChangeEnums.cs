//-----------------------------------------------------------------------
// <copyright file="ChangeEnums.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Meta data change type.
    /// </summary>
    public enum MetaDataChangeType {
        /// <summary>
        /// Nothing has been changed.
        /// </summary>
        NONE,

        /// <summary>
        /// The entity has been created.
        /// </summary>
        CREATED,

        /// <summary>
        /// The meta data has been changed.
        /// </summary>
        CHANGED,

        /// <summary>
        /// The entity has been deleted.
        /// </summary>
        DELETED,

        /// <summary>
        /// The entity has been moved.
        /// </summary>
        MOVED
    }

    /// <summary>
    /// Content change type.
    /// </summary>
    public enum ContentChangeType {
        /// <summary>
        /// The content has not been changed.
        /// </summary>
        NONE,

        /// <summary>
        /// The content has been created.
        /// </summary>
        CREATED,

        /// <summary>
        /// The content has been changed.
        /// </summary>
        CHANGED,

        /// <summary>
        /// The content has been deleted.
        /// </summary>
        DELETED,

        /// <summary>
        /// The content has been changed by an append.
        /// </summary>
        APPENDED
    }
}