//-----------------------------------------------------------------------
// <copyright file="ConfigChangedEvent.cs" company="GRAU DATA AG">
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
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Config changed event.
    /// </summary>
    public class ConfigChangedEvent : ISyncEvent
    {
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.ConfigChangedEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.ConfigChangedEvent"/>.
        /// </returns>
        public override string ToString()
        {
            return "ConfigChangedEvent";
        }
    }

    /// <summary>
    /// Repo config changed event.
    /// </summary>
    public class RepoConfigChangedEvent : ConfigChangedEvent
    {
        /// <summary>
        /// The repo info.
        /// </summary>
        public readonly RepoInfo RepoInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.RepoConfigChangedEvent"/> class.
        /// </summary>
        /// <param name='repoInfo'>
        /// Repo info.
        /// </param>
        public RepoConfigChangedEvent(RepoInfo repoInfo)
        {
            this.RepoInfo = repoInfo;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.RepoConfigChangedEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.RepoConfigChangedEvent"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("RepoConfigChangedEvent: {0}", this.RepoInfo.Name);
        }
    }
}
