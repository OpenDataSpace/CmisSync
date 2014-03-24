using System;

namespace CmisSync.Lib.Events
{
    /// <summary>
    /// Proxy auth required event.
    /// </summary>
    public class ProxyAuthRequiredEvent : ExceptionEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.ProxyAuthRequiredEvent"/> class.
        /// </summary>
        /// <param name='reponame'>
        /// Reponame.
        /// </param>
        public ProxyAuthRequiredEvent (DotCMIS.Exceptions.CmisRuntimeException e) : base (e) { }
    }
}

