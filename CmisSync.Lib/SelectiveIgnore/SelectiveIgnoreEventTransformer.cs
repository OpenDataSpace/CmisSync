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
            throw new NotImplementedException();
        }
    }
}