//-----------------------------------------------------------------------
// <copyright file="ClientBrandBase.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Cmis.UiUtils;
    using CmisSync.Lib.Config;

    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// The Base Class (template) for Client Brand support, based on CMIS
    /// The client code should derive from this class to support client brand
    /// </summary>
    public abstract class ClientBrandBase {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ClientBrandBase));
        private static readonly string DefaultRepoName = "config";
        private ISession session;

        /// <summary>
        /// Gets path list for the client brand files in CMIS server
        /// </summary>
        /// <returns>Path list for the client brand files</returns>
        public abstract List<string> PathList { get; }

        /// <summary>
        /// Gets the CMIS repository name, which holds the client brand files in CMIS server
        /// </summary>
        /// <returns>the CMIS repository name</returns>
        protected virtual string RepoName {
            get {
                return DefaultRepoName;
            }
        }

        /// <summary>
        /// Check if the CMIS server holds the client brand files
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns>Whether the CMIS server holds the client brand files</returns>
        public bool TestServer(ServerCredentials credentials) {
            IRepository repo = this.GetRepo(credentials);
            if (repo == null) {
                return false;
            }

            try {
                ISession session = repo.CreateSession();
                foreach (string path in this.PathList) {
                    try {
                        IDocument doc = session.GetObjectByPath(path) as IDocument;
                        if (doc == null) {
                            return false;
                        }
                    } catch (CmisObjectNotFoundException e) {
                        Logger.Debug(e.ErrorContent, e);
                        return false;
                    }
                }
            } catch (CmisObjectNotFoundException) {
                return false;
            } catch (Exception e) {
                Logger.Debug(e.Message, e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Setup the CMIS server to support Client Brand 
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns>Whether the CMIS server is setup</returns>
        public bool SetupServer(ServerCredentials credentials) {
            if (!this.TestServer(credentials)) {
                return false;
            }

            IRepository repo = this.GetRepo(credentials);
            if (repo == null) {
                return false;
            }

            try {
                this.session = repo.CreateSession();
                return true;
            } catch (CmisObjectNotFoundException) {
                return false;
            } catch (Exception e) {
                Logger.Debug(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Get the DateTime for the client brand file
        /// </summary>
        /// <param name="pathname">Client brand file path</param>
        /// <param name="date">The DateTime for the client brand file</param>
        /// <returns>Whether to get the DateTime for the cilent brand file</returns>
        public bool GetFileDateTime(string pathname, out DateTime date) {
            date = DateTime.Now;

            if (this.session == null) {
                return false;
            }

            try {
                IDocument doc = this.session.GetObjectByPath(pathname) as IDocument;
                if (doc == null || doc.LastModificationDate == null) {
                    return false;
                }

                date = doc.LastModificationDate.GetValueOrDefault();
                return true;
            } catch (CmisObjectNotFoundException) {
                return false;
            } catch (Exception e) {
                Logger.Debug(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Get the content for the client brand file
        /// </summary>
        /// <param name="pathname">Client brand file path</param>
        /// <param name="output">Stream to hold the client brand file</param>
        /// <returns>Whether to get the content for the client brand file</returns>
        public bool GetFile(string pathname, Stream output) {
            if (this.session == null) {
                return false;
            }

            try {
                IDocument doc = this.session.GetObjectByPath(pathname) as IDocument;
                if (doc == null) {
                    return false;
                }

                DotCMIS.Data.IContentStream contentStream = doc.GetContentStream();
                if (contentStream == null) {
                    return false;
                }

                contentStream.Stream.CopyTo(output);
                return true;
            } catch (CmisObjectNotFoundException) {
                return false;
            } catch (Exception e) {
                Logger.Debug(e.Message);
                return false;
            }
        }

        private IRepository GetRepo(ServerCredentials credentials) {
            Dictionary<string, string> parameters = CmisUtils.GetCmisParameters(credentials);
            try {
                ISessionFactory factory = SessionFactory.NewInstance();
                IList<IRepository> repos = factory.GetRepositories(parameters);
                foreach (IRepository repo in repos) {
                    if (repo.Name == this.RepoName) {
                        return repo;
                    }
                }

                return null;
            } catch (CmisObjectNotFoundException) {
                return null;
            } catch (Exception e) {
                Logger.Debug(e.Message);
                return null;
            }
        }
    }
}