//-----------------------------------------------------------------------
// <copyright file="ISolver.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer.SituationSolver
{
    using System;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    /// <summary>
    /// I solver implements a mechanism to resolve one specific sync situation.
    /// </summary>
    public interface ISolver
    {
        /// <summary>
        /// Solve the specified situation by using the session, storage, localFile and remoteId.
        /// </summary>
        /// <param name="session">Cmis session instance.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="localFile">Local file.</param>
        /// <param name="remoteId">Remote identifier.</param>
        void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId);
    }
}