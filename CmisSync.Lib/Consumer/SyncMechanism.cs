//-----------------------------------------------------------------------
// <copyright file="SyncMechanism.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Sync mechanism.
    /// </summary>
    public class SyncMechanism : ReportingSyncEventHandler
    {
        /// <summary>
        /// All available solver.
        /// </summary>
        public ISolver[,] Solver;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(SyncMechanism));

        private ISession session;
        private IMetaDataStorage storage;
        private IFileTransmissionStorage transmissionStorage;
        private ActivityListenerAggregator activityListener;
        private IFilterAggregator filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncMechanism"/> class.
        /// </summary>
        /// <param name="localSituation">Local situation.</param>
        /// <param name="remoteSituation">Remote situation.</param>
        /// <param name="queue">Sync event queue.</param>
        /// <param name="session">CMIS Session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="activityListener">Active sync progress listener.</param>
        /// <param name="solver">Solver for custom solver matrix.</param>
        /// <param name="isServerAbleToUpdateModificationDate">Enables the modification date sync feature.</param>
        public SyncMechanism(
            ISituationDetection<AbstractFolderEvent> localSituation,
            ISituationDetection<AbstractFolderEvent> remoteSituation,
            ISyncEventQueue queue,
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage,
            ActivityListenerAggregator activityListener,
            IFilterAggregator filters,
            ISolver[,] solver = null) : base(queue)
        {
            if (session == null) {
                throw new ArgumentNullException("Given session is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            if (transmissionStorage == null)
            {
                throw new ArgumentNullException("Given fileTransmissionStorage is null");
            }

            if (localSituation == null)
            {
                throw new ArgumentNullException("Given local situation detection is null");
            }

            if (remoteSituation == null) {
                throw new ArgumentNullException("Given remote situation detection is null");
            }

            if (activityListener == null) {
                throw new ArgumentNullException("Given activity listener is null");
            }

            if (filters == null) {
                throw new ArgumentNullException("Given filter aggregator is null");
            }

            this.session = session;
            this.storage = storage;
            this.transmissionStorage = transmissionStorage;
            this.LocalSituation = localSituation;
            this.RemoteSituation = remoteSituation;
            this.activityListener = activityListener;
            this.filters = filters;
            this.Solver = solver == null ? this.CreateSolver() : solver;
        }

        /// <summary>
        /// Gets or sets the local situation detection.
        /// </summary>
        public ISituationDetection<AbstractFolderEvent> LocalSituation { get; set; }

        /// <summary>
        /// Gets or sets the remote situation detection.
        /// </summary>
        public ISituationDetection<AbstractFolderEvent> RemoteSituation { get; set; }

        /// <summary>
        /// Handles File or FolderEvents and tries to solve the detected situation.
        /// </summary>
        /// <param name="e">FileEvent or FolderEvent</param>
        /// <returns><c>true</c> if the Event has been handled, otherwise <c>false</c></returns>
        public override bool Handle(ISyncEvent e)
        {
            if (e is AbstractFolderEvent) {
                var folderEvent = e as AbstractFolderEvent;

                try {
                    this.DoHandle(folderEvent);
                } catch (RetryException retry) {
                    Logger.Debug(string.Format("RetryException[{0}] thrown for event {1} => enqueue event", retry.Message, folderEvent.ToString()));
                    folderEvent.RetryCount++;
                    this.Queue.AddEvent(folderEvent);
                } catch (InteractionNeededException interaction) {
                    this.Queue.AddEvent(new InteractionNeededEvent(interaction));
                } catch (Exception ex) {
                    Logger.Debug("Exception in SyncMechanism, requesting FullSync and rethrowing", ex);
                    this.Queue.AddEvent(new StartNextSyncEvent(true));
                    throw;
                }

                return true;
            }

            return false;
        }

        private ISolver[,] CreateSolver()
        {
            int dim = Enum.GetNames(typeof(SituationType)).Length;
            ISolver[,] solver = new ISolver[dim, dim];
            var changeChangeSolver = new LocalObjectChangedRemoteObjectChanged(this.session, this.storage, this.transmissionStorage, this.activityListener.TransmissionManager);
            var renameRenameSolver = new LocalObjectRenamedRemoteObjectRenamed(this.session, this.storage, changeChangeSolver);
            var renameChangeSolver = new LocalObjectRenamedRemoteObjectChanged(this.session, this.storage, changeChangeSolver);

            solver[(int)SituationType.NOCHANGE, (int)SituationType.NOCHANGE] = new NothingToDoSolver();
            solver[(int)SituationType.ADDED, (int)SituationType.NOCHANGE] = new LocalObjectAdded(this.session, this.storage, this.activityListener.TransmissionManager);
            solver[(int)SituationType.CHANGED, (int)SituationType.NOCHANGE] = new LocalObjectChanged(this.session, this.storage, this.activityListener.TransmissionManager);
            solver[(int)SituationType.MOVED, (int)SituationType.NOCHANGE] = new LocalObjectMoved(this.session, this.storage);
            solver[(int)SituationType.RENAMED, (int)SituationType.NOCHANGE] = new LocalObjectRenamed(this.session, this.storage);
            solver[(int)SituationType.REMOVED, (int)SituationType.NOCHANGE] = new LocalObjectDeleted(this.session, this.storage);

            solver[(int)SituationType.NOCHANGE, (int)SituationType.ADDED] = new RemoteObjectAdded(this.session, this.storage, this.transmissionStorage, this.activityListener.TransmissionManager);

            solver[(int)SituationType.NOCHANGE, (int)SituationType.CHANGED] = new RemoteObjectChanged(this.session, this.storage, this.transmissionStorage, this.activityListener.TransmissionManager);
            solver[(int)SituationType.CHANGED, (int)SituationType.CHANGED] = changeChangeSolver;
            solver[(int)SituationType.MOVED, (int)SituationType.CHANGED] = new LocalObjectMovedRemoteObjectChanged(this.session, this.storage, renameChangeSolver, changeChangeSolver);
            solver[(int)SituationType.RENAMED, (int)SituationType.CHANGED] = renameChangeSolver;
            solver[(int)SituationType.REMOVED, (int)SituationType.CHANGED] = new LocalObjectDeletedRemoteObjectChanged(this.session, this.storage);

            solver[(int)SituationType.NOCHANGE, (int)SituationType.MOVED] = new RemoteObjectMoved(this.session, this.storage);
            solver[(int)SituationType.CHANGED, (int)SituationType.MOVED] = new LocalObjectChangedRemoteObjectMoved(this.session, this.storage, changeChangeSolver);
            solver[(int)SituationType.MOVED, (int)SituationType.MOVED] = new LocalObjectMovedRemoteObjectMoved(this.session, this.storage);
            solver[(int)SituationType.RENAMED, (int)SituationType.MOVED] = new LocalObjectRenamedRemoteObjectMoved(this.session, this.storage, renameRenameSolver, changeChangeSolver);
            solver[(int)SituationType.REMOVED, (int)SituationType.MOVED] = new LocalObjectDeletedRemoteObjectRenamedOrMoved(this.session, this.storage);

            solver[(int)SituationType.NOCHANGE, (int)SituationType.RENAMED] = new RemoteObjectRenamed(this.session, this.storage);
            solver[(int)SituationType.CHANGED, (int)SituationType.RENAMED] = new LocalObjectChangedRemoteObjectRenamed(this.session, this.storage, changeChangeSolver);
            solver[(int)SituationType.MOVED, (int)SituationType.RENAMED] = new LocalObjectMovedRemoteObjectRenamed(this.session, this.storage, changeChangeSolver, renameRenameSolver);
            solver[(int)SituationType.RENAMED, (int)SituationType.RENAMED] = renameRenameSolver;
            solver[(int)SituationType.REMOVED, (int)SituationType.RENAMED] = new LocalObjectDeletedRemoteObjectRenamedOrMoved(this.session, this.storage);

            solver[(int)SituationType.NOCHANGE, (int)SituationType.REMOVED] = new RemoteObjectDeleted(this.session, this.storage, this.filters);
            solver[(int)SituationType.CHANGED, (int)SituationType.REMOVED] = new RemoteObjectDeleted(this.session, this.storage, this.filters);
            solver[(int)SituationType.MOVED, (int)SituationType.REMOVED] = new LocalObjectRenamedOrMovedRemoteObjectDeleted(this.session, this.storage, this.activityListener.TransmissionManager);
            solver[(int)SituationType.RENAMED, (int)SituationType.REMOVED] = new LocalObjectRenamedOrMovedRemoteObjectDeleted(this.session, this.storage, this.activityListener.TransmissionManager);
            solver[(int)SituationType.REMOVED, (int)SituationType.REMOVED] = new LocalObjectDeletedRemoteObjectDeleted(this.session, this.storage);

            return solver;
        }

        private void DoHandle(AbstractFolderEvent actualEvent)
        {
            SituationType localSituation = this.LocalSituation.Analyse(this.storage, actualEvent);
            SituationType remoteSituation = this.RemoteSituation.Analyse(this.storage, actualEvent);
            ISolver solver = this.Solver[(int)localSituation, (int)remoteSituation];

            if (solver == null) {
                throw new NotImplementedException(string.Format("Solver for LocalSituation: {0}, and RemoteSituation {1} not implemented", localSituation, remoteSituation));
            }

            Stopwatch watch = Stopwatch.StartNew();
            this.Solve(solver, actualEvent);
            watch.Stop();
            Logger.Debug(string.Format("Solver {0} took {1} ms", solver.GetType(), watch.ElapsedMilliseconds));
        }

        private void Solve(ISolver s, AbstractFolderEvent e)
        {
            using (var activity = new ActivityListenerResource(this.activityListener)) {
                if (e is FolderEvent) {
                    s.Solve((e as FolderEvent).LocalFolder, (e as FolderEvent).RemoteFolder, ContentChangeType.NONE, ContentChangeType.NONE);
                } else if (e is FileEvent) {
                    s.Solve((e as FileEvent).LocalFile, (e as FileEvent).RemoteFile, (e as FileEvent).LocalContent, (e as FileEvent).RemoteContent);
                }
            }
        }
    }
}
