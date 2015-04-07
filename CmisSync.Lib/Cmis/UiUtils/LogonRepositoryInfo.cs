//-----------------------------------------------------------------------
// <copyright file="LogonRepositoryInfo.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis.UiUtils {
    using System;

    /// <summary>
    /// Logon repository info containing repo id and repo name.
    /// </summary>
    public class LogonRepositoryInfo {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo"/> class.
        /// </summary>
        /// <param name="id">Repository Identifier.</param>
        /// <param name="name">Repository Name.</param>
        public LogonRepositoryInfo(string id, string name) {
            this.Id = id;
            this.Name = name;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Cmis.UiUtils.LogonRepositoryInfo"/>.</returns>
        public override string ToString() {
            return string.Format("[LogonRepositoryInfo: Name={0}, Id={1}]", this.Name, this.Id);
        }
    }
}