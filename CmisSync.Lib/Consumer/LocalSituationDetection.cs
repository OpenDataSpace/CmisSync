//-----------------------------------------------------------------------
// <copyright file="LocalSituationDetection.cs" company="GRAU DATA AG">
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
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using log4net;

    /// <summary>
    /// Local situation detection.
    /// </summary>
    public class LocalSituationDetection : ISituationDetection<AbstractFolderEvent> {
        /// <summary>
        /// Analyse the situation of the actual event in combination with the meta data storage.
        /// </summary>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="actualEvent">Actual event.</param>
        /// <returns>The detected local situation.</returns>
        public SituationType Analyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent) {
            SituationType type = this.DoAnalyse(storage, actualEvent);
            return type;
        }

        private SituationType DoAnalyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent) {
            IFileSystemInfo localPath = actualEvent is FolderEvent ? (IFileSystemInfo)(actualEvent as FolderEvent).LocalFolder : (IFileSystemInfo)(actualEvent is FileEvent ? (actualEvent as FileEvent).LocalFile : null);
            switch (actualEvent.Local)
            {
            case MetaDataChangeType.CREATED:
                return SituationType.ADDED;
            case MetaDataChangeType.DELETED:
                return SituationType.REMOVED;
            case MetaDataChangeType.MOVED:
                try {
                    Guid? guid = localPath.Uuid;
                    var obj = storage.GetObjectByGuid((Guid)guid);
                    var parent = storage.GetObjectByRemoteId(obj.ParentId);
                    Guid? parentGuid = (localPath is IFileInfo) ? (localPath as IFileInfo).Directory.Uuid : (localPath as IDirectoryInfo).Parent.Uuid;
                    if (parent.Guid == (Guid)parentGuid) {
                        return SituationType.RENAMED;
                    }
                } catch (Exception) {
                }

                return SituationType.MOVED;
            case MetaDataChangeType.CHANGED:
                if (storage.GetObjectByLocalPath(localPath) == null) {
                    Guid? guid = localPath.Uuid;
                    if (guid != null && storage.GetObjectByGuid((Guid)guid) != null) {
                        return SituationType.RENAMED;
                    }
                }

                return SituationType.CHANGED;
            case MetaDataChangeType.NONE:
                if (actualEvent is FileEvent && (actualEvent as FileEvent).LocalContent != ContentChangeType.NONE) {
                    return SituationType.CHANGED;
                } else {
                    return SituationType.NOCHANGE;
                }

            default:
                return SituationType.NOCHANGE;
            }
        }
    }
}