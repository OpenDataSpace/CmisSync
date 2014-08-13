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

namespace CmisSync.Lib.Cmis.UiUtils
{
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
    /// Data object representing a CMIS server.
    /// </summary>
    public class CmisServer
    {
        /// <summary>
        /// URL of the CMIS server.
        /// </summary>
        public Uri Url { get; private set; }

        /// <summary>
        /// Repositories contained in the CMIS server.
        /// </summary>
        public Dictionary<string, string> Repositories { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisServer(Uri url, Dictionary<string, string> repositories)
        {
            this.Url = url;
            this.Repositories = repositories;
        }
    }

    /// <summary>
    /// Useful CMIS methods.
    /// </summary>
    public static class CmisUtils
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CmisUtils));

        /// <summary>
        /// Try to find the CMIS server associated to any URL.
        /// Users can provide the URL of the web interface, and we have to return the CMIS URL
        /// Returns the list of repositories as well.
        /// </summary>
        static public Tuple<CmisServer, Exception> GetRepositoriesFuzzy(ServerCredentials credentials)
        {
            Dictionary<string, string> repositories = null;
            Exception firstException = null;

            // Try the given URL, maybe user directly entered the CMIS AtomPub endpoint URL.
            try
            {
                repositories = GetRepositories(credentials);
            }
            catch (DotCMIS.Exceptions.CmisRuntimeException e)
            {
                if (e.Message == "ConnectFailure") {
                    return new Tuple<CmisServer, Exception>(new CmisServer(credentials.Address, null), new CmisServerNotFoundException(e.Message, e));
                }

                firstException = e;
            }
            catch (Exception e)
            {
                // Save first Exception and try other possibilities.
                firstException = e;
            }

            if (repositories != null)
            {
                // Found!
                return new Tuple<CmisServer, Exception>(new CmisServer(credentials.Address, repositories), null);
            }

            // Extract protocol and server name or IP address
            string prefix = credentials.Address.GetLeftPart(UriPartial.Authority);

            // See https://github.com/nicolas-raoul/CmisSync/wiki/What-address for the list of ECM products prefixes
            // Please send us requests to support more CMIS servers: https://github.com/nicolas-raoul/CmisSync/issues
            string[] atompubSuffixes = {
                "/cmis/atom11",
                "/alfresco/cmisatom",
                "/alfresco/service/cmis",
                "/cmis/resources/",
                "/emc-cmis-ea/resources/",
                "/xcmis/rest/cmisatom",
                "/files/basic/cmis/my/servicedoc",
                "/p8cmis/resources/Service",
                "/_vti_bin/cmis/rest?getRepositories",
                "/Nemaki/atom/bedroom",
                "/nuxeo/atom/cmis",
                "/cmis/atom"
            };
            string[] browserSuffixes = {
                "/cmis/browser"
            };
            string[] suffixes = atompubSuffixes;
            if (credentials.Binding == BindingType.Browser)
            {
                suffixes = browserSuffixes;
            }
            string bestUrl = null;

            // Try all suffixes
            for (int i = 0; i < suffixes.Length; i++)
            {
                string fuzzyUrl = prefix + suffixes[i];
                Logger.Info("Sync | Trying with " + fuzzyUrl);
                try
                {
                    ServerCredentials cred = new ServerCredentials()
                    {
                        UserName = credentials.UserName,
                        Password = credentials.Password.ToString(),
                        Address = new Uri(fuzzyUrl)
                    };
                    repositories = GetRepositories(cred);
                }
                catch (DotCMIS.Exceptions.CmisPermissionDeniedException e)
                {
                    firstException = new CmisPermissionDeniedException(e.Message, e);
                    bestUrl = fuzzyUrl;
                }
                catch (Exception e)
                {
                    // Do nothing, try other possibilities.
                    Logger.Debug(e.Message);
                }

                if (repositories != null)
                {
                    // Found!
                    return new Tuple<CmisServer, Exception>(new CmisServer(new Uri(fuzzyUrl), repositories), null);
                }
            }

            // Not found. Return also the first exception to inform the user correctly
            return new Tuple<CmisServer, Exception>(new CmisServer(bestUrl == null ? credentials.Address : new Uri(bestUrl), null), firstException);
        }

        static public Dictionary<string,string> GetCmisParameters(ServerCredentials credentials)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = credentials.Binding;
            if (credentials.Binding == BindingType.AtomPub)
            {
                cmisParameters[SessionParameter.AtomPubUrl] = credentials.Address.ToString();
            }
            else if (credentials.Binding == BindingType.Browser)
            {
                cmisParameters[SessionParameter.BrowserUrl] = credentials.Address.ToString();
            }
            cmisParameters[SessionParameter.User] = credentials.UserName;
            cmisParameters[SessionParameter.Password] = credentials.Password.ToString();
            return cmisParameters;
        }

        static public Dictionary<string, string> GetCmisParameters(CmisRepoCredentials credentials)
        {
            Dictionary<string, string> cmisParameters = GetCmisParameters((ServerCredentials)credentials);
            cmisParameters[SessionParameter.RepositoryId] = credentials.RepoId;
            return cmisParameters;
        }

        /// <summary>
        /// Get the list of repositories of a CMIS server
        /// Each item contains id + 
        /// </summary>
        /// <returns>The list of repositories. Each item contains the identifier and the human-readable name of the repository.</returns>
        static public Dictionary<string, string> GetRepositories(ServerCredentials credentials)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            // If no URL was provided, return empty result.
            if (credentials.Address == null)
            {
                return result;
            }
            
            // Create session factory.
            SessionFactory factory = SessionFactory.NewInstance();

            Dictionary<string, string> cmisParameters = GetCmisParameters(credentials);

            IList<IRepository> repositories;
            try
            {
                repositories = factory.GetRepositories(cmisParameters);
            }
            catch (DotCMIS.Exceptions.CmisPermissionDeniedException e)
            {
                Logger.Debug("CMIS server found, but permission denied. Please check username/password. " + Utils.ToLogString(e));
                throw;
            }
            catch (CmisRuntimeException e)
            {
                Logger.Debug("No CMIS server at this address, or no connection. " + Utils.ToLogString(e));
                throw;
            }
            catch (CmisObjectNotFoundException e)
            {
                Logger.Debug("No CMIS server at this address, or no connection. " + Utils.ToLogString(e));
                throw;
            }
            catch (CmisConnectionException e)
            {
                Logger.Debug("No CMIS server at this address, or no connection. " + Utils.ToLogString(e));
                throw;
            }
            catch (CmisInvalidArgumentException e)
            {
                Logger.Debug("Invalid URL, maybe Alfresco Cloud? " + Utils.ToLogString(e));
                throw;
            }

            // Populate the result list with identifier and name of each repository.
            foreach (IRepository repo in repositories)
            {
                if(!Utils.IsRepoNameHidden(repo.Name, ConfigManager.CurrentConfig.HiddenRepoNames))
                {
                    result.Add(repo.Id, repo.Name);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Get the sub-folders of a particular CMIS folder.
        /// </summary>
        /// <returns>Full path of each sub-folder, including leading slash.</returns>
        static public string[] GetSubfolders(CmisRepoCredentials credentials, string path)
        {
            List<string> result = new List<string>();

            // Connect to the CMIS repository.
            Dictionary<string, string> cmisParameters = GetCmisParameters(credentials);
            SessionFactory factory = SessionFactory.NewInstance();
            ISession session = factory.CreateSession(cmisParameters);

            // Get the folder.
            IFolder folder;
            try
            {
                folder = (IFolder)session.GetObjectByPath(path);
            }
            catch (Exception ex)
            {
                Logger.Warn(string.Format("CmisUtils | exception when session GetObjectByPath for {0}: {1}", path, Utils.ToLogString(ex)));
                return result.ToArray();
            }

            // Debug the properties count, which allows to check whether a particular CMIS implementation is compliant or not.
            // For instance, IBM Connections is known to send an illegal count.
            Logger.Info("CmisUtils | folder.Properties.Count:" + folder.Properties.Count.ToString());

            // Get the folder's sub-folders.
            IItemEnumerable<ICmisObject> children = folder.GetChildren();

            // Return the full path of each of the sub-folders.
            foreach (var subfolder in children.OfType<IFolder>())
            {
                result.Add(subfolder.Path);
            }

            return result.ToArray();
        }

        public class NodeTree
        {
            public List<NodeTree> Children = new List<NodeTree>();
            public string Path { get; set; }
            public string Name { get; set; }
            public bool Finished { get; set; }

            public NodeTree(IList<ITree<IFileableCmisObject>> trees, IFolder folder, int depth)
            {
                this.Path = folder.Path;
                this.Name = folder.Name;
                this.Finished = !(depth == 0);

                if(trees != null) {
                    foreach (ITree<IFileableCmisObject> tree in trees)
                    {
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
        static public NodeTree GetSubfolderTree(CmisRepoCredentials credentials, string path, int depth)
        {
            // Connect to the CMIS repository.
            Dictionary<string, string> cmisParameters = GetCmisParameters(credentials);
            SessionFactory factory = SessionFactory.NewInstance();
            ISession session = factory.CreateSession(cmisParameters);

            // Get the folder.
            IFolder folder;
            try
            {
                folder = (IFolder)session.GetObjectByPath(path);
            }
            catch (Exception ex)
            {
                Logger.Warn(string.Format("CmisUtils | exception when session GetObjectByPath for {0}: {1}", path, Utils.ToLogString(ex)));
                throw;
            }

            // Debug the properties count, which allows to check whether a particular CMIS implementation is compliant or not.
            // For instance, IBM Connections is known to send an illegal count.
            Logger.Info("CmisUtils | folder.Properties.Count:" + folder.Properties.Count.ToString());
            try
            {
                IList<ITree<IFileableCmisObject>> trees = folder.GetFolderTree(depth);
                return new NodeTree(trees, folder, depth);
            }
            catch (Exception e)
            {
                Logger.Info("CmisUtils getSubFolderTree | Exception " + e.Message, e);
                throw;
            }
        }
    }
}
