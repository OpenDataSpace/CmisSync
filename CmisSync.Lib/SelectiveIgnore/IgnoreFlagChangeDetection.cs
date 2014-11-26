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
    using CmisSync.Lib.Queueing;

    public class IgnoreFlagChangeDetection : SyncEventHandler
    {
        private ObservableCollection<IIgnoredEntity> ignores;
        public IgnoreFlagChangeDetection(ObservableCollection<IIgnoredEntity> ignores)
        {
            if (ignores == null) {
                throw new ArgumentNullException("Given ignores are null");
            }

            this.ignores = ignores;
        }

        public override bool Handle(ISyncEvent e)
        {
            if (e is ContentChangeEvent) {
                var change = e as ContentChangeEvent;
                if (this.IsIgnoredId(change.ObjectId) && change.CmisObject != null) {
                    var obj = change.CmisObject;
                    if (obj.AreAllChildrenIgnored()) {
                        return false;
                    } else {
////                        this.ignore
                    }
                }
            }

            return false;
        }

        private bool IsIgnoredId(string objectId) {
            foreach(var ignore in this.ignores) {
                if (objectId == ignore.ObjectId) {
                    return true;
                }
            }

            return false;
        }
    }
}