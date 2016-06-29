//-----------------------------------------------------------------------
// <copyright file="AllowableActionExtenders.cs" company="GRAU DATA AG">
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
    public static class AllowableActionExtenders {
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

        /// <summary>
        /// Determines if object parents can be requested.
        /// </summary>
        /// <returns><c>true</c> if object parents can be requested; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetObjectParents(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetObjectParents);
        }

        /// <summary>
        /// Determines if folder parent can be requested.
        /// </summary>
        /// <returns><c>true</c> if folder parent can be requested; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetFolderParent(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetFolderParent);
        }

        /// <summary>
        /// Determines if folder tree can be requested.
        /// </summary>
        /// <returns><c>true</c> if folder tree can be requested; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
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

        /// <summary>
        /// Determines if descendants can be requested.
        /// </summary>
        /// <returns><c>true</c> if descendants can be requested; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetDescendants(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetDescendants);
        }

        /// <summary>
        /// Determines if object can be moved.
        /// </summary>
        /// <returns><c>true</c> if object can be moved; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanMoveObject(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanMoveObject);
        }

        /// <summary>
        /// Determines if content stream can be deleted.
        /// </summary>
        /// <returns><c>true</c> if content stream can be deleted; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanDeleteContentStream(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanDeleteContentStream);
        }

        /// <summary>
        /// Determines if can check out the specified obj.
        /// </summary>
        /// <returns><c>true</c> if can check out the specified obj; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanCheckOut(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCheckOut);
        }

        /// <summary>
        /// Determines if can cancel check out the specified obj.
        /// </summary>
        /// <returns><c>true</c> if can cancel check out the specified obj; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanCancelCheckOut(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCancelCheckOut);
        }

        /// <summary>
        /// Determines if can check in the specified obj.
        /// </summary>
        /// <returns><c>true</c> if can check in the specified obj; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanCheckIn(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCheckIn);
        }

        /// <summary>
        /// Determines if content stream can be set.
        /// </summary>
        /// <returns><c>true</c> if content stream can be set; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanSetContentStream(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanSetContentStream);
        }

        /// <summary>
        /// Determines if all versions can be requested.
        /// </summary>
        /// <returns><c>true</c> if all versions of the specified obj can be requested; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetAllVersions(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetAllVersions);
        }

        /// <summary>
        /// Determines if adding object to folder is possible.
        /// </summary>
        /// <returns><c>true</c> if adding an object to folder is possible; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanAddObjectToFolder(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanAddObjectToFolder);
        }

        /// <summary>
        /// Determines if can remove object from folder.
        /// </summary>
        /// <returns><c>true</c> if can remove object from folder; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanRemoveObjectFromFolder(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanRemoveObjectFromFolder);
        }

        /// <summary>
        /// Determines if can get content stream.
        /// </summary>
        /// <returns><c>true</c> if can get content stream; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetContentStream(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetContentStream);
        }

        /// <summary>
        /// Determines if can apply policy to the specified obj.
        /// </summary>
        /// <returns><c>true</c> if can apply policy to the specified obj; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanApplyPolicy(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanApplyPolicy);
        }

        /// <summary>
        /// Determines if can get applied policies of the specified obj.
        /// </summary>
        /// <returns><c>true</c> if can get applied policies of the specified obj; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetAppliedPolicies(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetAppliedPolicies);
        }

        /// <summary>
        /// Determines if a policy can be removed from obj.
        /// </summary>
        /// <returns><c>true</c> if a policy can be removed from the specified obj; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanRemovePolicy(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanRemovePolicy);
        }

        /// <summary>
        /// Determines if children can be requested.
        /// </summary>
        /// <returns><c>true</c> if children can be requested; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetChildren(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetChildren);
        }

        /// <summary>
        /// Determines if create document is possible.
        /// </summary>
        /// <returns><c>true</c> if can create document; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanCreateDocument(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCreateDocument);
        }

        /// <summary>
        /// Determines if create folder is possible.
        /// </summary>
        /// <returns><c>true</c> if can create folder; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanCreateFolder(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCreateFolder);
        }

        /// <summary>
        /// Determines if can create relationship.
        /// </summary>
        /// <returns><c>true</c> if can create relationship; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanCreateRelationship(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanCreateRelationship);
        }

        /// <summary>
        /// Determines if can delete tree.
        /// </summary>
        /// <returns><c>true</c> if can delete tree; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanDeleteTree(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanDeleteTree);
        }

        /// <summary>
        /// Determines if can get renditions.
        /// </summary>
        /// <returns><c>true</c> if can get renditions; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetRenditions(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetRenditions);
        }

        /// <summary>
        /// Determines if can get acl.
        /// </summary>
        /// <returns><c>true</c> if can get acl; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanGetAcl(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanGetAcl);
        }

        /// <summary>
        /// Determines if acl can be applied.
        /// </summary>
        /// <returns><c>true</c> if can apply acl; otherwise, <c>false</c> or <c>null</c> if no information about actions is available..</returns>
        /// <param name="obj">Cmis object.</param>
        public static bool? CanApplyAcl(this ICmisObject obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return obj.IsActionAllowed(Actions.CanApplyAcl);
        }

        /// <summary>
        /// Determines if obj is read only.
        /// </summary>
        /// <returns><c>true</c> if obj is read only; otherwise, <c>false</c>.</returns>
        /// <param name="obj">Cmis object.</param>
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
        /// Determines if the folder can be renamed and moved and deleted. This is forbidden for shared folder and project rooms.
        /// </summary>
        /// <returns><c>true</c> if the folder can be renamed and moved and deleted; otherwise, <c>false</c>.</returns>
        /// <param name="folder">CMIS folder.</param>
        public static bool CanRenameAndMoveAndDelete(this IFolder folder) {
            if (folder == null) {
                throw new ArgumentNullException("folder");
            }

            return !(folder.CanCreateFolder() == true &&
                folder.CanCreateDocument() == true &&
                folder.CanGetChildren() == true &&
                folder.CanGetProperties() == true &&
                folder.CanMoveObject() == false);
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