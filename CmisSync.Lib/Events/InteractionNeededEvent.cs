//-----------------------------------------------------------------------
// <copyright file="InteractionNeededEvent.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Exceptions;

    public class InteractionNeededEvent : ExceptionEvent
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Details { get; set; }

        public Dictionary<string, Action> Actions { get; private set; }

        public List<IFileSystemInfo> AffectedFiles { get; private set; }

        public InteractionNeededEvent(InteractionNeededException e) : base(e)
        {
            if (e == null) {
                throw new ArgumentNullException("Given Exception is null");
            }

            this.AffectedFiles = new List<IFileSystemInfo>(e.AffectedFiles);
            this.Actions = new Dictionary<string, Action>(e.Actions);
            this.Title = e.Title;
            this.Description = e.Description;
            this.Details = e.Details;
        }

        public InteractionNeededEvent(string msg) : this(new InteractionNeededException(msg)) {
        }
    }
}