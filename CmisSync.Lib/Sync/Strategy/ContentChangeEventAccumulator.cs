//-----------------------------------------------------------------------
// <copyright file="ContentChangeEventAccumulator.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
using System;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using CmisSync.Lib.Events;

using log4net;

namespace CmisSync.Lib.Sync.Strategy
{
    public class ContentChangeEventAccumulator : ReportingSyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentChangeEventAccumulator));

        private ISession session;

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
