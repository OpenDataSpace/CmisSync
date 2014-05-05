using System;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using CmisSync.Lib.Events;

using log4net;

namespace CmisSync.Lib.Sync.Strategy
{
    public class ContentChangeEventAccumulator : ReportingSyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentChangeEventAccumulator));

        /// this has to run before ContentChangeEventTransformer
        public static readonly int DEFAULT_PRIORITY = 2000;

        private ISession session;

        public override int Priority {
            get {
                return DEFAULT_PRIORITY;
            }
        }

        public override bool Handle (ISyncEvent e) {
            if(!(e is ContentChangeEvent)){
                return false;
            }

            var contentChangeEvent = e as ContentChangeEvent;
            if(contentChangeEvent.Type != DotCMIS.Enums.ChangeType.Deleted) {
                try{
                    contentChangeEvent.UpdateObject(session);
                }catch(CmisObjectNotFoundException){
                    Logger.Debug("Object with id " + contentChangeEvent.ObjectId + " has been deleted - ignore"); 
                    return true;
                }catch(CmisPermissionDeniedException){
                    Logger.Debug("Object with id " + contentChangeEvent.ObjectId + " gives Access Denied: ACL changed - ignore"); 
                    return true;
                }catch(Exception ex){
                    Logger.Warn("Unable to fetch object " + contentChangeEvent.ObjectId + " starting CrawlSync");
                    Logger.Debug(ex.StackTrace);
                    Queue.AddEvent(new StartNextSyncEvent(true));
                    return true;
                }
            }
            return false;
        }

        public ContentChangeEventAccumulator(ISession session, ISyncEventQueue queue) : base(queue) {
            if(session == null)
                throw new ArgumentNullException("Session instance is needed for the ContentChangeEventAccumulator, but was null");
            this.session = session;
        }
    }
}
