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

namespace CmisSync.Lib.Events {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Exceptions;

    /// <summary>
    /// Interaction needed event embeds exceptions which must be resolved by user interaction.
    /// </summary>
    public class InteractionNeededEvent : ExceptionEvent {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.InteractionNeededEvent"/> class.
        /// </summary>
        /// <param name="e">Exception which invokes a need for a user interaction.</param>
        public InteractionNeededEvent(InteractionNeededException e) : base(e) {
            if (e == null) {
                throw new ArgumentNullException("e");
            }

            this.AffectedFiles = new List<IFileSystemInfo>(e.AffectedFiles);
            this.Actions = new Dictionary<string, Action>(e.Actions);
            this.Title = e.Title;
            this.Description = e.Description;
            this.Details = e.Details;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.InteractionNeededEvent"/> class.
        /// </summary>
        /// <param name="msg">Message of the interaction needed exception.</param>
        public InteractionNeededEvent(string msg) : this(new InteractionNeededException(msg)) {
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>The details.</value>
        public string Details { get; set; }

        /// <summary>
        /// Gets the actions to resolve problem.
        /// </summary>
        /// <value>The actions.</value>
        public Dictionary<string, Action> Actions { get; private set; }

        /// <summary>
        /// Gets the affected files.
        /// </summary>
        /// <value>The affected files.</value>
        public List<IFileSystemInfo> AffectedFiles { get; private set; }
    }
}