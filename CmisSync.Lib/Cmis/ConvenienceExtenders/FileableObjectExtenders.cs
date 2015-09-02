
namespace CmisSync.Lib.Cmis.ConvenienceExtenders {
    using System;
    using System.Collections.Generic;
    using System.Text;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    /// <summary>
    /// DotCMIS Fileable object extenders.
    /// </summary>
    public static class FileableObjectExtenders {
        /// <summary>
        /// Converts cmis fileable object to logable string.
        /// </summary>
        /// <returns>The log string.</returns>
        /// <param name="obj">Cmis fileable object.</param>
        public static string ToLogString(this IFileableCmisObject obj) {
            if (obj == null) {
                return "null";
            }

            var sb = new StringBuilder(obj.ToString());
            sb.AppendLine(string.Format("ID:           {0}", obj.Id));
            sb.AppendLine(string.Format("Name:         {0}", obj.Name));
            sb.AppendLine(string.Format("ChangeToken:  {0}", obj.ChangeToken));
            var date = obj.LastModificationDate;
            if (date != null) {
                sb.AppendLine(string.Format("LastModified: {0} Ticks", date.Value.Ticks));
            } else {
                sb.AppendLine(string.Format("LastModified: {0}", date));
            }

            sb.AppendLine(string.Format("ObjectType:   {0}", obj.ObjectType));
            sb.AppendLine(string.Format("BaseType:     {0}", obj.BaseType.DisplayName));
            var folder = obj as IFolder;
            var doc = obj as IDocument;
            if (folder != null) {
                sb.AppendLine(string.Format("Path:         {0}", folder.Path));
                sb.AppendLine(string.Format("ParentId:     {0}", folder.ParentId));
            } else if (doc != null) {
                sb.AppendLine(string.Format("StreamLength: {0}", doc.ContentStreamLength));
                sb.AppendLine(string.Format("MimeType:     {0}", doc.ContentStreamMimeType));
                sb.AppendLine(string.Format("StreamName:   {0}", doc.ContentStreamFileName));
            }

            foreach (var prop in obj.Properties) {
                sb.AppendLine(string.Format("Property {0}: {1}", prop.DisplayName, prop.ValuesAsString));
            }

            return sb.ToString();
        }

        /// <summary>
        /// If the allowable actions to not deny the change of last write time UTC, it will be updated based on the given file info.
        /// </summary>
        /// <returns>The updated remote object or the original untouched one if the change wasn't allowed.</returns>
        /// <param name="obj">Cmis object.</param>
        /// <param name="basedOn">File system info object which will be used to extract the last write time in UTC.</param>
        public static IObjectId IfAllowedUpdateLastWriteTimeUtc(
            this IFileableCmisObject obj,
            CmisSync.Lib.Storage.FileSystem.IFileSystemInfo basedOn)
        {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            if (basedOn == null) {
                throw new ArgumentNullException("basedOn");
            }

            if (obj.CanUpdateProperties() != false) {
                return obj.UpdateLastWriteTimeUtc(basedOn);
            } else {
                return obj;
            }
        }

        /// <summary>
        /// Updates the last write time in UTC via UpdateProperties
        /// </summary>
        /// <returns>The result of UpdateProperties.</returns>
        /// <param name="obj">Fileable cmis object.</param>
        /// <param name="basedOn">File system info object with its modification date.</param>
        public static IObjectId UpdateLastWriteTimeUtc(
            this IFileableCmisObject obj,
            CmisSync.Lib.Storage.FileSystem.IFileSystemInfo basedOn)
        {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            if (basedOn == null) {
                throw new ArgumentNullException("basedOn");
            }

            return obj.UpdateLastWriteTimeUtc(basedOn.LastWriteTimeUtc);
        }

        /// <summary>
        /// Updates the last write time in UTC via UpdateProperties
        /// </summary>
        /// <returns>The result of UpdateProperties.</returns>
        /// <param name="obj">Fileable cmis object.</param>
        /// <param name="modificationDate">Modification date.</param>
        public static IObjectId UpdateLastWriteTimeUtc(this IFileableCmisObject obj, DateTime modificationDate) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            var properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.LastModificationDate, modificationDate);
            try {
                return obj.UpdateProperties(properties, true);
            } catch (CmisConstraintException e) {
                var oldObject = obj.ToLogString();
                obj.Refresh();
                throw new CmisConstraintException(string.Format("Old object: {0}{1}New object: {2}", oldObject, Environment.NewLine, obj.ToLogString()), e);
            }
        }
    }
}