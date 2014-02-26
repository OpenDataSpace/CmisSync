using System;
using System.IO;

using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Events;

using DotCMIS.Client;

namespace CmisSync.Lib.Sync.Strategy
{
    public class SyncMechanism : ReportingSyncEventHandler
    {
        public static readonly int DEFAULT_PRIORITY = 1;
        private ISession Session;
        private IMetaDataStorage Storage;

        public override int Priority {
            get {
                return DEFAULT_PRIORITY;
            }
        }

        public ISolver[,] Solver;
        public ISituationDetection<IFileSystemInfo> LocalSituation;
        public ISituationDetection<IObjectId> RemoteSituation;

        public SyncMechanism (ISituationDetection<IFileSystemInfo> localSituation,
                              ISituationDetection<IObjectId> remoteSituation,
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
                HandleMetaData ((e as FolderEvent).LocalFolder,(e as FolderEvent).RemoteFolder);
                return true;
            }
            if (e is FileEvent) {
                HandleFileEvent ((e as FileEvent));
                return true;
            }
            return false;
        }

        private void HandleMetaData (IFileSystemInfo localObject, IObjectId remoteObject)
        {
            int localSituation = (int)LocalSituation.Analyse (Storage, localObject);
            int remoteSituation = (int)RemoteSituation.Analyse (Storage, remoteObject);
            ISolver solver = Solver [localSituation, remoteSituation];
            if (solver != null)
            try{
                solver.Solve(Session, Storage, localObject, remoteObject);
            }catch(DotCMIS.Exceptions.CmisBaseException) {
                int newLocalSituation = (int) LocalSituation.Analyse(Storage, localObject);
                int newRemoteSituation = (int) RemoteSituation.Analyse(Storage, remoteObject);
                solver = Solver [newLocalSituation, newRemoteSituation];
                if(solver != null)
                    solver.Solve(Session, Storage, localObject, remoteObject);
            }
                
        }

        private void HandleFileEvent (FileEvent file)
        {
            HandleMetaData (file.LocalFile, file.RemoteFile);
            //TODO Content sync if Situation is 
        }
    }
}

