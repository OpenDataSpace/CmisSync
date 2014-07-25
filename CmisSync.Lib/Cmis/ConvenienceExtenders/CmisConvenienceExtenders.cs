//-----------------------------------------------------------------------
// <copyright file="CmisConvenienceExtenders.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis.ConvenienceExtenders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using CmisSync.Lib.Cmis;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Data.Impl;

    /// <summary>
    /// Cmis convenience extenders.
    /// </summary>
    public static class CmisConvenienceExtenders
    {
        /// <summary>
        /// Creates a sub folder with the given name.
        /// </summary>
        /// <returns>The created folder.</returns>
        /// <param name="folder">parent folder.</param>
        /// <param name="name">Name of the new sub folder.</param>
        public static IFolder CreateFolder(this IFolder folder, string name)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");

            return folder.CreateFolder(properties);
        }

        /// <summary>
        /// Creates a document.
        /// </summary>
        /// <returns>The document.</returns>
        /// <param name="folder">Parent folder.</param>
        /// <param name="name">Name of the document.</param>
        /// <param name="content">If content is not null, a content stream containing the given content will be added.</param>
        public static IDocument CreateDocument(this IFolder folder, string name, string content)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");

            if (string.IsNullOrEmpty(content)) {
                return folder.CreateDocument(properties, null, null);
            }

            ContentStream contentStream = new ContentStream();
            contentStream.FileName = name;
            contentStream.MimeType = MimeType.GetMIMEType(name);
            contentStream.Length = content.Length;
            contentStream.Stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            return folder.CreateDocument(properties, contentStream, null);
        }

        /// <summary>
        /// Sets the content stream of the document.
        /// </summary>
        /// <returns>The content.</returns>
        /// <param name="doc">Remote document.</param>
        /// <param name="content">New content as string.</param>
        /// <param name="overwrite">If set to <c>true</c> overwrites existing content.</param>
        /// <param name="refresh">If set to <c>true</c> refreshs the original remote doc instance.</param>
        public static IObjectId SetContent(this IDocument doc, string content, bool overwrite = true, bool refresh = true)
        {
            ContentStream contentStream = new ContentStream();
            contentStream.FileName = doc.Name;
            contentStream.MimeType = MimeType.GetMIMEType(doc.Name);
            contentStream.Length = content.Length;
            contentStream.Stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            return doc.SetContentStream(contentStream, overwrite, refresh);
        }

        /// <summary>
        /// Updates the last write time in UTC via UpdateProperties
        /// </summary>
        /// <returns>The result of UpdateProperties.</returns>
        /// <param name="obj">Fileable cmis object.</param>
        /// <param name="modificationDate">Modification date.</param>
        public static ICmisObject UpdateLastWriteTimeUtc(this IFileableCmisObject obj, DateTime modificationDate) {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.LastModificationDate, modificationDate);
            return obj.UpdateProperties(properties);
        }
    }
}