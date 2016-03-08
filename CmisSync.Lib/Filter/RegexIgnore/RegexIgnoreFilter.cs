//-----------------------------------------------------------------------
// <copyright file="RegexIgnoreFilter.cs" company="GRAU DATA AG">
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
﻿
namespace CmisSync.Lib.Filter.RegexIgnore {
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Queueing;

    using DotCMIS.Client;

    public class RegexIgnoreFilter : SyncEventHandler {
        private IPathMatcher matcher;
        private IgnoredFolderNameFilter filter;
        public RegexIgnoreFilter(IPathMatcher matcher, IgnoredFolderNameFilter folderNameFilter) {
            if (matcher == null) {
                throw new ArgumentNullException("matcher");
            }

            if (folderNameFilter == null) {
                throw new ArgumentNullException("folderNameFilter");
            }

            this.matcher = matcher;
            this.filter = folderNameFilter;
        }

        public override bool Handle(ISyncEvent e) {
            var filterableRemoteObjectEvent = e as IFilterableRemoteObjectEvent;
            if (filterableRemoteObjectEvent != null) {
                var remoteObject = filterableRemoteObjectEvent.RemoteObject as IFileableCmisObject;
                if (remoteObject != null) {
                    var localPath = this.matcher.CreateLocalPath(remoteObject.Paths[0]);
                    string reason;
                    if (filter.CheckFolderPath(localPath, out reason)) {
                        return true;
                    }
                }
            }

            var filterableLocalObjectEvent = e as IFilterableLocalPathEvent;
            if (filterableLocalObjectEvent != null) {
                var path = filterableLocalObjectEvent.LocalPath;
                string reason;
                if (this.filter.CheckFolderPath(path, out reason)) {
                    return true;
                }
            }

            return false;
        }
    }
}