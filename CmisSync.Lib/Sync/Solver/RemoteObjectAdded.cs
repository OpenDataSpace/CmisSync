//-----------------------------------------------------------------------
// <copyright file="RemoteObjectAdded.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Sync.Solver
{
    using System;
    using System.IO;
 
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    
    using DotCMIS.Client;
  
    using log4net;
 
    /// <summary>
    /// Solver to handle a new object which has been found on the server
    /// </summary>
    public class RemoteObjectAdded : ISolver
    {
        /// <summary>
        /// Adds the Object to Disk and Database
        /// </summary>
        /// <param name='session'>
        /// Cmis Session.
        /// </param>
        /// <param name='storage'>
        /// Metadata Storage.
        /// </param>
        /// <param name='localFile'>
        /// Local file.
        /// </param>
        /// <param name='remoteId'>
        /// Remote Object (already fetched).
        /// </param>
        /// <exception cref='ArgumentException'>
        /// Is thrown when remoteId is not prefetched.
        /// </exception>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId) {
            if(localFile is IDirectoryInfo) {
                if(!(remoteId is IFolder)) {
                    throw new ArgumentException("remoteId has to be a prefetched Folder");
                }

                var remoteFolder = remoteId as IFolder;
                IDirectoryInfo localFolder = (localFile as IDirectoryInfo);
                localFolder.Create();

                if(remoteFolder.LastModificationDate != null)
                {
                    localFolder.LastWriteTimeUtc = (DateTime)remoteFolder.LastModificationDate;
                }

                Guid uuid = Guid.Empty;
                if (localFolder.IsExtendedAttributeAvailable())
                {
                    uuid = Guid.NewGuid();
                    localFolder.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, uuid.ToString());
                }

                var mappedObject = new MappedObject(remoteFolder);
                mappedObject.Guid = uuid;
                storage.SaveMappedObject(mappedObject);
            }
        }
    }
}