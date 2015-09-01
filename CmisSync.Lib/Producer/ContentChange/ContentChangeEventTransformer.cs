//-----------------------------------------------------------------------
// <copyright file="ContentChangeEventTransformer.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Producer.ContentChange {
    using System;
    using System.IO;
    using System.Linq;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Content change event transformer. Produces Folder and FileEvents from ContentChange Events.
    /// </summary>
    /// <exception cref='ArgumentNullException'>
    /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
    /// </exception>
    /// <exception cref='InvalidOperationException'>
    /// Is thrown when an operation cannot be performed.
    /// </exception>
    public class ContentChangeEventTransformer : ReportingSyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentChangeEventTransformer));

        private IMetaDataStorage storage;

        private IFileSystemInfoFactory fsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Producer.ContentChange.ContentChangeEventTransformer"/> class.
        /// </summary>
        /// <param name='queue'>The ISyncEventQueue.</param>
        /// <param name='storage'>The meta data storage.</param>
        /// <param name='fsFactory'>Fs factory can be null.</param>
        /// <exception cref='ArgumentNullException'>
        /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
        /// </exception>
        public ContentChangeEventTransformer(ISyncEventQueue queue, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null) : base(queue) {
            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            this.storage = storage;

            if (fsFactory == null) {
                this.fsFactory = new FileSystemInfoFactory();
            } else {
                this.fsFactory = fsFactory;
            }
        }

        /// <summary>
        /// Handle the specified e.
        /// </summary>
        /// <param name='e'>
        /// The ISyncEvent only handled when ContentChangeEvent
        /// </param>
        /// <returns>
        /// true if handled.
        /// </returns>
        /// <exception cref='InvalidOperationException'>
        /// Is thrown when an operation cannot be performed.
        /// </exception>
        public override bool Handle(ISyncEvent e) {
            if (!(e is ContentChangeEvent)) {
                return false;
            }

            Logger.Debug("Handling ContentChangeEvent");

            var contentChangeEvent = e as ContentChangeEvent;
            if (contentChangeEvent.Type != DotCMIS.Enums.ChangeType.Deleted && contentChangeEvent.CmisObject == null) {
                throw new InvalidOperationException("ERROR, ContentChangeEventAccumulator Missing");
            }

            if (contentChangeEvent.Type == DotCMIS.Enums.ChangeType.Deleted) {
                this.HandleDeletion(contentChangeEvent);
            } else if (contentChangeEvent.CmisObject is IFolder) {
                this.HandleAsIFolder(contentChangeEvent);
            } else if (contentChangeEvent.CmisObject is IDocument) {
                this.HandleAsIDocument(contentChangeEvent);
            }

            return true;
        }

        private void HandleDeletion(ContentChangeEvent contentChangeEvent) {
            Logger.Debug(contentChangeEvent.ObjectId);
            IMappedObject savedObject = this.storage.GetObjectByRemoteId(contentChangeEvent.ObjectId);
            if (savedObject != null) {
                IMappedObject obj = savedObject as IMappedObject;
                if (obj != null) {
                    if (obj.Type == MappedObjectType.Folder) {
                        var dirInfo = this.fsFactory.CreateDirectoryInfo(this.storage.GetLocalPath(obj));
                        Queue.AddEvent(new FolderEvent(dirInfo, null, this) { Remote = MetaDataChangeType.DELETED });
                        return;
                    } else {
                        var fileInfo = this.fsFactory.CreateFileInfo(this.storage.GetLocalPath(obj));
                        Queue.AddEvent(new FileEvent(fileInfo, null) { Remote = MetaDataChangeType.DELETED });
                        return;
                    }
                }
            }

            Logger.Debug("nothing found in local storage; it has never been synced");
        }

        private void HandleAsIDocument(ContentChangeEvent contentChangeEvent) {
            IDocument doc = contentChangeEvent.CmisObject as IDocument;
            switch(contentChangeEvent.Type)
            {
            case DotCMIS.Enums.ChangeType.Created:
            {
                var fileEvent = new FileEvent(null, doc) { Remote = MetaDataChangeType.CREATED };
                fileEvent.RemoteContent = doc.ContentStreamId == null ? ContentChangeType.NONE : ContentChangeType.CREATED;
                Queue.AddEvent(fileEvent);
                break;
            }

            case DotCMIS.Enums.ChangeType.Security:
            {
                IMappedObject file = this.storage.GetObjectByRemoteId(doc.Id);
                var fileInfo = (file == null) ? null : this.fsFactory.CreateFileInfo(this.storage.GetLocalPath(file));
                var fileEvent = new FileEvent(fileInfo, doc);
                if (fileInfo != null) {
                    fileEvent.Remote = MetaDataChangeType.CHANGED;
                } else {
                    fileEvent.Remote = MetaDataChangeType.CREATED;
                    if (file != null) {
                        byte[] hash = doc.ContentStreamHash(file.ChecksumAlgorithmName);
                        if (hash == null || !hash.Equals(file.LastChecksum)) {
                            fileEvent.RemoteContent = ContentChangeType.CHANGED;
                        } else {
                            fileEvent.RemoteContent = ContentChangeType.NONE;
                        }
                    } else {
                        fileEvent.RemoteContent = ContentChangeType.CREATED;
                    }
                }

                Queue.AddEvent(fileEvent);
                break;
            }

            case DotCMIS.Enums.ChangeType.Updated:
            {
                IMappedObject file = this.storage.GetObjectByRemoteId(doc.Id);
                var fileInfo = (file == null) ? null : this.fsFactory.CreateFileInfo(this.storage.GetLocalPath(file));
                var fileEvent = new FileEvent(fileInfo, doc);
                if (fileInfo != null) {
                    fileEvent.Remote = MetaDataChangeType.CHANGED;
                    if (file != null) {
                        byte[] hash = doc.ContentStreamHash(file.ChecksumAlgorithmName);
                        if (hash == null || !hash.SequenceEqual(file.LastChecksum)) {
                            Logger.Debug(string.Format("SavedChecksum: {0} RemoteChecksum: {1}", Utils.ToHexString(file.LastChecksum), Utils.ToHexString(hash)));
                            fileEvent.RemoteContent = ContentChangeType.CHANGED;
                        } else {
                            fileEvent.RemoteContent = ContentChangeType.NONE;
                        }
                    } else {
                        fileEvent.Remote = MetaDataChangeType.CREATED;
                        fileEvent.RemoteContent = ContentChangeType.CREATED;
                    }
                } else {
                    fileEvent.Remote = MetaDataChangeType.CREATED;
                    fileEvent.RemoteContent = ContentChangeType.CREATED;
                }

                Queue.AddEvent(fileEvent);
                break;
            }
            }
        }

        private void HandleAsIFolder(ContentChangeEvent contentChangeEvent) {
            IFolder folder = contentChangeEvent.CmisObject as IFolder;

            IMappedObject dir = this.storage.GetObjectByRemoteId(folder.Id);
            IDirectoryInfo dirInfo = (dir == null) ? null : this.fsFactory.CreateDirectoryInfo(this.storage.GetLocalPath(dir));
            var folderEvent = new FolderEvent(dirInfo, folder, this);
            switch(contentChangeEvent.Type) {
            case DotCMIS.Enums.ChangeType.Created:
                Logger.Debug("Created Folder Event");
                folderEvent.Remote = MetaDataChangeType.CREATED;
                break;
            case DotCMIS.Enums.ChangeType.Updated:
                folderEvent.Remote = MetaDataChangeType.CHANGED;
                break;
            case DotCMIS.Enums.ChangeType.Security:
                folderEvent.Remote = MetaDataChangeType.CHANGED;
                break;
            }

            Queue.AddEvent(folderEvent);
        }
    }
}