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

namespace CmisSync.Lib.SelectiveIgnore
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    public class SelectiveIgnoreEventTransformer : SyncEventHandler
    {
        private ISyncEventQueue queue;
        private ObservableCollection<IIgnoredEntity> ignores;

        public SelectiveIgnoreEventTransformer(ObservableCollection<IIgnoredEntity> ignores, ISyncEventQueue queue) {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is empty");
            }

            if (ignores == null) {
                throw new ArgumentNullException("Given ignore collection is null");
            }

            this.queue = queue;
            this.ignores = ignores;
        }

        public override bool Handle(ISyncEvent e)
        {
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
            return false;
        }

        private bool IsInsideIgnoredPath(string path) {
            foreach(var ignore in this.ignores) {
                if (path.StartsWith(ignore.LocalPath)) {
                    return true;
                }
            }

            return false;
        }
    }
}