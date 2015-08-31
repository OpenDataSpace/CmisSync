//-----------------------------------------------------------------------
// <copyright file="AllowableActionConvenienceExtenders.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis.ConvenienceExtenders {
    using System;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Enums;

    /// <summary>
    /// Allowable action convenience extenders of cmis objects
    /// </summary>
    public static class AllowableActionConvenienceExtenders {
        /// <summary>
        /// Ares the allowable actions available with this session and its default operation context.
        /// </summary>
        /// <returns><c>true</c>, if allowable actions are available, <c>false</c> otherwise.</returns>
        /// <param name="session">Cmis session with its default context.</param>
        public static bool AreAllowableActionsAvailable(this ISession session) {
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            var repositoryInfo = session.RepositoryInfo;
#region Workaround
            // Workaround to detect minimum version of correct responding cmis gw (https://mantis.dataspace.cc/view.php?id=4463)
            if (repositoryInfo.ProductName == "GRAU DataSpace CMIS Gateway") {
                try {
                    var version = new Version(repositoryInfo.ProductVersion);
                    if (version < new Version(1, 5, 1120)) {
                        return false;
                    }
                } catch (Exception) {
                }
            }
#endregion
            var defaultContext = session.DefaultContext;
            if (defaultContext.IncludeAllowableActions) {
                return true;
            }

            return defaultContext.IncludeAcls && repositoryInfo.Capabilities.AclCapability != CapabilityAcl.None;
        }

        /// <summary>
        /// Determines if object can be deleted.
        /// </summary>
        /// <returns><c>true</c> if object can be deleted; otherwise, <c>false</c> or <c>null</c> if no actions are available</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanDeleteObject(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanDeleteObject);
        }

        /// <summary>
        /// Determines if object properties can be updated.
        /// </summary>
        /// <returns><c>true</c> if object properties can be updated; otherwise, <c>false</c> or <c>null</c> if no actions are available.</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanUpdateProperties(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanUpdateProperties);
        }

        /// <summary>
        /// Determines if object properties can be get.
        /// </summary>
        /// <returns><c>true</c> if properties can be get; otherwise, <c>false</c> or <c>null</c> if no actions are available.</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetProperties(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetProperties);
        }

        /// <summary>
        /// Determines if object relationships can be get.
        /// </summary>
        /// <returns><c>true</c> if object relationships can be get; otherwise, <c>false</c> or <c>null</c> if no actions are available.</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetObjectRelationships(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetObjectRelationships);
        }

        public static bool? CanGetObjectParents(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetObjectParents);
        }

        public static bool? CanGetFolderParent(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetFolderParent);
        }

        public static bool? CanGetFolderTree(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            if (obj.IsActionAllowed(Actions.CanGetDescendants) == true) {
                return true;
            } else {
                return obj.IsActionAllowed(Actions.CanGetFolderTree);
            }
        }

        public static bool? CanGetDescendants(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetDescendants);
        }

        public static bool? CanMoveObject(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanMoveObject);
        }

        public static bool? CanDeleteContentStream(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanDeleteContentStream);
        }

        public static bool? CanCheckOut(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCheckOut);
        }

        public static bool? CanCancelCheckOut(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCancelCheckOut);
        }

        public static bool? CanCheckIn(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCheckIn);
        }

        public static bool? CanSetContentStream(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanSetContentStream);
        }

        public static bool? CanGetAllVersions(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetAllVersions);
        }

        public static bool? CanAddObjectToFolder(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanAddObjectToFolder);
        }

        public static bool? CanRemoveObjectFromFolder(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanRemoveObjectFromFolder);
        }

        public static bool? CanGetContentStream(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetContentStream);
        }

        public static bool? CanApplyPolicy(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanApplyPolicy);
        }

        public static bool? CanGetAppliedPolicies(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetAppliedPolicies);
        }

        public static bool? CanRemovePolicy(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanRemovePolicy);
        }

        public static bool? CanGetChildren(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetChildren);
        }

        public static bool? CanCreateDocument(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCreateDocument);
        }

        public static bool? CanCreateFolder(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCreateFolder);
        }

        public static bool? CanCreateRelationship(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCreateRelationship);
        }

        public static bool? CanDeleteTree(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanDeleteTree);
        }

        public static bool? CanGetRenditions(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetRenditions);
        }

        public static bool? CanGetAcl(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetAcl);
        }

        public static bool? CanApplyAcl(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanApplyAcl);
        }

        public static bool IsReadOnly(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            if (obj is IDocument) {
                return obj.CanSetContentStream() == false && obj.CanDeleteObject() == false;
            } else {
                return obj.CanCreateFolder() == false && obj.CanCreateDocument() == false;
            }
        }

        /// <summary>
        /// Determines if the specified action is allowed on the cmis object.
        /// </summary>
        /// <returns><c>true</c> if action is allowed on the cmis object; otherwise, <c>false</c> or <c>null</c> if no information about actions is available.</returns>
        /// <param name="obj">Cmis Object.</param>
        /// <param name="action">Action name.</param>
        public static bool? IsActionAllowed(this ICmisObject obj, string action) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            try {
                return obj.AllowableActions.Actions.Contains(action);
            } catch (NullReferenceException) {
                return null;
            }
        }
    }
}