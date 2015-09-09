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

    public static class LinkExtenders {
        private static ICmisObject CreateDownloadLink(this ISession session,
            TimeSpan? expirationIn = null,
            string password = null,
            string mailAddress = null,
            string subject = null,
            string message = null,
            params string[] objectIds)
        {
            IDictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisItem.GetCmisValue());
            List<string> idsSecondary = new List<string>();
            idsSecondary.Add("gds:link");
            idsSecondary.Add("cmis:rm_clientMgtRetention");
            properties.Add(PropertyIds.SecondaryObjectTypeIds, idsSecondary);
            properties.Add("gds:linkType", "gds:downloadLink");
            if (expirationIn != null) {
                properties.Add("cmis:rm_expirationDate", DateTime.UtcNow + (TimeSpan)(expirationIn));
            }

            if (subject != null) {
                properties.Add("gds:subject", subject);
            }

            if (message != null) {
                properties.Add("gds:message", message);
            }

            if (mailAddress != null) {
                properties.Add("gds:emailAddress", mailAddress);
            }

            if (password != null) {
                using (var hashAlg = SHA256Managed.Create()) {
                    properties.Add("gds:password", hashAlg.ComputeHash(Encoding.UTF8.GetBytes(password)).ToHexString());
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

        public static Uri GetUrl(this ICmisObject linkItem) {
            if (linkItem == null) {
                throw new ArgumentNullException("linkItem");
            }

            var url = linkItem.GetPropertyValue("gds:url") as string;
            return url == null ? null : new Uri(url);
        }

        public static string GetMessage(this ICmisObject linkItem) {
            if (linkItem == null) {
                throw new ArgumentNullException("linkItem");
            }

            return linkItem.GetPropertyValue("gds:message") as string;
        }
    }
}