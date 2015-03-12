//-----------------------------------------------------------------------
// <copyright file="LocalObjectAddedWithPWC.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer.SituationSolver.PWC {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Data.Impl;

    using log4net;

    /// <summary>
    /// Local object added and the server is able to update PWC. If a folder is added => calls the given local folder added solver implementation
    /// </summary>
    public class LocalObjectAddedWithPWC : AbstractEnhancedSolver {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LocalObjectAddedWithPWC));

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.PWC.LocalObjectAddedWithPWC"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="transmissionStorage">Transmission storage.</param>
        /// <param name="manager">Active activities manager.</param>
        /// <param name="localFolderAddedSolver">Local folder added solver.</param>
        public LocalObjectAddedWithPWC(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage,
            ActiveActivitiesManager manager) : base(session, storage, transmissionStorage)
        {
            if (!session.ArePrivateWorkingCopySupported()) {
                throw new ArgumentException("Given session doesn't support private working copies");
            }
        }

        /// <summary>
        /// Solve the specified situation by using localFile and remote object.
        /// </summary>
        /// <param name="localFileSystemInfo">Local filesystem info instance.</param>
        /// <param name="remoteId">Remote identifier or object.</param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
        public override void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            IFileInfo localFile = localFileSystemInfo as IFileInfo;
            if (localFile == null) {
                throw new NotSupportedException();
            }

            Stopwatch completewatch = new Stopwatch();
            completewatch.Start();
            Logger.Debug("Starting LocalObjectAddedWithPWC");

            localFile.Refresh();
            if (!localFile.Exists) {
                throw new FileNotFoundException(string.Format("Local file {0} has been renamed/moved/deleted", localFile.FullName));
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, localFile.Name);
            if (this.ServerCanModifyDateTimes) {
                properties.Add(PropertyIds.CreationDate, localFile.CreationTimeUtc);
                properties.Add(PropertyIds.LastModificationDate, localFile.LastWriteTimeUtc);
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            var objId = Session.CreateDocument(
                properties,
                new ObjectId(Storage.GetRemoteId(localFile.Directory)),
                null,
                null,
                null,
                null,
                null);
            watch.Stop();
            Logger.Debug(string.Format("CreatedDocument in [{0} msec]", watch.ElapsedMilliseconds));

            watch.Restart();
            IDocument remoteDocument = Session.GetObject(objId) as IDocument;
            watch.Stop();
            Logger.Debug(string.Format("GetDocument in [{0} msec]", watch.ElapsedMilliseconds));

            Guid uuid = this.WriteOrUseUuidIfSupported(localFileSystemInfo);
        }
    }
}