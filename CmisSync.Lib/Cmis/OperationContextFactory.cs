//-----------------------------------------------------------------------
// <copyright file="OperationContextFactory.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis
{
    using System;
    using System.Collections.Generic;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Enums;

    /// <summary>
    /// Operation context factory.
    /// </summary>
    public static class OperationContextFactory
    {
        private static readonly int MaximumItemsPerPage = 1000;

        /// <summary>
        /// Creates the content change event context.
        /// </summary>
        /// <returns>The content change event context.</returns>
        /// <param name="session">Cmis session.</param>
        public static IOperationContext CreateContentChangeEventContext(ISession session) {
            return CreateContext(session: session, cacheEnabled: true, includePathSegments: true, elements: "cmis:path");
        }

        /// <summary>
        /// Creates the crawl context.
        /// </summary>
        /// <returns>The crawl context.</returns>
        /// <param name="session">Cmis session.</param>
        public static IOperationContext CreateCrawlContext(ISession session) {
            return CreateContext(session: session, cacheEnabled: true, includePathSegments: true, elements: null);
        }

        /// <summary>
        /// Creates the default context.
        /// </summary>
        /// <returns>The default context.</returns>
        /// <param name="session">Cmis session.</param>
        public static IOperationContext CreateDefaultContext(ISession session) {
            return CreateContext(session: session, cacheEnabled: true, includePathSegments: true, elements: "cmis:path");
        }

        /// <summary>
        /// Creates the non caching and path including context.
        /// </summary>
        /// <returns>The non caching and path including context.</returns>
        /// <param name="session">Cmis session.</param>
        public static IOperationContext CreateNonCachingPathIncludingContext(ISession session) {
            return CreateContext(session: session, cacheEnabled: false, includePathSegments: true, elements: "cmis:path");
        }

        /// <summary>
        /// Creates an operation context.
        /// </summary>
        /// <returns>The context.</returns>
        /// <param name="session">Cmis session.</param>
        /// <param name="cacheEnabled">If set to <c>true</c> cache enabled.</param>
        /// <param name="includePathSegments">If set to <c>true</c> include path segments.</param>
        /// <param name="elements">Requested cmis elements.</param>
        public static IOperationContext CreateContext(ISession session, bool cacheEnabled, bool includePathSegments, params string[] elements) {
            HashSet<string> filter = CreateFilter(elements);
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            return session.CreateOperationContext(
                filter: filter,
                includeAcls: false,
                includeAllowableActions: true,
                includePolicies: false,
                includeRelationships: IncludeRelationshipsFlag.None,
                renditionFilter: renditions,
                includePathSegments: includePathSegments,
                orderBy: null,
                cacheEnabled: cacheEnabled,
                maxItemsPerPage: MaximumItemsPerPage);
        }

        private static HashSet<string> CreateFilter(params string[] additionalElements) {
            HashSet<string> filter = new HashSet<string>();
            filter.Add(PropertyIds.ObjectId);
            filter.Add(PropertyIds.Name);
            filter.Add(PropertyIds.ContentStreamFileName);
            filter.Add(PropertyIds.ContentStreamLength);
            filter.Add(PropertyIds.LastModificationDate);
            filter.Add(PropertyIds.ChangeToken);
            filter.Add(PropertyIds.ParentId);
            filter.Add("cmis:contentStreamHash");
            filter.Add(PropertyIds.SecondaryObjectTypeIds);
            filter.Add("gds:sync.gds:ignoreDeviceIds");
            filter.Add(PropertyIds.IsVersionSeriesCheckedOut);
            filter.Add(PropertyIds.VersionSeriesCheckedOutId);
            if (additionalElements != null) {
                foreach (var entry in additionalElements) {
                    filter.Add(entry);
                }
            }

            return filter;
        }
    }
}