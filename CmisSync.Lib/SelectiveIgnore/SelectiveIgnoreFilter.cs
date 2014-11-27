//-----------------------------------------------------------------------
// <copyright file="SelectiveIgnoreFilter.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.SelectiveIgnore
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    /// <summary>
    /// Selective ignore filter.
    /// All file/folder events for affecting files/folders which are inside an ignored folder are filtered out.
    /// </summary>
    public class SelectiveIgnoreFilter : SyncEventHandler
    {
        private IIgnoredEntitiesStorage storage;

        public SelectiveIgnoreFilter(ObservableCollection<IIgnoredEntity> ignores, IIgnoredEntitiesStorage storage) {
            if (ignores == null) {
                throw new ArgumentNullException("The collection of ignored entities is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("The given storage is null");
            }

            this.storage = storage;
        }

        public override bool Handle(ISyncEvent e)
        {
            if (e is IFilterableRemoteObjectEvent) {
                var ev = e as IFilterableRemoteObjectEvent;

                if (ev.RemoteObject is IFolder) {
                    if (this.storage.IsIgnored(ev.RemoteObject as IFolder) == IgnoredState.INHERITED) {
                        return true;
                    }
                } else if (ev.RemoteObject is IDocument) {
                    if (this.storage.IsIgnored(ev.RemoteObject as IDocument) == IgnoredState.INHERITED) {
                        return true;
                    }
                }
            }

            if (e is IFilterableLocalPathEvent) {
                var path = (e as IFilterableLocalPathEvent).LocalPath;
                if (path != null) {
                    if (this.storage.IsIgnoredPath(path) == IgnoredState.INHERITED) {
                        return true;
                    }
                }
            }

            if (e is FolderEvent) {
                var folderEvent = e as FolderEvent;
                if (folderEvent.LocalFolder != null &&
                    (folderEvent.Local == MetaDataChangeType.CREATED || folderEvent.Local == MetaDataChangeType.DELETED))
                {

                }
            }

            return false;
        }
    }
}