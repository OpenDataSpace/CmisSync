using System;

namespace CmisSync.Lib.Events
{
    public delegate void ShowChangePasswordEventHandler (string reponame);

    public class PermissionDeniedEventHandler : SyncEventHandler
    {

        private string Repo;
        private ShowChangePasswordEventHandler Callback;
        private bool Execute = true;

        public PermissionDeniedEventHandler (string repo, ShowChangePasswordEventHandler callback)
        {
            if (String.IsNullOrEmpty (repo))
                throw new ArgumentNullException ("Given repo is null or empty");
            if (callback == null)
                throw new ArgumentNullException ("Given callback is null");
            Repo = repo;
            Callback = callback;
        }

        public override bool Handle (ISyncEvent e)
        {
            if (e is PermissionDeniedEvent) {
                if (Execute) {
                    Callback (Repo);
                    Execute = false;
                }
                return true;
            }
            if (e is ConfigChangedEvent || e is SuccessfulLoginEvent) {
                Execute = true;
            }
            return false;
        }
    }
}

