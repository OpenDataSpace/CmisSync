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
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            return session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, renditions, true, null, true, 100);
        }

        public static IOperationContext CreateCrawlContext(ISession session) {
            HashSet<string> filters = new HashSet<string>();
            filters.Add("cmis:objectId");
            filters.Add("cmis:name");
            filters.Add("cmis:contentStreamFileName");
            filters.Add("cmis:contentStreamLength");
            filters.Add("cmis:lastModificationDate");
            filters.Add("cmis:changeToken");
            filters.Add("cmis:parentId");
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            return session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, renditions, true, null, true, 100);
        }

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
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            return session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, renditions, true, null, true, 100);
        }

        public static IOperationContext CreateNonCachingPathIncludingContext(ISession session) {
            HashSet<string> filters = new HashSet<string>();
            filters.Add("cmis:objectId");
            filters.Add("cmis:name");
            filters.Add("cmis:contentStreamFileName");
            filters.Add("cmis:contentStreamLength");
            filters.Add("cmis:lastModificationDate");
            filters.Add("cmis:path");
            filters.Add("cmis:changeToken");
            filters.Add("cmis:parentId");
            HashSet<string> renditions = new HashSet<string>();
            renditions.Add("cmis:none");
            return session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, renditions, true, null, false, 100);
        }
    }
}