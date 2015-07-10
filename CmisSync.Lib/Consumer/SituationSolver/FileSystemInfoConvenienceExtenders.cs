//-----------------------------------------------------------------------
// <copyright file="FileSystemInfoConvenienceExtenders.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Consumer.SituationSolver {
    using System;
    using System.IO;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// File system info convenience extenders for often needed operations on file system.
    /// </summary>
    public static class FileSystemInfoConvenienceExtenders {
        /// <summary>
        /// Tries the read only state of the remote object to the local file system object. If an exception is thrown, it will be passed to the given logger with log level info.
        /// </summary>
        /// <param name="to">Sets the read only state to the local file system object</param>
        /// <param name="from">Takes the read only state from this remote object</param>
        /// <param name="andLogErrorsTo">Logs errors to the given logger or ignores errors if logger is null.</param>
        public static void TryToSetReadOnlyState(this IFileSystemInfo to, ICmisObject from, ILog andLogErrorsTo = null) {
            if (from == null) {
                throw new ArgumentNullException("from");
            }

            if (to == null) {
                throw new ArgumentNullException("to");
            }

            try {
                to.ReadOnly = from.IsReadOnly();
            } catch (IOException e) {
                if (andLogErrorsTo != null) {
                    andLogErrorsTo.Info(string.Format("Cannot set {0} permission to {1}", from.IsReadOnly() ? "read only" : "read write", to.FullName), e);
                }
            }
        }

        /// <summary>
        /// Sets the local read only state to the remote object if the local state is different to the remote state.
        /// </summary>
        /// <param name="localState">Local state.</param>
        /// <param name="from">From.</param>
        /// <param name="andLogErrorsTo">And log errors to.</param>
        public static void TryToSetReadOnlyStateIfDiffers(this IFileSystemInfo localState, ICmisObject from, ILog andLogErrorsTo = null) {
            if (from == null) {
                throw new ArgumentNullException("from");
            }

            if (localState == null) {
                throw new ArgumentNullException("localState");
            }

            if (from.IsReadOnly() != localState.ReadOnly) {
                localState.TryToSetReadOnlyState(from: from, andLogErrorsTo: andLogErrorsTo);
            }
        }

        /// <summary>
        /// Tries to set last write time UTC from remote object to local file if available.
        /// </summary>
        /// <param name="localFile">Local file.</param>
        /// <param name="from">Remote object.</param>
        /// <param name="andLogErrorsTo">Logs error to given logger if not null.</param>
        public static void TryToSetLastWriteTimeUtcIfAvailable(this IFileSystemInfo localFile, ICmisObject from, ILog andLogErrorsTo = null) {
            if (localFile == null) {
                throw new ArgumentNullException("localFile");
            }

            if (from == null) {
                throw new ArgumentNullException("from");
            }

            if (from.LastModificationDate != null) {
                try {
                    localFile.LastWriteTimeUtc = (DateTime)from.LastModificationDate;
                } catch (IOException e) {
                    if (andLogErrorsTo != null) {
                        andLogErrorsTo.Debug("Couldn't set the server side modification date", e);
                    }
                }
            }
        }
    }
}