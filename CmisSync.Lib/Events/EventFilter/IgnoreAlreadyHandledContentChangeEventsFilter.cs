//-----------------------------------------------------------------------
// <copyright file="IgnoreAlreadyHandledContentChangeEventsFilter.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events.Filter
{
    using System;

    using CmisSync.Lib.Storage;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using log4net;

    /// <summary>
    /// Filters already handled content change events.
    /// </summary>
    public class IgnoreAlreadyHandledContentChangeEventsFilter : SyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IgnoreAlreadyHandledContentChangeEventsFilter));

        private IMetaDataStorage storage;
        private ISession session;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Events.Filter.IgnoreAlreadyHandledContentChangeEventsFilter"/> class.
        /// </summary>
        /// <param name="storage">Storage instance.</param>
        /// <param name="session">Session instance.</param>
        public IgnoreAlreadyHandledContentChangeEventsFilter(IMetaDataStorage storage, ISession session)
        {
            if (storage == null) {
                throw new ArgumentNullException("Given storage instance is null");
            }

            if (session == null) {
                throw new ArgumentNullException("Given session instance is null");
            }

            this.storage = storage;
            this.session = session;
        }

        /// <summary>
        /// Checks is the given Event is a content change event and filters it, if it has been handled already.
        /// </summary>
        /// <param name="e">Sync event</param>
        /// <returns><c>true</c> if the event has been already handled, otherwise <c>false</c></returns>
        public override bool Handle(ISyncEvent e)
        {
            if (e is ContentChangeEvent) {
                ContentChangeEvent change = e as ContentChangeEvent;
                switch (change.Type) {
                case ChangeType.Created:
                    return this.storage.GetObjectByRemoteId(change.ObjectId) != null;
                case ChangeType.Deleted:
                    return this.storage.GetObjectByRemoteId(change.ObjectId) == null;
                case ChangeType.Security:
                    goto case ChangeType.Updated;
                case ChangeType.Updated:
                    var mappedObject = this.storage.GetObjectByRemoteId(change.ObjectId);
                    if(mappedObject == null || mappedObject.LastChangeToken == null) {
                        return false;
                    } else {
                        if (change.CmisObject == null) {
                            try {
                                change.UpdateObject(this.session);
                            } catch (DotCMIS.Exceptions.CmisBaseException) {
                                return false;
                            }
                        }

                        if (mappedObject.LastChangeToken == change.CmisObject.ChangeToken) {
                            Logger.Debug(string.Format("Ignoring remote change because the ChangeToken \"{0}\" is equal to the stored one", mappedObject.LastChangeToken));
                            return true;
                        } else {
                            return false;
                        }
                    }

                default:
                    return false;
                }
            } else {
                return false;
            }
        }
    }
}