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

namespace CmisSync.Lib.Sync.Strategy
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.SyncMechanism"/> class.
        /// </summary>
        /// <param name="localSituation">Local situation.</param>
        /// <param name="remoteSituation">Remote situation.</param>
        /// <param name="queue">Sync event queue.</param>
        /// <param name="session">CMIS Session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="solver">Solver for custom solver matrix.</param>
        public SyncMechanism(
            ISituationDetection<AbstractFolderEvent> localSituation,
            ISituationDetection<AbstractFolderEvent> remoteSituation,
            ISyncEventQueue queue,
            ISession session,
            IMetaDataStorage storage,
            ISolver[,] solver = null) : base(queue)
        {
            if (session == null) {
                throw new ArgumentNullException("Given session is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            if (localSituation == null) {
                throw new ArgumentNullException("Given local situation detection is null");
            }

            if (remoteSituation == null) {
                throw new ArgumentNullException("Given remote situation detection is null");
            }

            this.session = session;
            this.storage = storage;
            this.LocalSituation = localSituation;
            this.RemoteSituation = remoteSituation;
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
                } catch(Exception) {
                    Logger.Debug("Exception in SyncMechanism, requesting FullSync and rethrowing");
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
            solver[(int)SituationType.NOCHANGE, (int)SituationType.NOCHANGE] = new NothingToDoSolver();
            solver[(int)SituationType.ADDED, (int)SituationType.NOCHANGE] = new LocalObjectAdded(this.Queue);
            solver[(int)SituationType.CHANGED, (int)SituationType.NOCHANGE] = new LocalObjectChanged(this.Queue);
            solver[(int)SituationType.MOVED, (int)SituationType.NOCHANGE] = new LocalObjectMoved();
            solver[(int)SituationType.RENAMED, (int)SituationType.NOCHANGE] = new LocalObjectRenamed();
            solver[(int)SituationType.REMOVED, (int)SituationType.NOCHANGE] = new LocalObjectDeleted();

            solver[(int)SituationType.NOCHANGE, (int)SituationType.ADDED] = new RemoteObjectAdded(this.Queue);
            solver[(int)SituationType.ADDED, (int)SituationType.ADDED] = null;
            solver[(int)SituationType.CHANGED, (int)SituationType.ADDED] = null;
            solver[(int)SituationType.MOVED, (int)SituationType.ADDED] = null;
            solver[(int)SituationType.RENAMED, (int)SituationType.ADDED] = null;
            solver[(int)SituationType.REMOVED, (int)SituationType.ADDED] = null;

            solver[(int)SituationType.NOCHANGE, (int)SituationType.CHANGED] = new RemoteObjectChanged(this.Queue);
            solver[(int)SituationType.ADDED, (int)SituationType.CHANGED] = null;
            solver[(int)SituationType.CHANGED, (int)SituationType.CHANGED] = null;
            solver[(int)SituationType.MOVED, (int)SituationType.CHANGED] = null;
            solver[(int)SituationType.RENAMED, (int)SituationType.CHANGED] = null;
            solver[(int)SituationType.REMOVED, (int)SituationType.CHANGED] = null;

            solver[(int)SituationType.NOCHANGE, (int)SituationType.MOVED] = new RemoteObjectMoved();
            solver[(int)SituationType.ADDED, (int)SituationType.MOVED] = null;
            solver[(int)SituationType.CHANGED, (int)SituationType.MOVED] = null;
            solver[(int)SituationType.MOVED, (int)SituationType.MOVED] = null;
            solver[(int)SituationType.RENAMED, (int)SituationType.MOVED] = null;
            solver[(int)SituationType.REMOVED, (int)SituationType.MOVED] = null;

            solver[(int)SituationType.NOCHANGE, (int)SituationType.RENAMED] = new RemoteObjectRenamed();
            solver[(int)SituationType.ADDED, (int)SituationType.RENAMED] = null;
            solver[(int)SituationType.CHANGED, (int)SituationType.RENAMED] = null;
            solver[(int)SituationType.MOVED, (int)SituationType.RENAMED] = null;
            solver[(int)SituationType.RENAMED, (int)SituationType.RENAMED] = null;
            solver[(int)SituationType.REMOVED, (int)SituationType.RENAMED] = null;

            solver[(int)SituationType.NOCHANGE, (int)SituationType.REMOVED] = new RemoteObjectDeleted();
            solver[(int)SituationType.ADDED, (int)SituationType.REMOVED] = null;
            solver[(int)SituationType.CHANGED, (int)SituationType.REMOVED] = null;
            solver[(int)SituationType.MOVED, (int)SituationType.REMOVED] = null;
            solver[(int)SituationType.RENAMED, (int)SituationType.REMOVED] = null;
            solver[(int)SituationType.REMOVED, (int)SituationType.REMOVED] = null;

            return solver;
        }

        private void DoHandle(AbstractFolderEvent actualEvent)
        {
            SituationType localSituation = this.LocalSituation.Analyse(this.storage, actualEvent);
            SituationType remoteSituation = this.RemoteSituation.Analyse(this.storage, actualEvent);
            ISolver solver = this.Solver[(int)localSituation, (int)remoteSituation];

            if(solver == null) {
                throw new NotImplementedException(string.Format("Solver for LocalSituation: {0}, and RemoteSituation {1} not implemented", localSituation, remoteSituation));
            }

            Logger.Debug("Using Solver: " + solver.GetType());
            this.Solve(solver, actualEvent);
        }

        private void Solve(ISolver s, AbstractFolderEvent e)
        {
            if(e is FolderEvent) {
                s.Solve(this.session, this.storage, (e as FolderEvent).LocalFolder, (e as FolderEvent).RemoteFolder);
                // this.storage.ValidateObjectStructure();
            } else if (e is FileEvent) {
                s.Solve(this.session, this.storage, (e as FileEvent).LocalFile, (e as FileEvent).RemoteFile);
                // this.storage.ValidateObjectStructure();
            }
        }
    }
}
