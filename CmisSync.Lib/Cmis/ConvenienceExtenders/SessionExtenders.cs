//-----------------------------------------------------------------------
// <copyright file="SessionExtenders.cs" company="GRAU DATA AG">
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

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    /// <summary>
    /// DotCMIS Session extenders.
    /// </summary>
    public static class SessionExtenders {
        /// <summary>
        /// Returns if the server is able to return content hashes.
        /// </summary>
        /// <returns><c>true</c> if the server type system supports content hashes, otherwise <c>false</c>.</returns>
        /// <param name="session">Cmis session.</param>
        public static bool IsContentStreamHashSupported(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            try {
                var type = session.GetTypeDefinition(BaseTypeId.CmisDocument.GetCmisValue());
                if (type == null) {
                    return false;
                }

                foreach (var prop in type.PropertyDefinitions) {
                    if (prop.Id.Equals("cmis:contentStreamHash")) {
                        return true;
                    }
                }
            } catch (CmisObjectNotFoundException) {
            }

            return false;
        }

        /// <summary>
        /// Determines if is server able to update modification date.
        /// </summary>
        /// <returns><c>true</c> if is server able to update modification date; otherwise, <c>false</c>.</returns>
        /// <param name="session">Cmis session.</param>
        public static bool IsServerAbleToUpdateModificationDate(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            bool result = false;
            var docType = session.GetTypeDefinition(BaseTypeId.CmisDocument.GetCmisValue());
            foreach (var prop in docType.PropertyDefinitions) {
                if (prop.Id == PropertyIds.LastModificationDate && prop.Updatability == DotCMIS.Enums.Updatability.ReadWrite) {
                    result = true;
                    break;
                }
            }

            if (result) {
                var folderType = session.GetTypeDefinition(BaseTypeId.CmisFolder.GetCmisValue());
                foreach (var prop in folderType.PropertyDefinitions) {
                    if (prop.Id == PropertyIds.LastModificationDate && prop.Updatability != DotCMIS.Enums.Updatability.ReadWrite) {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Detect whether the repository has the ChangeLog capability.
        /// </summary>
        /// <param name="session">The Cmis Session</param>
        /// <returns>
        /// <c>true</c> if this feature is available, otherwise <c>false</c>
        /// </returns>
        public static bool AreChangeEventsSupported(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            try {
                return session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.All ||
                    session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.ObjectIdsOnly;
            } catch (NullReferenceException) {
                return false;
            }
        }

        /// <summary>
        /// Detect whether the repository supports checkout/cancelCheckout/checkin
        /// </summary>
        /// <param name="session">The Cmis Session</param>
        /// <returns>
        /// <c>true</c> if this feature is available, otherwise <c>false</c>
        /// </returns>
        public static bool IsPrivateWorkingCopySupported(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            try {
                return session.RepositoryInfo.Capabilities.IsPwcUpdatableSupported.GetValueOrDefault();
            } catch (NullReferenceException) {
                return false;
            }
        }

        /// <summary>
        /// Determines if multi filing is supported with the specified session.
        /// </summary>
        /// <returns><c>true</c> if multi filing supported with the specified session; otherwise, <c>false</c>.</returns>
        /// <param name="session">Cmis session.</param>
        public static bool IsMultiFilingSupported(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            try {
                return session.RepositoryInfo.Capabilities.IsMultifilingSupported == true;
            } catch (NullReferenceException) {
                return false;
            }
        }

        /// <summary>
        /// Determines if unfiling is supported with the specified session.
        /// </summary>
        /// <returns><c>true</c> if is unfiling is supported with the specified session; otherwise, <c>false</c>.</returns>
        /// <param name="session">Cmis session.</param>
        public static bool IsUnFilingSupported(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            try {
                return session.RepositoryInfo.Capabilities.IsUnfilingSupported == true;
            } catch (NullReferenceException) {
                return false;
            }
        }

        /// <summary>
        /// Determines if getDescendants calls are supported the specified session.
        /// </summary>
        /// <returns><c>true</c> if getDescendants calls supported the specified session; otherwise, <c>false</c>.</returns>
        /// <param name="session">Cmis session.</param>
        public static bool IsGetDescendantsSupported(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            try {
                return session.RepositoryInfo.Capabilities.IsGetDescendantsSupported == true;
            } catch (NullReferenceException) {
                return false;
            }
        }

        /// <summary>
        /// Ares download links supported.
        /// </summary>
        /// <returns><c>true</c>, if download links are supported, <c>false</c> otherwise.</returns>
        /// <param name="session">Cmis Session.</param>
        public static bool AreDownloadLinksSupported(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            try {
                var type = session.GetTypeDefinition("gds:link");
                if (type == null) {
                    return false;
                }

                if (session.GetTypeDefinition(BaseTypeId.CmisRelationship.GetCmisValue()) == null) {
                    return false;
                }

                foreach (var prop in type.PropertyDefinitions) {
                    if (prop.Id.Equals("gds:linkType")) {
                        return true;
                    }
                }

            } catch (CmisObjectNotFoundException) {
            }

            return false;
        }
    }
}