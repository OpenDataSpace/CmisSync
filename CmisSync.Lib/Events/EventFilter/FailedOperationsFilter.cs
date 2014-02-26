using System;
using CmisSync.Lib.Cmis;

namespace CmisSync.Lib.Events.Filter
{
    /// <summary>
    /// Failed operations filter. If any requested operation is failed too often, it will be ignored and filtered out.
    /// </summary>
    public class FailedOperationsFilter : AbstractFileFilter
    {
        private static readonly int DEFAULT_FILTER_PRIORITY = 9998;

        /// <summary>
        /// Returns the default filter priority and cannot be changed during runtime.
        /// </summary>
        /// <value>
        /// The priority is 9998.
        /// </value>
        public override int Priority { get { return DEFAULT_FILTER_PRIORITY; } }

        /// <summary>
        /// Gets or sets the max upload retries.
        /// </summary>
        /// <value>
        /// The max upload retries.
        /// </value>
        public long MaxUploadRetries { get; set; }

        /// <summary>
        /// Gets or sets the max download retries.
        /// </summary>
        /// <value>
        /// The max download retries.
        /// </value>
        public long MaxDownloadRetries { get; set; }

        /// <summary>
        /// Gets or sets the max deletion retries.
        /// </summary>
        /// <value>
        /// The max deletion retries.
        /// </value>
        public long MaxDeletionRetries { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.FailedOperationsFilter"/> class.
        /// </summary>
        /// <param name='db'>
        /// Database where the counters are saved persistantly.
        /// </param>
        /// <param name='queue'>
        /// Queue where ignored requests are reported to.
        /// </param>
        public FailedOperationsFilter (ISyncEventQueue queue) : base(queue)
        {
            throw new NotImplementedException("This is a draft, don't use");

        }

        /// <summary>
        /// Filters all specific requested operations it the maximum retry counter is reached.
        /// </summary>
        /// <param name='e'>
        /// If e is a request for a already too often failed operation, true will be returned.
        /// </param>
        public override bool Handle (ISyncEvent e)
        {
            return false;
        }
    }
}

