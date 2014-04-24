//-----------------------------------------------------------------------
// <copyright file="RemoteEvent.cs" company="GRAU DATA AG">
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
using System;

using DotCMIS.Client;
using DotCMIS.Enums;

namespace CmisSync.Lib.Events
{
    public class RemoteEvent : ISyncEvent
    {
        private IChangeEvent change;

        public IChangeEvent Change { get { return this.change; } }

        public string ObjectId { get { return this.change.ObjectId; } }

        public DotCMIS.Enums.ChangeType? Type { get { return this.change.ChangeType; } }

        public RemoteEvent (IChangeEvent change)
        {
            if(change == null)
                throw new ArgumentNullException("The given change event must not be null");
            this.change = change;
        }

        public override string ToString ()
        {
            return string.Format ("[RemoteEvent: ChangeType={0} ObjectId={1}]", Type, ObjectId);
        }
    }
}

