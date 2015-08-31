//-----------------------------------------------------------------------
// <copyright file="CmisUtils.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis.UiUtils {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;

    using CmisSync.Lib.Config;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Data;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Useful CMIS methods.
    /// </summary>
    public static class CmisUtils {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CmisUtils));

        static public Dictionary<string, string> GetCmisParameters(ServerCredentials credentials) {
            if (credentials == null) {
                throw new ArgumentNullException("credentials");
            }

            var cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = credentials.Binding;
            if (credentials.Binding == BindingType.AtomPub) {
                cmisParameters[SessionParameter.AtomPubUrl] = credentials.Address.ToString();
            } else if (credentials.Binding == BindingType.Browser) {
                cmisParameters[SessionParameter.BrowserUrl] = credentials.Address.ToString();
            }

            cmisParameters[SessionParameter.User] = credentials.UserName;
            cmisParameters[SessionParameter.Password] = credentials.Password.ToString();
            cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();
            return cmisParameters;
        }

        static public Dictionary<string, string> GetCmisParameters(CmisRepoCredentials credentials) {
            if (credentials == null) {
                throw new ArgumentNullException("credentials");
            }

            var cmisParameters = GetCmisParameters((ServerCredentials)credentials);
            cmisParameters[SessionParameter.RepositoryId] = credentials.RepoId;
            return cmisParameters;
        }

        /// <summary>
        /// Get the sub-folders of a particular CMIS folder.
        /// </summary>
        /// <returns>Full path of each sub-folder, including leading slash.</returns>
        static public string[] GetSubfolders(CmisRepoCredentials credentials, string path) {
            var result = new List<string>();

            // Connect to the CMIS repository.
            var cmisParameters = GetCmisParameters(credentials);
            var factory = SessionFactory.NewInstance();
            var session = factory.CreateSession(cmisParameters);

            // Get the folder.
            IFolder folder;
            try {
                folder = (IFolder)session.GetObjectByPath(path);
            } catch (Exception ex) {
                Logger.Warn(string.Format("CmisUtils | exception when session GetObjectByPath for {0}: {1}", path, Utils.ToLogString(ex)));
                return result.ToArray();
            }

            // Debug the properties count, which allows to check whether a particular CMIS implementation is compliant or not.
            // For instance, IBM Connections is known to send an illegal count.
            Logger.Info("CmisUtils | folder.Properties.Count:" + folder.Properties.Count.ToString());

            // Get the folder's sub-folders.
            var children = folder.GetChildren();

            // Return the full path of each of the sub-folders.
            foreach (var subfolder in children.OfType<IFolder>()) {
                result.Add(subfolder.Path);
            }

            return result.ToArray();
        }

        public class NodeTree {
            public List<NodeTree> Children = new List<NodeTree>();
            public string Path { get; set; }
            public string Name { get; set; }
            public bool Finished { get; set; }

            public NodeTree(IList<ITree<IFileableCmisObject>> trees, IFolder folder, int depth) {
                this.Path = folder.Path;
                this.Name = folder.Name;
                this.Finished = !(depth == 0);

                if (trees != null) {
                    foreach (ITree<IFileableCmisObject> tree in trees) {
                        Folder f = tree.Item as Folder;
                        if (f != null) {
                            this.Children.Add(new NodeTree(tree.Children, f, depth - 1));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the sub-folders of a particular CMIS folder.
        /// </summary>
        /// <returns>Full path of each sub-folder, including leading slash.</returns>
        static public NodeTree GetSubfolderTree(CmisRepoCredentials credentials, string path, int depth) {
            // Connect to the CMIS repository.
            Dictionary<string, string> cmisParameters = GetCmisParameters(credentials);
            SessionFactory factory = SessionFactory.NewInstance();
            ISession session = factory.CreateSession(cmisParameters);

            // Get the folder.
            IFolder folder;
            try {
                folder = (IFolder)session.GetObjectByPath(path);
            } catch (Exception ex) {
                Logger.Warn(string.Format("CmisUtils | exception when session GetObjectByPath for {0}: {1}", path, Utils.ToLogString(ex)));
                throw;
            }

            // Debug the properties count, which allows to check whether a particular CMIS implementation is compliant or not.
            // For instance, IBM Connections is known to send an illegal count.
            Logger.Info("CmisUtils | folder.Properties.Count:" + folder.Properties.Count.ToString());
            try {
                IList<ITree<IFileableCmisObject>> trees = folder.GetFolderTree(depth);
                return new NodeTree(trees, folder, depth);
            } catch (Exception e) {
                Logger.Info("CmisUtils getSubFolderTree | Exception " + e.Message, e);
                throw;
            }
        }
    }
}