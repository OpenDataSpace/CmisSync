//-----------------------------------------------------------------------
// <copyright file="RepoInfo.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Config {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;

    using DotCMIS;

    /// <summary>
    /// All the info for a particular CmisSync synchronized folder.
    /// Contains local info, as well as remote info to connect to the CMIS folder.
    /// </summary>
    [Serializable]
    public class RepoInfo {
        private CmisRepoCredentials credentials = new CmisRepoCredentials();
        private double pollInterval = Config.DefaultPollInterval;
        private long chunkSize = Config.DefaultChunkSize;
        private int connectionTimeout = Config.DefaultConnectionTimeout;
        private int readTimeout = Config.DefaultReadTimeout;
        private int uploadRetries = 2;
        private int downloadRetries = 2;
        private int deletionRetries = 2;
        private List<IgnoredFolder> ignoredFolders;

        /// <summary>
        /// Occurs when saved.
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        [XmlElement("name")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the local path.
        /// </summary>
        /// <value>The local path.</value>
        [XmlElement("path")]
        public string LocalPath { get; set; }

        /// <summary>
        /// Gets or sets the remote URL.
        /// For instance: https://demo.deutsche-wolke.de/cmis/atom
        /// </summary>
        /// <value>The remote URL.</value>
        [XmlElement("url")]
        public XmlUri Address {
            get {
                return this.credentials.Address;
            }

            set {
                this.credentials.Address = value;
            }
        }

        /// <summary>
        /// Gets or sets the binding type.
        /// </summary>
        /// <value>The binding type.</value>
        [XmlElement("binding"), System.ComponentModel.DefaultValue(BindingType.AtomPub)]
        public string Binding {
            get {
                return this.credentials.Binding;
            }

            set {
                this.credentials.Binding = value;
            }
        }

        /// <summary>
        /// Gets or sets the remote path on the remote server, starting from the root of the CMIS repository.
        /// </summary>
        /// <value>
        /// The remote path.
        /// </value>
        [XmlElement("remoteFolder")]
        public string RemotePath { get; set; }

        /// <summary>
        /// Gets or sets the supported features.
        /// </summary>
        /// <value>The supported features.</value>
        [XmlElement("features")]
        public Feature SupportedFeatures { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        [XmlElement("user")]
        public string User {
            get {
                return this.credentials.UserName;
            }

            set {
                this.credentials.UserName = value;
            }
        }

        /// <summary>
        /// Gets or sets the obfuscated password.
        /// </summary>
        /// <value>The obfuscated password.</value>
        [XmlElement("password")]
        public string ObfuscatedPassword {
            get {
                return this.credentials.Password.ObfuscatedPassword;
            }

            set {
                this.credentials.Password = new Password { ObfuscatedPassword = value };
            }
        }

        /// <summary>
                /// Gets or sets the type of the authentication.
                /// </summary>
                /// <value>The type of the auth.</value>
        [XmlElement("authType")]
        public AuthenticationType AuthenticationType { get; set; }

        /// <summary>
                /// Gets or sets the repository identifier.
                /// </summary>
                /// <value>The repository identifier.</value>
        [XmlElement("repository")]
        public string RepositoryId {
            get {
                return this.credentials.RepoId;
            }

            set {
                this.credentials.RepoId = value;
            }
        }

        /// <summary>
        /// Gets or sets the poll interval in milliseconds.
        /// CmisSync.Lib will sync this remote folder once during this interval of time.
        /// </summary>
        /// <value>The poll interval.</value>
        [XmlElement("pollinterval"), System.ComponentModel.DefaultValue(Config.DefaultPollInterval)]
        public double PollInterval {
            get {
                return this.pollInterval;
            }

            set {
                this.pollInterval = value > 0 ? value : Config.DefaultPollInterval;
            }
        }

        /// <summary>
        /// Gets or sets the connection timeout.
        /// If zero or negative number is passed, it will be converted to -1
        /// </summary>
        /// <value>The connection timeout.</value>
        [XmlElement("connectionTimeout"), System.ComponentModel.DefaultValue(Config.DefaultConnectionTimeout)]
        public int ConnectionTimeout {
            get {
                return this.connectionTimeout;
            }

            set {
                this.connectionTimeout = value > 0 ? value : -1;
            }
        }

        /// <summary>
        /// Gets or sets the read timeout.
        /// If zero or negative number is passed, it will be converted to -1
        /// </summary>
        /// <value>The read timeout.</value>
        [XmlElement("readTimeout"), System.ComponentModel.DefaultValue(Config.DefaultReadTimeout)]
        public int ReadTimeout {
            get {
                return this.readTimeout;
            }

            set {
                this.readTimeout = value > 0 ? value : -1;
            }
        }

        /// <summary>
        /// Gets or sets the ignored folder names.
        /// </summary>
        /// <value>
        /// The ignored folder names.
        /// </value>
        [XmlArray("ignoreFolderNames")]
        [XmlArrayItem("pattern")]
        public List<string> IgnoreFolderNames { get; set; }

        /// <summary>
        /// Gets or sets the ignored file names.
        /// </summary>
        /// <value>
        /// The ignored file names.
        /// </value>
        [XmlArray("ignoreFileNames")]
        [XmlArrayItem("pattern")]
        public List<string> IgnoreFileNames { get; set; }

        /// <summary>
        /// Gets or sets the size of a chunk.
        /// If none zero, CmisSync will divide the document by chunk size for download/upload.
        /// </summary>
        /// <value>The size of the chunk.</value>
        [XmlElement("chunkSize"), System.ComponentModel.DefaultValue(Config.DefaultChunkSize)]
        public long ChunkSize {
            get {
                return this.chunkSize;
            }

            set {
                this.chunkSize = (value < 0) ? 0 : value;
            }
        }

        /// <summary>
        /// Gets or sets the max upload retries.
        /// </summary>
        /// <value>The upload retries.</value>
        [XmlElement("maxUploadRetries", IsNullable = true)]
        public int? MaxUploadRetries {
            get {
                return this.uploadRetries;
            }

            set {
                this.uploadRetries = (value == null || value < 0) ? 2 : (int)value;
            }
        }

        /// <summary>
        /// Gets or sets download retry counter.
        /// </summary>
        /// <value>Down load retries.</value>
        [XmlElement("maxDownloadRetries", IsNullable = true)]
        public int? MaxDownloadRetries {
            get {
                return this.downloadRetries;
            }

            set {
                this.downloadRetries = (value == null || value < 0) ? 2 : (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the deletion retry counter.
        /// </summary>
        /// <value>The deletion retries.</value>
        [XmlElement("maxDeletionRetries", IsNullable = true)]
        public int? MaxDeletionRetries {
            get {
                return this.deletionRetries;
            }

            set {
                this.deletionRetries = (value == null || value < 0) ? 2 : (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the ignored folders.
        /// </summary>
        /// <value>The ignored folders.</value>
        [XmlElement("ignoreFolder")]
        public List<IgnoredFolder> IgnoredFolders {
            get {
                if (this.ignoredFolders == null) {
                    return new List<IgnoredFolder>();
                } else {
                    return this.ignoredFolders;
                }
            }

            set {
                this.ignoredFolders = value;
            }
        }

        [XmlElement("downloadLimit"), System.ComponentModel.DefaultValue(0)]
        public long DownloadLimit { get; set; }

        [XmlElement("uploadLimit"), System.ComponentModel.DefaultValue(0)]
        public long UploadLimit { get; set; }

        /// <summary>
        /// Gets the CmisRepoCredentials
        /// </summary>
        public CmisRepoCredentials Credentials {
            get {
                return this.credentials;
            }
        }

        /// <summary>
        /// Gets the ignored paths.
        /// </summary>
        /// <returns>The ignored paths.</returns>
        public List<string> GetIgnoredPaths() {
            List<string> list = new List<string>();

            foreach (IgnoredFolder folder in this.IgnoredFolders) {
                list.Add(folder.Path);
            }

            return list;
        }

        /// <summary>
        /// Adds the ignore path.
        /// </summary>
        /// <param name='ignorePath'>
        /// Ignore path.
        /// </param>
        public void AddIgnorePath(string ignorePath) {
            this.IgnoredFolders.Add(new IgnoredFolder { Path = ignorePath });
        }

        /// <summary>
        /// Full path to the local database file/folder.
        /// </summary>
        /// <returns>
        /// Full path
        /// </returns>
        public virtual string GetDatabasePath() {
            string name = this.DisplayName.Replace("\\", "_");
            name = name.Replace("/", "_");
            return Path.Combine(ConfigManager.CurrentConfig.GetConfigPath(), name + "_DB");
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <returns>The password.</returns>
        public virtual Password GetPassword() {
            return new Password { ObfuscatedPassword = this.credentials.Password.ObfuscatedPassword };
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="password">Password instance.</param>
        public virtual void SetPassword(Password password) {
            this.credentials.Password = new Password { ObfuscatedPassword = password.ObfuscatedPassword };
        }

        /// <summary>
        /// If the given path should be ignored, TRUE will be returned,
        /// otherwise FALSE.
        /// </summary>
        /// <param name="path">
        /// Path to be checked
        /// </param>
        /// <returns>
        /// <c>true</c> if the path should be ignored, otherwise <c>false</c>
        /// </returns>
        public bool IsPathIgnored(string path) {
            string[] names = path.Split("/".ToCharArray());
            foreach (string name in names) {
                if (Utils.IsInvalidFolderName(name, new List<string>())) {
                    return true;
                }
            }

            return !string.IsNullOrEmpty(this.GetIgnoredPaths().Find(delegate(string ignore) {
                if (string.IsNullOrEmpty(ignore)) {
                    return false;
                }

                if (path.Equals(ignore)) {
                    return true;
                }

                return path.StartsWith(ignore) && path[ignore.Length] == '/';
            }));
        }

        /// <summary>
        /// Raises the saved event.
        /// </summary>
        public void OnSaved() {
            var handler = this.Saved;
            if (handler != null) {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Ignored folder
        /// </summary>
        [Serializable]
        public struct IgnoredFolder {
            /// <summary>
            /// Gets or sets the path of the ignored folder.
            /// </summary>
            /// <value>
            /// The path.
            /// </value>
            [XmlAttribute("path")]
            public string Path { get; set; }
        }
    }
}