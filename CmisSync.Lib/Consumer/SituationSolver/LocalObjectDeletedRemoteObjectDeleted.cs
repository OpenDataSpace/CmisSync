//-----------------------------------------------------------------------
// <copyright file="LocalObjectDeletedRemoteObjectDeleted.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;
    using DotCMIS.Client;

    /// <summary>
    /// Local object deleted remote object deleted.
    /// </summary>
    public class LocalObjectDeletedRemoteObjectDeleted : AbstractEnhancedSolver
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Consumer.SituationSolver.LocalObjectDeletedRemoteObjectDeleted"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        public LocalObjectDeletedRemoteObjectDeleted(ISession session, IMetaDataStorage storage) : base(session, storage) {
        }

        public override void Solve(IFileSystemInfo localFile, IObjectId remoteId)
        {
            var mappedObject = this.Storage.GetObjectByLocalPath(localFile);
            this.Storage.RemoveObject(mappedObject);
        }
    }
}
