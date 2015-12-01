//-----------------------------------------------------------------------
// <copyright file="FolderExtenders.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Cmis.ConvenienceExtenders {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Data.Impl;
    using DotCMIS.Enums;

    /// <summary>
    /// DotCMIS Folder extenders.
    /// </summary>
    public static class FolderExtenders {
        /// <summary>
        /// Creates a sub folder with the given name.
        /// </summary>
        /// <returns>The created folder.</returns>
        /// <param name="folder">parent folder.</param>
        /// <param name="name">Name of the new sub folder.</param>
        public static IFolder CreateFolder(this IFolder folder, string name) {
            if (folder == null) {
                throw new ArgumentNullException("folder");
            }

            var properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);
            properties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisFolder.GetCmisValue());

            return folder.CreateFolder(properties);
        }

        /// <summary>
        /// Creates a document.
        /// </summary>
        /// <returns>The document.</returns>
        /// <param name="folder">Parent folder.</param>
        /// <param name="name">Name of the document.</param>
        /// <param name="content">If content is not null, a content stream containing the given content will be added.</param>
        /// <param name="checkedOut">If true, the new document will be created in checked out state.</param>
        public static IDocument CreateDocument(this IFolder folder, string name, string content, bool checkedOut = false) {
            if (folder == null) {
                throw new ArgumentNullException("folder");
            }

            return folder.CreateDocument(name, string.IsNullOrEmpty(content) ? (byte[])null : Encoding.UTF8.GetBytes(content), checkedOut);
        }

        /// <summary>
        /// Creates a document.
        /// </summary>
        /// <returns>The document.</returns>
        /// <param name="folder">Parent folder.</param>
        /// <param name="name">Name of the document.</param>
        /// <param name="content">If content is not null, a content stream containing the given content will be added.</param>
        /// <param name="checkedOut">If true, the new document will be created in checked out state.</param>
        public static IDocument CreateDocument(this IFolder folder, string name, byte[] content, bool checkedOut = false) {
            if (folder == null) {
                throw new ArgumentNullException("folder");
            }

            var properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);
            properties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisDocument.GetCmisValue());

            if (content == null) {
                return folder.CreateDocument(properties, null, checkedOut ? (VersioningState?)VersioningState.CheckedOut : (VersioningState?)null);
            }

            var contentStream = new ContentStream() {
                FileName = name,
                MimeType = MimeType.GetMIMEType(name),
                Length = content.Length
            };
            IDocument doc = null;
            using (var stream = new MemoryStream(content)) {
                contentStream.Stream = stream;
                doc = folder.CreateDocument(properties, contentStream, checkedOut ? (VersioningState?)VersioningState.CheckedOut : (VersioningState?)null);
            }

            return doc;
        }

        /// <summary>
        /// Sets a flag to ignores all children of this folder.
        /// </summary>
        /// <param name="folder">Folder which children should be ignored.</param>
        /// <param name="deviceId">Device Ids which should be ignored.</param>
        public static void IgnoreAllChildren(this IFolder folder, string deviceId = "*") {
            if (string.IsNullOrEmpty(deviceId)) {
                throw new ArgumentException("Given deviceId is null or empty");
            }

            if (folder == null) {
                throw new ArgumentNullException("folder");
            }

            var properties = new Dictionary<string, object>();
            var devices = folder.IgnoredDevices();
            if (!devices.Contains(deviceId.ToLower())) {
                devices.Add(deviceId.ToLower());
            }

            properties.Add("gds:ignoreDeviceIds", devices);
            var ids = folder.SecondaryObjectTypeIds();
            if (!ids.Contains("gds:sync")) {
                ids.Add("gds:sync");
                properties.Add(PropertyIds.SecondaryObjectTypeIds, ids);
            }

            folder.UpdateProperties(properties, true);
        }
    }
}