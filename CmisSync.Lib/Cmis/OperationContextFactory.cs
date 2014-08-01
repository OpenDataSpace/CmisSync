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
            HashSet<string> filters = new HashSet<string>();
            filters.Add("cmis:objectId");
            filters.Add("cmis:name");
            filters.Add("cmis:contentStreamFileName");
            filters.Add("cmis:contentStreamLength");
            filters.Add("cmis:lastModificationDate");
            filters.Add("cmis:path");
            filters.Add("cmis:changeToken");
            filters.Add("cmis:parentId");
            filters.Add("cmis:contentStreamHash");
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            return session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, renditions, true, null, true, 100);
        }

        /// <summary>
        /// Creates the crawl context.
        /// </summary>
        /// <returns>The crawl context.</returns>
        /// <param name="session">Cmis session.</param>
        public static IOperationContext CreateCrawlContext(ISession session) {
            HashSet<string> filters = new HashSet<string>();
            filters.Add("cmis:objectId");
            filters.Add("cmis:name");
            filters.Add("cmis:contentStreamFileName");
            filters.Add("cmis:contentStreamLength");
            filters.Add("cmis:lastModificationDate");
            filters.Add("cmis:changeToken");
            filters.Add("cmis:parentId");
            filters.Add("cmis:contentStreamHash");
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            return session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, renditions, true, null, true, 100);
        }

        /// <summary>
        /// Creates the default context.
        /// </summary>
        /// <returns>The default context.</returns>
        /// <param name="session">Cmis session.</param>
        public static IOperationContext CreateDefaultContext(ISession session) {
            HashSet<string> filters = new HashSet<string>();
            filters.Add("cmis:objectId");
            filters.Add("cmis:name");
            filters.Add("cmis:contentStreamFileName");
            filters.Add("cmis:contentStreamLength");
            filters.Add("cmis:lastModificationDate");
            filters.Add("cmis:path");
            filters.Add("cmis:changeToken");
            filters.Add("cmis:parentId");
            filters.Add("cmis:contentStreamHash");
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            return session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, renditions, true, null, true, 100);
        }

        /// <summary>
        /// Creates the non caching and path including context.
        /// </summary>
        /// <returns>The non caching and path including context.</returns>
        /// <param name="session">Cmis session.</param>
        public static IOperationContext CreateNonCachingPathIncludingContext(ISession session) {
            return CreateContext(
                session,
                false,
                true,
                "cmis:objectId",
                "cmis:name",
                "cmis:contentStreamFileName",
                "cmis:contentStreamLength",
                "cmis:lastModificationDate",
                "cmis:path",
                "cmis:changeToken",
                "cmis:parentId",
                "cmis:contentStreamHash");
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
            HashSet<string> filter = new HashSet<string>();
            foreach (var entry in elements) {
                filter.Add(entry);
            }

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
    }
}