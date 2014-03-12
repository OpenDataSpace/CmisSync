using System;

using log4net;

namespace CmisSync.Lib.Events
{
    public class DebugLoggingHandler : SyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DebugLoggingHandler));

        public override bool Handle(ISyncEvent e)
        {
            Logger.Debug("Incomming Event: " + e.ToString());
            return false;
        }
    }
}

