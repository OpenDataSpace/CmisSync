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
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content))) {
                contentStream.Stream = stream;
                return folder.CreateDocument(properties, contentStream, null);
            }
        }

        /// <summary>
        /// Returns the hash of the content stream on the server.
        /// </summary>
        /// <returns>The hash.</returns>
        /// <param name="doc">Document with the content stream.</param>
        /// <param name="type">Type of the requested hash.</param>
        public static byte[] ContentStreamHash(this IDocument doc, string type = "SHA-1") {
            if (doc.Properties == null) {
                return null;
            }

            string prefix = string.Format("{{{0}}}", type.ToLower());
            foreach (var prop in doc.Properties) {
                if (prop.Id == "cmis:contentStreamHash") {
                    if (prop.Values != null) {
                        foreach (string entry in prop.Values) {
                            if (entry.StartsWith(prefix)) {
                                return StringToByteArray(entry.Substring(prefix.Length));
                            }
                        }
                    }
                }
            }

            return null;
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
            byte[] c = Encoding.UTF8.GetBytes(content);
            contentStream.Length = c.LongLength;
            using (var stream = new MemoryStream(c)) {
                contentStream.Stream = stream;
                return doc.SetContentStream(contentStream, overwrite, refresh);
            }
        }

        /// <summary>
        /// Updates the last write time in UTC via UpdateProperties
        /// </summary>
        /// <returns>The result of UpdateProperties.</returns>
        /// <param name="obj">Fileable cmis object.</param>
        /// <param name="modificationDate">Modification date.</param>
        public static IObjectId UpdateLastWriteTimeUtc(this IFileableCmisObject obj, DateTime modificationDate) {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.LastModificationDate, modificationDate);
            return obj.UpdateProperties(properties, true);
        }

        /// <summary>
        /// Sets a flag to ignores all children of this folder.
        /// </summary>
        /// <param name="folder">Folder which children should be ignored.</param>
        public static void IgnoreAllChildren(this IFolder folder, string deviceId = "*") {
            if (deviceId == null) {
                throw new ArgumentException("Given deviceId is null or empty");
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            IList<string> devices = folder.IgnoredDevices();
            if (!devices.Contains(deviceId.ToLower())) {
                devices.Add(deviceId.ToLower());
            }

            properties.Add("gds:ignoreDeviceIds", devices);
            IList<string> ids = folder.SecondaryObjectTypeIds();
            if (!ids.Contains("gds:sync")) {
                ids.Add("gds:sync");
                properties.Add(PropertyIds.SecondaryObjectTypeIds, ids);
            }

            folder.UpdateProperties(properties, true);
        }

        public static void RemoveSyncIgnore(this ICmisObject obj, params Guid[] deviceIds) {
            string[] ids = new string[deviceIds.Length];
            for (int i = 0; i < deviceIds.Length; i++) {
                ids[i] = deviceIds[i].ToString().ToLower();
            }
        }

        public static void RemoveSyncIgnore(this ICmisObject obj, params string[] deviceIds) {
            var ids = obj.SecondaryObjectTypeIds();
            if (ids.Contains("gds:sync")) {
                var devices = obj.IgnoredDevices();
                if (deviceIds == null || deviceIds.Length == 0) {
                    if (devices.Remove("*") ||
                        devices.Remove(Config.ConfigManager.CurrentConfig.DeviceId.ToString().ToLower())) {
                        if (devices.Count > 0) {
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            properties.Add("gds:ignoreDeviceIds", devices);
                            obj.UpdateProperties(properties, true);
                        } else {
                            obj.RemoveAllSyncIgnores();
                        }
                    }
                } else {
                    bool changed = false;
                    foreach (var removeId in deviceIds) {
                        if (devices.Remove(removeId)) {
                            changed = true;
                        }
                    }

                    if (changed) {
                        if (devices.Count > 0) {
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            properties.Add("gds:ignoreDeviceIds", devices);
                            obj.UpdateProperties(properties, true);
                        } else {
                            obj.RemoveAllSyncIgnores();
                        }
                    }
                }
            }
        }

        public static void RemoveAllSyncIgnores(this ICmisObject obj) {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            var ids = obj.SecondaryObjectTypeIds();
            if (ids.Remove("gds:sync")) {
                properties.Add(PropertyIds.SecondaryObjectTypeIds, ids);
                obj.UpdateProperties(properties, true);
            }
        }

        /// <summary>
        /// Lists all Ignored devices.
        /// </summary>
        /// <returns>The devices.</returns>
        /// <param name="obj">Cmis object.</param>
        public static IList<string> IgnoredDevices(this ICmisObject obj) {
            IList<string> result = new List<string>();
            if (obj.Properties != null) {
                foreach (var ignoredProperty in obj.Properties) {
                    if (ignoredProperty.Id.Equals("gds:ignoreDeviceIds")) {
                        foreach (var device in ignoredProperty.Values) {
                            result.Add((device as string).ToLower());
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Secondary object type identifiers.
        /// </summary>
        /// <returns>The object type identifiers.</returns>
        /// <param name="obj">Cmis object.</param>
        public static IList<string> SecondaryObjectTypeIds(this ICmisObject obj) {
            IList<string> ids = new List<string>();
            if (obj.SecondaryTypes != null) {
                foreach (var secondaryType in obj.SecondaryTypes) {
                    ids.Add(secondaryType.Id);
                }
            }

            return ids;
        }

        /// <summary>
        /// Indicates if all children are ignored.
        /// </summary>
        /// <returns><c>true</c>, if all children are ignored, <c>false</c> otherwise.</returns>
        /// <param name="obj">Remote cmis object.</param>
        public static bool AreAllChildrenIgnored(this ICmisObject obj) {
            IList<string> devices = obj.IgnoredDevices();
            if (devices.Contains("*") || devices.Contains(Config.ConfigManager.CurrentConfig.DeviceId.ToString().ToLower())) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Determines if is server able to update modification date.
        /// </summary>
        /// <returns><c>true</c> if is server able to update modification date; otherwise, <c>false</c>.</returns>
        /// <param name="session">Cmis session.</param>
        public static bool IsServerAbleToUpdateModificationDate(this ISession session) {
            bool result = false;
            var docType = session.Binding.GetRepositoryService().GetTypeDefinition(session.RepositoryInfo.Id, "cmis:document", null);
            foreach (var prop in docType.PropertyDefinitions) {
                if (prop.Id == "cmis:lastModificationDate" && prop.Updatability == DotCMIS.Enums.Updatability.ReadWrite) {
                    result = true;
                    break;
                }
            }

            if (result) {
                var folderType = session.Binding.GetRepositoryService().GetTypeDefinition(session.RepositoryInfo.Id, "cmis:folder", null);
                foreach (var prop in folderType.PropertyDefinitions) {
                    if (prop.Id == "cmis:lastModificationDate" && prop.Updatability != DotCMIS.Enums.Updatability.ReadWrite) {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Hex string to byte array.
        /// </summary>
        /// <returns>The byte array.</returns>
        /// <param name="hex">Hex string without leading 0x.</param>
        private static byte[] StringToByteArray(string hex) {
            if (hex.Length % 2 == 1) {
                throw new ArgumentException("The binary key cannot have an odd number of digits");
            }

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < (hex.Length >> 1); ++i) {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
            }

            return arr;
        }

        private static int GetHexVal(char hex) {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}