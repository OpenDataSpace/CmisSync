//-----------------------------------------------------------------------
// <copyright file="CmisSelectiveIgnoreCapability.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.SelectiveIgnore
{
    using System;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    public static class CmisSelectiveIgnoreCapability
    {
        /// <summary>
        /// Determines if is server supports selective ignore feature.
        /// </summary>
        /// <returns><c>true</c> if is server able to selectivly ignore folders; otherwise, <c>false</c>.</returns>
        /// <param name="session">Cmis session.</param>
        public static bool SupportsSelectiveIgnore(this ISession session) {
            try {
                var type = session.GetTypeDefinition("gds:sync");
                if (type == null) {
                    return false;
                }

                foreach (var prop in type.PropertyDefinitions) {
                    if (prop.Id.Equals("gds:ignoreDeviceIds")) {
                        return true;
                    }
                }
            } catch (CmisObjectNotFoundException) {
            }

            return false;
        }
    }
}