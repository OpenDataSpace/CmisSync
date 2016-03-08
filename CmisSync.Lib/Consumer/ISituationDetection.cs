//-----------------------------------------------------------------------
// <copyright file="ISituationDetection.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer {
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;

    /// <summary>
    /// Situation type.
    /// </summary>
    public enum SituationType {
        /// <summary>
        /// Constant NOCHANGE.
        /// </summary>
        NOCHANGE,
        
        /// <summary>
        /// Constant ADDED.
        /// </summary>
        ADDED,
        
        /// <summary>
        /// Constant CHANGE.
        /// </summary>
        CHANGED,
        
        /// <summary>
        /// Constant RENAME.
        /// </summary>
        RENAMED,
        
        /// <summary>
        /// Constant MOVE.
        /// </summary>
        MOVED,
        
        /// <summary>
        /// Constant REMOVE.
        /// </summary>
        REMOVED
    }

    /// <summary>
    /// Situation Detection Interface for SyncStrategy
    /// </summary>
    /// <typeparam name="T">
    /// Type of folderevent to detect sitation upon
    /// </typeparam>
    public interface ISituationDetection<T> where T : AbstractFolderEvent {
        /// <summary>
        /// Analyse the specified actualEvent.
        /// </summary>
        /// <param name='storage'>
        /// Storage interface.
        /// </param>
        /// <param name='actualEvent'>
        /// Actual event.
        /// </param>
        /// <returns>
        /// The Situation Type.
        /// </returns>
        SituationType Analyse(IMetaDataStorage storage, T actualEvent);
    }
}