//-----------------------------------------------------------------------
// <copyright file="SelectiveIgnoreEventTransformer.cs" company="GRAU DATA AG">
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
    using System.Collections.ObjectModel;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    public class SelectiveIgnoreEventTransformer : SyncEventHandler {
        private ISyncEventQueue queue;
        private IIgnoredEntitiesCollection ignores;

        public SelectiveIgnoreEventTransformer(IIgnoredEntitiesCollection ignores, ISyncEventQueue queue) {
            if (queue == null) {
                throw new ArgumentNullException("queue");
            }

            if (ignores == null) {
                throw new ArgumentNullException("ignores");
            }

            this.ignores = ignores;
            this.queue = queue;
        }

        public override bool Handle(ISyncEvent e) {
            if (e is FSMovedEvent) {
                var movedEvent = e as FSMovedEvent;
                if (this.IsInsideIgnoredPath(movedEvent.OldPath) && !this.IsInsideIgnoredPath(movedEvent.LocalPath)) {
                    this.queue.AddEvent(new FSEvent(WatcherChangeTypes.Created, movedEvent.LocalPath, movedEvent.IsDirectory));
                    return true;
                } else if (this.IsInsideIgnoredPath(movedEvent.LocalPath) && !this.IsInsideIgnoredPath(movedEvent.OldPath)) {
                    this.queue.AddEvent(new FSEvent(WatcherChangeTypes.Deleted, movedEvent.OldPath, movedEvent.IsDirectory));
                    return true;
                }
            }

            if (e is ContentChangeEvent) {
                var contentChangeEvent = e as ContentChangeEvent;
                if (contentChangeEvent.Type != ChangeType.Deleted) {
                    var state = IgnoredState.NOT_IGNORED;
                    var cmisObject = contentChangeEvent.CmisObject;
                    if (cmisObject is IFolder) {
                        state = this.ignores.IsIgnored(cmisObject as IFolder);
                    } else if (cmisObject is IDocument) {
                        state = this.ignores.IsIgnored(cmisObject as IDocument);
                    }

                    if (state == IgnoredState.INHERITED) {
                        this.queue.AddEvent(new ContentChangeEvent(ChangeType.Deleted, contentChangeEvent.ObjectId));
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsInsideIgnoredPath(string path) {
            return this.ignores.IsIgnoredPath(path) == IgnoredState.INHERITED;
        }
    }
}