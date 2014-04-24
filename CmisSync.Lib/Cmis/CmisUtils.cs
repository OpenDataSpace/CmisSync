
namespace CmisSync.Lib.Cmis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.IO;

    using CmisSync.Lib.Config;

    using DotCMIS;
    using DotCMIS.Data;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;

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
            Url = url;
            Repositories = repositories;
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
        static public Tuple<CmisServer, Exception> GetRepositoriesFuzzy(Credentials.ServerCredentials credentials)
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
                if (e.Message == "ConnectFailure")
                    return new Tuple<CmisServer, Exception>(new CmisServer(credentials.Address, null), new CmisServerNotFoundException(e.Message, e));
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
            string[] suffixes = {
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
            string bestUrl = null;
            // Try all suffixes
            for (int i=0; i < suffixes.Length; i++)
            {
                string fuzzyUrl = prefix + suffixes[i];
                Logger.Info("Sync | Trying with " + fuzzyUrl);
                try
                {
                    Credentials.ServerCredentials cred = new Credentials.ServerCredentials()
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
                    return new Tuple<CmisServer, Exception>( new CmisServer(new Uri(fuzzyUrl), repositories), null);
                }
            }

            // Not found. Return also the first exception to inform the user correctly
            return new Tuple<CmisServer,Exception>(new CmisServer(bestUrl==null?credentials.Address:new Uri(bestUrl), null), firstException);
        }


        /// <summary>
        /// Get the list of repositories of a CMIS server
        /// Each item contains id + 
        /// </summary>
        /// <returns>The list of repositories. Each item contains the identifier and the human-readable name of the repository.</returns>
        static public Dictionary<string,string> GetRepositories(Credentials.ServerCredentials credentials)
        {
            Dictionary<string,string> result = new Dictionary<string,string>();

            // If no URL was provided, return empty result.
            if (credentials.Address == null )
            {
                return result;
            }
            
            // Create session factory.
            SessionFactory factory = SessionFactory.NewInstance();

            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = credentials.Address.ToString();
            cmisParameters[SessionParameter.User] = credentials.UserName;
            cmisParameters[SessionParameter.Password] = credentials.Password.ToString();

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
        static public string[] GetSubfolders(string repositoryId, string path,
            string address, string user, string password)
        {
            List<string> result = new List<string>();

            // Connect to the CMIS repository.
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = address;
            cmisParameters[SessionParameter.User] = user;
            cmisParameters[SessionParameter.Password] = password;
            cmisParameters[SessionParameter.RepositoryId] = repositoryId;
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
                Logger.Warn(String.Format("CmisUtils | exception when session GetObjectByPath for {0}: {1}", path, Utils.ToLogString(ex)));
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
                if (depth == 0)
                {
                    this.Finished = false;
                }
                else
                {
                    this.Finished = true;
                }

                if(trees != null)
                    foreach (ITree<IFileableCmisObject> tree in trees)
                    {
                        Folder f = tree.Item as Folder;
                        if (f != null)
                            this.Children.Add(new NodeTree(tree.Children, f, depth - 1));
                    }
            }
        }

        /// <summary>
        /// Get the sub-folders of a particular CMIS folder.
        /// </summary>
        /// <returns>Full path of each sub-folder, including leading slash.</returns>
        static public NodeTree GetSubfolderTree(Credentials.CmisRepoCredentials credentials, string path, int depth)
        {

            // Connect to the CMIS repository.
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = credentials.Address.ToString();
            cmisParameters[SessionParameter.User] = credentials.UserName;
            cmisParameters[SessionParameter.Password] = credentials.Password.ToString();
            cmisParameters[SessionParameter.RepositoryId] = credentials.RepoId;
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
                Logger.Warn(String.Format("CmisUtils | exception when session GetObjectByPath for {0}: {1}", path, Utils.ToLogString(ex)));
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

        /// <summary>
        /// Retrieve the CMIS metadata of a document.
        /// </summary>
        /// <returns>a dictionary in which each key is a type id and each value is a couple indicating the mode ("readonly" or "ReadWrite") and the value itself.</returns>
        public static Dictionary<string, string[]> FetchMetadata(ICmisObject o, IObjectType typeDef)
        {
            Dictionary<string, string[]> metadata = new Dictionary<string, string[]>();

            IList<IPropertyDefinition> propertyDefs = typeDef.PropertyDefinitions;

            // Get metadata.
            foreach (IProperty property in o.Properties)
            {
                // Mode
                string mode = "readonly";
                foreach (IPropertyDefinition propertyDef in propertyDefs)
                {
                    if (propertyDef.Id.Equals("cmis:name"))
                    {
                        Updatability updatability = propertyDef.Updatability;
                        mode = updatability.ToString();
                    }
                }

                // Value
                if (property.IsMultiValued)
                {
                    metadata.Add(property.Id, new string[] { property.DisplayName, mode, property.ValuesAsString });
                }
                else
                {
                    metadata.Add(property.Id, new string[] { property.DisplayName, mode, property.ValueAsString });
                }
            }
            return metadata;
        }

        /// <summary>
        /// Tries to set the last modified date of the given file to the last modified date of the remote document.
        /// </summary>
        /// <param name='remoteDocument'>
        /// Remote document.
        /// </param>
        /// <param name='filepath'>
        /// Filepath.
        /// </param>
        /// <param name='metadata'>
        /// Metadata of the remote file.
        /// </param>
        public static void SetLastModifiedDate(IDocument remoteDocument, string filepath, Dictionary<string, string[]> metadata)
        {
            try
            {
                if (remoteDocument.LastModificationDate != null)
                {
                    File.SetLastWriteTimeUtc(filepath, (DateTime)remoteDocument.LastModificationDate);
                }
                else
                {
                    string[] cmisModDate;
                    if (metadata.TryGetValue("cmis:lastModificationDate", out cmisModDate) && cmisModDate.Length == 3)
                    {
                        DateTime modDate = DateTime.Parse(cmisModDate[2]);
                        File.SetLastWriteTimeUtc(filepath, modDate);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Debug(String.Format("Failed to set last modified date for the local file: {0}", filepath), e);
            }
        }

        /// <summary>
        /// Tries to set the last modified date of the given folder to the last modified date of the remote folder.
        /// </summary>
        /// <param name='remoteFolder'>
        /// Remote folder.
        /// </param>
        /// <param name='folderpath'>
        /// Folderpath.
        /// </param>
        /// <param name='metadata'>
        /// Metadata ot the remote folder.
        /// </param>
        public static void SetLastModifiedDate(IFolder remoteFolder, string folderpath, Dictionary<string, string[]> metadata)
        {
            try{
                if (remoteFolder.LastModificationDate != null)
                {
                    Directory.SetLastWriteTimeUtc(folderpath, (DateTime)remoteFolder.LastModificationDate);
                }
                else
                {
                    string[] cmisModDate;
                    if (metadata.TryGetValue("cmis:lastModificationDate", out cmisModDate) && cmisModDate.Length == 3)
                    {
                        DateTime modDate = DateTime.Parse(cmisModDate[2]);
                        Directory.SetLastWriteTimeUtc(folderpath, modDate);
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Debug(String.Format("Failed to set last modified date for the local folder: {0}", folderpath), e);
            }
        }


        public static List<string> GetLocalPaths(IDocument remoteDococument, string remoteTargetFolder, string localTargetFolder) {
            List<string> results = new List<string>();
            foreach (string remotePath in remoteDococument.Paths) {
                if(remotePath.Length <= remoteTargetFolder.Length)
                    continue;
                string relativePath = remotePath.Substring(remoteTargetFolder.Length);
                if (relativePath[0] == '/')
                {
                    relativePath = relativePath.Substring(1);
                }
                string localPath = Path.Combine(remoteTargetFolder, relativePath).Replace('/', Path.DirectorySeparatorChar);
                results.Add(localPath);
            }
            return results;
        }
    }
}
