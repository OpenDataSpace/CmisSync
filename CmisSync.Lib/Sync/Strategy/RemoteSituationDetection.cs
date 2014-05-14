//-----------------------------------------------------------------------
// <copyright file="RemoteSituationDetection.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Sync.Strategy
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Remote situation detection.
    /// </summary>
    public class RemoteSituationDetection : ISituationDetection<AbstractFolderEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RemoteSituationDetection));

        /// <summary>
        /// Analyse the specified actual event.
        /// </summary>
        /// <param name="storage">Storage of saved MappedObjects.</param>
        /// <param name="actualEvent">Actual event.</param>
        /// <returns>The detected situation type</returns>
        public SituationType Analyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            SituationType type = this.DoAnalyse(storage, actualEvent);
            Logger.Debug(string.Format("Remote Situation is: {0}", type));
            return type;
        }

        private SituationType DoAnalyse(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            switch (actualEvent.Remote) 
            {
            case MetaDataChangeType.CREATED:
                if(actualEvent is FileEvent) {
                    return this.IsSavedFileEqual(storage, (actualEvent as FileEvent).RemoteFile) ? SituationType.NOCHANGE : SituationType.ADDED;
                } else {
                    return SituationType.ADDED;
                }

            case MetaDataChangeType.DELETED:
                return SituationType.REMOVED;
            case MetaDataChangeType.MOVED:
                return SituationType.MOVED;
            case MetaDataChangeType.CHANGED:
                if (this.IsChangeEventAHintForMove(storage, actualEvent)) {
                    return SituationType.MOVED;
                }

                if(this.IsChangeEventAHintForRename(storage, actualEvent)) {
                    return SituationType.RENAMED;
                }

                return SituationType.CHANGED;
            case MetaDataChangeType.NONE:
            default:
                return SituationType.NOCHANGE;
            }
        }

        private bool IsSavedFileEqual(IMetaDataStorage storage, IDocument doc)
        {
            var mappedFile = storage.GetObjectByRemoteId(doc.Id) as IMappedObject;
            if(mappedFile != null &&
               mappedFile.Type == MappedObjectType.File &&
               mappedFile.LastRemoteWriteTimeUtc == doc.LastModificationDate &&
               mappedFile.Name == doc.Name &&
               mappedFile.LastChangeToken == doc.ChangeToken)
            {
                return true;
            } else {
                return false;
            }
        }

        private bool IsChangeEventAHintForMove(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            if(actualEvent is FolderEvent)
            {
                var folderEvent = actualEvent as FolderEvent;
                var storedFolder = storage.GetObjectByRemoteId(folderEvent.RemoteFolder.Id);
                return storedFolder.Name == folderEvent.RemoteFolder.Name && storedFolder.ParentId != folderEvent.RemoteFolder.ParentId;
            }

            return false;
        }

        private bool IsChangeEventAHintForRename(IMetaDataStorage storage, AbstractFolderEvent actualEvent)
        {
            if(actualEvent is FolderEvent)
            {
                var folderEvent = actualEvent as FolderEvent;
                var storedFolder = storage.GetObjectByRemoteId(folderEvent.RemoteFolder.Id);
                return storedFolder.Name != folderEvent.RemoteFolder.Name;
            }

            return false;
        }
    }
}