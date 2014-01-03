using System;
using CmisSync.Lib.Cmis;
namespace CmisSync.Lib.Events.Filter
{
    public class FailedOperationsFilter : AbstractFileFilter
    {
        private static readonly int DEFAULT_FILTER_PRIORITY = 9998;
        private Database database;
        public override int Priority {get {return DEFAULT_FILTER_PRIORITY;} }

        public long MaxUploadRetries { get; set; }

        public long MaxDownloadRetries { get; set; }

        public long MaxDeletionRetries { get; set; }

        public FailedOperationsFilter (Database db, SyncEventQueue queue) : base(queue)
        {
            if(db == null)
                throw new ArgumentNullException("The given database instance must not be null");
            this.database = db;
        }

        public override bool Handle (ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if(request!=null)
            {
                long counter = database.GetOperationRetryCounter(request.TargetFilePath ,Database.OperationType.DOWNLOAD);
                if( counter > MaxDownloadRetries)
                {
                    string reason = String.Format("Skipping download of file {0} because of too many failed ({1}) downloads", request.TargetFilePath , counter);
                    Queue.AddEvent(new RequestIgnoredEvent(e, reason:reason));
                    return true;
                }
            }
            return false;
        }
    }
}

