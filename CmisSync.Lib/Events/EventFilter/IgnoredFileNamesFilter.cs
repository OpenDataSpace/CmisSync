using System;
using System.IO;

namespace CmisSync.Lib.Events.Filter
{
    /// <summary>
    /// Ignored file names filter.
    /// </summary>
    public class IgnoredFileNamesFilter : AbstractFileFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.IgnoredFileNamesFilter"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue.
        /// </param>
        public IgnoredFileNamesFilter(SyncEventQueue queue) : base(queue) { }

        /// <summary>
        /// Checks the filename for valid regex.
        /// </summary>
        /// <returns>
        /// The file.
        /// </returns>
        /// <param name='e'>
        /// If set to <c>true</c> e.
        /// </param>
        /// <param name='fileName'>
        /// If set to <c>true</c> file name.
        /// </param>
        private bool checkFile(ISyncEvent e, string fileName) {
            if(!Utils.WorthSyncing(fileName)) {
                Queue.AddEvent(new RequestIgnoredEvent(e, source: this));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles FSEvents and FileDownloadRequest events.
        /// </summary>
        /// <param name='e'>
        /// If a filename contains invalid patterns, <c>true</c> is returned and the filtering is reported to the queue. Otherwise <c>false</c> is returned.
        /// </param>
        public override bool Handle (ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if( request != null)
                return checkFile(request, request.Document.Name);

            FSEvent fsevent = e as FSEvent;
            if(fsevent!=null)
            {
                try{
                    if(!fsevent.IsDirectory())
                        return checkFile(fsevent, Path.GetFileName(fsevent.Path));
                }catch(System.IO.FileNotFoundException) {
                    // Only happens, if the deleted file/folder does not exists anymore
                    // To be sure, this event is not misinterpreted, just let it pass
                    return false;
                }
            }
            return false;
        }
    }
}

