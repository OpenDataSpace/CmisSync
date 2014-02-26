using System;
using CmisSync.Lib.Events;

namespace CmisSync.Lib.Events.Filter
{
    /// <summary>
    /// Ignored files filter.
    /// TODO Should be implemented in the future, if explicit files could be ignored
    /// </summary>
    public class IgnoredFilesFilter : AbstractFileFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.IgnoredFilesFilter"/> class.
        /// Throws Exception while it is implemented.
        /// </summary>
        /// <param name='queue'>
        /// Queue.
        /// </param>
        public IgnoredFilesFilter (ISyncEventQueue queue) : base(queue)
        {
            throw new NotImplementedException("IgnoredFilesFilter is not yet implemented, because there is no possibility to ignore excact files");
        }


        public override bool Handle (ISyncEvent e)
        {
            FileDownloadRequest request = e as FileDownloadRequest;
            if(request == null)
                return false;
            //TODO If files could be ignored, they should be filtered out here
            return false;
        }
    }
}

