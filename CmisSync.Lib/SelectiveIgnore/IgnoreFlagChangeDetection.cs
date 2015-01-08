//-----------------------------------------------------------------------
// <copyright file="IgnoreFlagChangeDetection.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Queueing;

    using DotCMIS.Client;

    public class IgnoreFlagChangeDetection : ReportingSyncEventHandler
    {
        private IIgnoredEntitiesStorage ignores;
        private IPathMatcher matcher;
        public IgnoreFlagChangeDetection(IIgnoredEntitiesStorage ignores, IPathMatcher matcher, ISyncEventQueue queue) : base(queue)
        {
            if (ignores == null) {
                throw new ArgumentNullException("Given ignores are null");
            }

            if (matcher == null) {
                throw new ArgumentNullException("Given path matcher is null");
            }

            this.ignores = ignores;
            this.matcher = matcher;
        }

        public override bool Handle(ISyncEvent e)
        {
            if (e is ContentChangeEvent) {
                var change = e as ContentChangeEvent;
                if (change.Type == DotCMIS.Enums.ChangeType.Deleted) {
                    if (this.ignores.IsIgnoredId(change.ObjectId) == IgnoredState.IGNORED) {
                        this.ignores.Remove(change.ObjectId);
                        this.Queue.AddEvent(new StartNextSyncEvent(true));
                    }

                    return false;
                }

                switch (this.ignores.IsIgnoredId(change.ObjectId)) {
                case IgnoredState.IGNORED:
                    if (!change.CmisObject.AreAllChildrenIgnored()) {
                        this.ignores.Remove(change.ObjectId);
                        this.Queue.AddEvent(new StartNextSyncEvent(true));
                    }

                    break;
                case IgnoredState.INHERITED:
                    goto case IgnoredState.NOT_IGNORED;
                case IgnoredState.NOT_IGNORED:
                    if (change.CmisObject.AreAllChildrenIgnored()) {
                        if (change.CmisObject is IFolder) {
                            this.ignores.AddOrUpdateEntryAndDeleteAllChildrenFromStorage(new IgnoredEntity(change.CmisObject as IFolder, this.matcher));
                        } else if (change.CmisObject is IDocument) {
                            this.ignores.Add(new IgnoredEntity(change.CmisObject as IDocument, this.matcher));
                        }
                    }

                    break;
                }
            }

            return false;
        }
    }
}