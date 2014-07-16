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

namespace CmisSync.Lib.Consumer
{
    using System;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Storage.Database;

    using log4net;

    /// <summary>
    /// Local situation detection.
    /// </summary>
    public class LocalSituationDetection : ISituationDetection<AbstractFolderEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalSituationDetection));

        /// <summary>
        /// Analyse the situation of the actual event in combination with the meta data storage.
        /// </summary>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="actualEvent">Actual event.</param>
        /// <returns>The detected local situation.</returns>
        public SituationType Analyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            SituationType type = this.DoAnalyse(storage, actualEvent);
            Logger.Debug(string.Format("Local Situation is: {0}", type));
            return type;
        }

        private SituationType DoAnalyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            switch (actualEvent.Local)
            {
            case MetaDataChangeType.CREATED:
                return SituationType.ADDED;
            case MetaDataChangeType.DELETED:
                return SituationType.REMOVED;
            case MetaDataChangeType.MOVED:
                return SituationType.MOVED;
            case MetaDataChangeType.CHANGED:
                IFileSystemInfo localPath = actualEvent is FolderEvent ? (IFileSystemInfo)(actualEvent as FolderEvent).LocalFolder : (IFileSystemInfo)(actualEvent is FileEvent ? (actualEvent as FileEvent).LocalFile : null);
                if (storage.GetObjectByLocalPath(localPath) == null) {
                    string ea = localPath.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
                    Guid guid;
                    if (Guid.TryParse(ea, out guid) && storage.GetObjectByGuid(guid) != null) {
                        return SituationType.RENAMED;
                    }
                }

                return SituationType.CHANGED;
            case MetaDataChangeType.NONE:
                if(actualEvent is FileEvent && (actualEvent as FileEvent).LocalContent != ContentChangeType.NONE) {
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