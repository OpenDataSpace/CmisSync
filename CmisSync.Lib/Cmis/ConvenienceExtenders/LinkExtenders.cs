//-----------------------------------------------------------------------
// <copyright file="LinkExtenders.cs" company="GRAU DATA AG">
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
    using System.Security.Cryptography;
    using System.Text;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    /// <summary>
    /// Download and Upload Link extenders.
    /// </summary>
    public static class LinkExtenders {
        private const string GdsLinkTypeName = "gds:link";
        private const string GdsLinkTypePropertyName = "gds:linkType";
        private const string GdsLinkMessagePropertyName = "gds:message";
        private const string GdsLinkSubjectPropertyName = "gds:subject";
        private const string GdsLinkMailPropertyName = "gds:emailAddress";
        private const string GdsLinkNotificationPropertyName = "gds:executeNotification";
        private const string GdsLinkPasswordPropertyName = "gds:password";
        private const string GdsLinkUrlPropertyName = "gds:url";

        /// <summary>
        /// Creates a download link.
        /// </summary>
        /// <returns>The download link.</returns>
        /// <param name="session">Cmis session.</param>
        /// <param name="expirationIn">Expiration in.</param>
        /// <param name="password">Link password.</param>
        /// <param name="mailAddresses">Mail addresses.</param>
        /// <param name="subject">Link mail subject.</param>
        /// <param name="message">Link mail message.</param>
        /// <param name="notifyAboutLinkUsage">If set to <c>true</c> notify about link usage.</param>
        /// <param name="objectIds">Downloadable objects.</param>
        public static ICmisObject CreateDownloadLink(
            this ISession session,
            TimeSpan? expirationIn = null,
            string password = null,
            IList<string> mailAddresses = null,
            string subject = null,
            string message = null,
            bool notifyAboutLinkUsage = true,
            params string[] objectIds)
        {
            return session.CreateLink(
                LinkType.DownloadLink,
                expirationIn,
                password,
                mailAddresses,
                subject,
                message,
                notifyAboutLinkUsage,
                objectIds);
        }

        /// <summary>
        /// Creates a download link.
        /// </summary>
        /// <returns>The download link.</returns>
        /// <param name="session">Cmis session.</param>
        /// <param name="expirationIn">Expiration in.</param>
        /// <param name="password">Link password.</param>
        /// <param name="mailAddresses">Mail addresses.</param>
        /// <param name="subject">Link mail subject.</param>
        /// <param name="message">Link mail message.</param>
        /// <param name="notifyAboutLinkUsage">If set to <c>true</c> notify about link usage.</param>
        /// <param name="downloadableObjects">Object identifiers of downloadable objects.</param>
        public static ICmisObject CreateDownloadLink(
            this ISession session,
            TimeSpan? expirationIn = null,
            string password = null,
            IList<string> mailAddresses = null,
            string subject = null,
            string message = null,
            bool notifyAboutLinkUsage = true,
            params IFileableCmisObject[] downloadableObjects)
        {
            var objectIds = new List<string>();
            if (downloadableObjects != null) {
                foreach (var downloadableObject in downloadableObjects) {
                    objectIds.Add(downloadableObject.Id);
                }
            }

            return session.CreateDownloadLink(
                expirationIn,
                password,
                mailAddresses,
                subject,
                message,
                notifyAboutLinkUsage,
                objectIds.ToArray());
        }

        /// <summary>
        /// Creates an upload link.
        /// </summary>
        /// <returns>The upload link.</returns>
        /// <param name="session">Cmis session.</param>
        /// <param name="expirationIn">Expiration in.</param>
        /// <param name="password">Link password.</param>
        /// <param name="mailAddresses">Mail addresses.</param>
        /// <param name="subject">Link mail subject.</param>
        /// <param name="message">Link mail message.</param>
        /// <param name="notifyAboutLinkUsage">If set to <c>true</c> notify about link usage.</param>
        /// <param name="targetFolder">The upload target folder.</param>
        public static ICmisObject CreateUploadLink(
            this ISession session,
            TimeSpan? expirationIn = null,
            string password = null,
            IList<string> mailAddresses = null,
            string subject = null,
            string message = null,
            bool notifyAboutLinkUsage = true,
            IFolder targetFolder = null)
        {
            return session.CreateUploadLink(
                expirationIn,
                password,
                mailAddresses,
                subject,
                message,
                notifyAboutLinkUsage,
                targetFolder.Id);
        }

        /// <summary>
        /// Creates upload link.
        /// </summary>
        /// <returns>The upload link.</returns>
        /// <param name="session">Cmis session.</param>
        /// <param name="expirationIn">Expiration in.</param>
        /// <param name="password">Link password.</param>
        /// <param name="mailAddresses">Mail addresses.</param>
        /// <param name="subject">Link mail subject.</param>
        /// <param name="message">Link mail message.</param>
        /// <param name="notifyAboutLinkUsage">If set to <c>true</c> notify about link usage.</param>
        /// <param name="objectId">Object identifier of the upload target folder.</param>
        public static ICmisObject CreateUploadLink(
            this ISession session,
            TimeSpan? expirationIn = null,
            string password = null,
            IList<string> mailAddresses = null,
            string subject = null,
            string message = null,
            bool notifyAboutLinkUsage = true,
            string objectId = null)
        {
            var objectIds = new List<string>();
            if (objectId != null) {
                objectIds.Add(objectId);
            }

            return session.CreateLink(
                LinkType.UploadLink,
                expirationIn,
                password,
                mailAddresses,
                subject,
                message,
                notifyAboutLinkUsage,
                objectIds.ToArray());
        }

        /// <summary>
        /// Creates an upload or download link and adds relations to the given object id(s).
        /// </summary>
        /// <returns>The link object.</returns>
        /// <param name="session">Cmis session.</param>
        /// <param name="linkType">Gds link type.</param>
        /// <param name="expirationIn">Link expiration in the given time span.</param>
        /// <param name="password">Link password.</param>
        /// <param name="mailAddresses">Mail receiver addresses.</param>
        /// <param name="subject">Link mail subject.</param>
        /// <param name="message">Link mail message.</param>
        /// <param name="notifyAboutLinkUsage">Enables the notification of the link creator of the link is used.</param>
        /// <param name="objectIds">Object identifiers of downloadable objects or the object id of the upload target folder.</param>
        public static ICmisObject CreateLink(
            this ISession session,
            LinkType linkType,
            TimeSpan? expirationIn = null,
            string password = null,
            IList<string> mailAddresses = null,
            string subject = null,
            string message = null,
            bool notifyAboutLinkUsage = true,
            params string[] objectIds)
        {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            IDictionary<string, object> properties = new Dictionary<string, object>();
            List<string> idsSecondary = new List<string>();
            properties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisItem.GetCmisValue());
            idsSecondary.Add(GdsLinkTypeName);
            properties.Add(PropertyIds.SecondaryObjectTypeIds, idsSecondary);
            switch (linkType) {
            case LinkType.DownloadLink:
                break;
            case LinkType.UploadLink:
                if (objectIds.Length > 1) {
                    throw new ArgumentOutOfRangeException("objectIds", string.Format("Upload links are not able to link to multiple ({0}) target ids", objectIds.Length));
                }

                break;
            default:
                throw new ArgumentException(string.Format("Given link type {0} is invalid or unknown", linkType.GetCmisValue()), "linkType");
            }

            properties.Add(GdsLinkTypePropertyName, linkType.GetCmisValue());

            properties.Add(GdsLinkNotificationPropertyName, notifyAboutLinkUsage);
            if (expirationIn != null) {
                properties.Add("cmis:rm_expirationDate", DateTime.UtcNow + (TimeSpan)(expirationIn));
            }

            if (subject != null) {
                properties.Add(GdsLinkSubjectPropertyName, subject);
            }

            if (message != null) {
                properties.Add(GdsLinkMessagePropertyName, message);
            }

            if (mailAddresses != null && mailAddresses.Count > 0) {
                properties.Add(GdsLinkMailPropertyName, new List<string>(mailAddresses));
            }

            if (password != null) {
                using (var hashAlg = SHA256Managed.Create()) {
                    properties.Add(GdsLinkPasswordPropertyName, hashAlg.ComputeHash(Encoding.UTF8.GetBytes(password)).ToHexString());
                }
            }

            var linkItem = session.CreateItem(properties, null);
            foreach (var objectId in objectIds) {
                IDictionary<string, object> relProperties = new Dictionary<string, object>();
                relProperties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisRelationship.GetCmisValue());
                relProperties.Add(PropertyIds.SourceId, linkItem.Id);
                relProperties.Add(PropertyIds.TargetId, objectId);
                session.CreateRelationship(relProperties);
            }

            return session.GetObject(linkItem);
        }

        /// <summary>
        /// Gets the link URL of the given link item.
        /// </summary>
        /// <returns>The URL.</returns>
        /// <param name="linkItem">Link item.</param>
        public static Uri GetUrl(this ICmisObject linkItem) {
            if (linkItem == null) {
                throw new ArgumentNullException("linkItem");
            }

            var url = linkItem.GetPropertyValue(GdsLinkUrlPropertyName) as string;
            return url == null ? null : new Uri(url);
        }

        /// <summary>
        /// Gets the message of the giben link item.
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="linkItem">Link item.</param>
        public static string GetMessage(this ICmisObject linkItem) {
            if (linkItem == null) {
                throw new ArgumentNullException("linkItem");
            }

            return linkItem.GetPropertyValue(GdsLinkMessagePropertyName) as string;
        }

        /// <summary>
        /// Are links supported.
        /// </summary>
        /// <returns><c>true</c>, if links are supported, <c>false</c> otherwise.</returns>
        /// <param name="session">Cmis Session.</param>
        public static bool AreLinksSupported(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            try {
                var type = session.GetTypeDefinition(GdsLinkTypeName);
                if (type == null) {
                    return false;
                }

                if (session.GetTypeDefinition(BaseTypeId.CmisRelationship.GetCmisValue()) == null) {
                    return false;
                }

                foreach (var prop in type.PropertyDefinitions) {
                    if (prop.Id.Equals(GdsLinkTypePropertyName)) {
                        return true;
                    }
                }
            } catch (CmisObjectNotFoundException) {
            }

            return false;
        }
    }
}