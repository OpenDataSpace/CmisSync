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

namespace TestLibrary.TestUtils
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using CmisSync.Lib.Cmis;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Data.Impl;

    public static class CmisConvenienceExtenders
    {
        public static IFolder CreateFolder(this IFolder folder, string name)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.Name, name);
            properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");

            return folder.CreateFolder(properties);
        }

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

        public static IObjectId SetContent(this IDocument doc, string content, bool overwrite = true, bool refresh = true)
        {
            ContentStream contentStream = new ContentStream();
            contentStream.FileName = doc.Name;
            contentStream.MimeType = MimeType.GetMIMEType(doc.Name);
            contentStream.Length = content.Length;
            contentStream.Stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            return doc.SetContentStream(contentStream, overwrite, refresh);
        }
    }
}