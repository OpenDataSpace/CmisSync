using System;
using System.IO;

using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Events;

using DotCMIS.Client;

namespace CmisSync.Lib.Sync.Strategy
{
    using log4net;
    public class SyncMechanism : ReportingSyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SyncMechanism));

        private ISession Session;
        private IMetaDataStorage Storage;

        public ISolver[,] Solver;
        public ISituationDetection<AbstractFolderEvent> LocalSituation;
        public ISituationDetection<AbstractFolderEvent> RemoteSituation;

        public SyncMechanism (ISituationDetection<AbstractFolderEvent> localSituation,
                              ISituationDetection<AbstractFolderEvent> remoteSituation,
                              ISyncEventQueue queue,
                              ISession session,
                              IMetaDataStorage storage,
                              ISolver[,] solver = null) : base(queue)
        {
            if (session == null)
                throw new ArgumentNullException ("Given session is null");
            if (storage == null)
                throw new ArgumentNullException ("Given storage is null");
            if (localSituation == null)
                throw new ArgumentNullException ("Given local situation detection is null");
            if (remoteSituation == null)
                throw new ArgumentNullException ("Given remote situation detection is null");
            Session = session;
            Storage = storage;
            LocalSituation = localSituation;
            RemoteSituation = remoteSituation;
            Solver = (solver == null) ? createSolver () : solver;
        }

        private ISolver[,] createSolver ()
        {
            int dim = Enum.GetNames (typeof(SituationType)).Length;
            ISolver[,] solver = new ISolver[dim, dim];
            solver [(int)SituationType.NOCHANGE, (int)SituationType.NOCHANGE] = new NothingToDoSolver();
            solver [(int)SituationType.ADDED, (int)SituationType.NOCHANGE] = new LocalObjectAdded ();
            solver [(int)SituationType.CHANGED, (int)SituationType.NOCHANGE] = new LocalObjectChanged ();
            solver [(int)SituationType.MOVED, (int)SituationType.NOCHANGE] = new LocalObjectMoved ();
            solver [(int)SituationType.RENAMED, (int)SituationType.NOCHANGE] = new LocalObjectRenamed ();
            solver [(int)SituationType.REMOVED, (int)SituationType.NOCHANGE] = new LocalObjectDeleted ();

            solver [(int)SituationType.NOCHANGE, (int)SituationType.ADDED] = new RemoteObjectAdded ();
            solver [(int)SituationType.ADDED, (int)SituationType.ADDED] = null;
            solver [(int)SituationType.CHANGED, (int)SituationType.ADDED] = null;
            solver [(int)SituationType.MOVED, (int)SituationType.ADDED] = null;
            solver [(int)SituationType.RENAMED, (int)SituationType.ADDED] = null;
            solver [(int)SituationType.REMOVED, (int)SituationType.ADDED] = null;

            solver [(int)SituationType.NOCHANGE, (int)SituationType.CHANGED] = new RemoteObjectChanged ();
            solver [(int)SituationType.ADDED, (int)SituationType.CHANGED] = null;
            solver [(int)SituationType.CHANGED, (int)SituationType.CHANGED] = null;
            solver [(int)SituationType.MOVED, (int)SituationType.CHANGED] = null;
            solver [(int)SituationType.RENAMED, (int)SituationType.CHANGED] = null;
            solver [(int)SituationType.REMOVED, (int)SituationType.CHANGED] = null;

            solver [(int)SituationType.NOCHANGE, (int)SituationType.MOVED] = new RemoteObjectMoved ();
            solver [(int)SituationType.ADDED, (int)SituationType.MOVED] = null;
            solver [(int)SituationType.CHANGED, (int)SituationType.MOVED] = null;
            solver [(int)SituationType.MOVED, (int)SituationType.MOVED] = null;
            solver [(int)SituationType.RENAMED, (int)SituationType.MOVED] = null;
            solver [(int)SituationType.REMOVED, (int)SituationType.MOVED] = null;

            solver [(int)SituationType.NOCHANGE, (int)SituationType.RENAMED] = new RemoteObjectRenamed ();
            solver [(int)SituationType.ADDED, (int)SituationType.RENAMED] = null;
            solver [(int)SituationType.CHANGED, (int)SituationType.RENAMED] = null;
            solver [(int)SituationType.MOVED, (int)SituationType.RENAMED] = null;
            solver [(int)SituationType.RENAMED, (int)SituationType.RENAMED] = null;
            solver [(int)SituationType.REMOVED, (int)SituationType.RENAMED] = null;

            solver [(int)SituationType.NOCHANGE, (int)SituationType.REMOVED] = new RemoteObjectDeleted ();
            solver [(int)SituationType.ADDED, (int)SituationType.REMOVED] = null;
            solver [(int)SituationType.CHANGED, (int)SituationType.REMOVED] = null;
            solver [(int)SituationType.MOVED, (int)SituationType.REMOVED] = null;
            solver [(int)SituationType.RENAMED, (int)SituationType.REMOVED] = null;
            solver [(int)SituationType.REMOVED, (int)SituationType.REMOVED] = null;

            return solver;
        }

        public override bool Handle (ISyncEvent e)
        {
            if (e is FolderEvent) {
                var folderEvent = e as FolderEvent;

                if(folderEvent.LocalFolder == null) {
                    throw new ArgumentException("LocalFolder has to be filled: " + folderEvent);
                }

                HandleMetaData (folderEvent);
                return true;
            }
            if (e is FileEvent) {
                var fileEvent = e as FileEvent;

                if(fileEvent.LocalFile == null) {
                    throw new ArgumentException("LocalFile has to be filled " + fileEvent);
                }

                HandleFileEvent (fileEvent);
                return true;
            }
            return false;
        }

        private void HandleMetaData (AbstractFolderEvent actualEvent)
        {
            int localSituation = (int)LocalSituation.Analyse (Storage, actualEvent);
            int remoteSituation = (int)RemoteSituation.Analyse (Storage, actualEvent);
            ISolver solver = Solver [localSituation, remoteSituation];
            if (solver != null)
            try{
                Logger.Debug("Using Solver: " + solver.GetType());
                Solve (solver, actualEvent);
            }catch(DotCMIS.Exceptions.CmisBaseException) {
                int newLocalSituation = (int) LocalSituation.Analyse(Storage, actualEvent);
                int newRemoteSituation = (int) RemoteSituation.Analyse(Storage, actualEvent);
                solver = Solver [newLocalSituation, newRemoteSituation];
                if(solver != null)
                    Solve (solver, actualEvent);
            }
        }

        private void HandleFileEvent (FileEvent file)
        {
            HandleMetaData (file);
            //TODO Content sync if Situation is 
        }

        private void Solve(ISolver s, AbstractFolderEvent e)
        {

            if(e is FolderEvent)
            {
                s.Solve(Session, Storage, (e as FolderEvent).LocalFolder, (e as FolderEvent).RemoteFolder);
            }
            else if( e is FileEvent)
            {
                s.Solve(Session, Storage, (e as FileEvent).LocalFile, (e as FileEvent).RemoteFile);
            }
        }
    }
}

