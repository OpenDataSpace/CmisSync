//-----------------------------------------------------------------------
// <copyright file="CmisRepositoryInfoConvenienceExtenders.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis {
    using System;

    using DotCMIS.Data;

    /// <summary>
    /// Cmis repository info convenience extenders.
    /// </summary>
    public static class CmisRepositoryInfoConvenienceExtenders {
        /// <summary>
        /// Creates a string containing all needed properties of a repository info from remote. If passed repoInfo is null, an empty string is returned.
        /// </summary>
        /// <returns>The log string.</returns>
        /// <param name="repoInfo">Remote repository information.</param>
        public static string ToLogString(this IRepositoryInfo repoInfo) {
            if (repoInfo == null) {
                return string.Empty;
            }

            return string.Format(
                "Name: \"{1}\" Id: \"{2}\"{0}" +
                "Description: \"{3}\"{0}" +
                "ProductName: \"{4}\"{0}" +
                "ProductVersion: \"{5}\"{0}" +
                "Vendor: \"{6}\"{0}" +
                "Supported Cmis Versions: \"{7}\"{0}",
                Environment.NewLine,
                repoInfo.Name,
                repoInfo.Id,
                repoInfo.Description,
                repoInfo.ProductName,
                repoInfo.ProductVersion,
                repoInfo.VendorName,
                repoInfo.CmisVersionSupported);
        }
    }
}