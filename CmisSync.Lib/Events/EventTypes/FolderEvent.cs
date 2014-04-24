//-----------------------------------------------------------------------
// <copyright file="FolderEvent.cs" company="GRAU DATA AG">
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
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Storage;

namespace CmisSync.Lib.Events
{
    public class FolderEvent : AbstractFolderEvent
    {
        public bool Recursive { get; set; }

        public IDirectoryInfo LocalFolder { get; set; }

        public IFolder RemoteFolder { get; set; }

        public FolderEvent (IDirectoryInfo localFolder = null, IFolder remoteFolder = null)
        {
            if(localFolder == null && remoteFolder == null)
                throw new ArgumentNullException("One of the given folders must not be null");
            Recursive = false;
            LocalFolder = localFolder;
            RemoteFolder = remoteFolder;
        }

        public override string ToString ()
        {
            return string.Format ("[FolderEvent: Local={0} on {2}, Remote={1} on {3}]",
                                  Local,
                                  Remote,
                                  LocalFolder != null ? LocalFolder.Name : "",
                                  RemoteFolder!= null ? RemoteFolder.Name : "");
        }
    }
}

