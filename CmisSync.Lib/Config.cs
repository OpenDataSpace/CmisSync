//   CmisSync, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;

namespace CmisSync.Lib
{
    /// <summary>
    /// Configuration of a CmisSync synchronized folder.
    /// It can be found in the XML configuration file.
    /// </summary>
    public class Config
    {
        private const long DefaultChunkSize = 1024 * 1024;
        private const int DefaultPollInterval = 5000;

        /// <summary>
        /// data structure storing the configuration.
        /// </summary>
        private SyncConfig configXml;


        /// <summary>
        /// Full path to the XML configuration file.
        /// </summary>
        public string FullPath { get; private set; }


        /// <summary>
        /// Path of the folder where configuration files are.
        /// These files are in particular the XML configuration file, the database files, and the log file.
        /// </summary>
        public string ConfigPath { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether notifications should be shown, or not.
        /// </summary>
        /// <value><c>true</c> if notifications; otherwise, <c>false</c>.</value>
        public bool Notifications { get { return configXml.Notifications; } set { configXml.Notifications = value; } }

        /// <summary>
        /// Gets or sets the proxy settings.
        /// </summary>
        /// <value>The proxy.</value>
        public ProxySettings Proxy { get { return configXml.Proxy; } set{configXml.Proxy = value;} }

        /// <summary>
        /// Gets the device identifier. If no ID has been created yet, a new one is generated and saved.
        /// </summary>
        /// <value>The device identifier.</value>
        public Guid DeviceId {
            get {
                if(this.configXml.DeviceId.Equals(Guid.Empty))
                {
                    this.DeviceId = Guid.NewGuid();
                    Save();
                }
                return this.configXml.DeviceId;
            }
            private set {
                this.configXml.DeviceId = value;
            }
        }

        /// <summary>
        /// Gets the configured folders.
        /// </summary>
        /// <value>The folder.</value>
        public List<SyncConfig.Folder> Folder { get { return configXml.Folders; } }

        /// <summary>
        /// Gets the configured folder with the given name or null if no folder with this name exists.
        /// </summary>
        /// <returns>The folder.</returns>
        /// <param name="name">Name.</param>
        public SyncConfig.Folder getFolder(string name)
        {
            foreach (SyncConfig.Folder folder in configXml.Folders)
            {
                if( folder.DisplayName.Equals(name))
                    return folder;
            }
            return null;
        }

        /// <summary>
        /// Path to the user's home folder.
        /// </summary>
        public string HomePath
        {
            get
            {
                if (Backend.Platform == PlatformID.Win32NT)
                    return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                else
                    return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
        }


        /// <summary>
        /// Path where the synchronized folders are stored by default.
        /// </summary>
        public string FoldersPath
        {
            get
            {
                return Path.Combine(HomePath, "DataSpace");
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public Config(string fullPath)
        {
            FullPath = fullPath;
            ConfigPath = Path.GetDirectoryName(FullPath);
            Console.WriteLine("FullPath: " + FullPath);

            // Create configuration folder if it does not exist yet.
            if (!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);

            // Create an empty XML configuration file if none is present yet.
            if (!File.Exists(FullPath))
                CreateInitialConfig();

            // Load the XML configuration.
            try
            {
                Load();
            }
            catch (TypeInitializationException)
            {
                CreateInitialConfig();
            }
            catch (FileNotFoundException)
            {
                CreateInitialConfig();
            }
            catch (XmlException)
            {
                FileInfo file = new FileInfo(FullPath);

                // If the XML configuration file exists but with file size zero, then recreate it.
                if (file.Length == 0)
                {
                    File.Delete(FullPath);
                    CreateInitialConfig();
                }
                else
                {
                    throw new XmlException(FullPath + " does not contain a valid config XML structure.");
                }

            }
            finally
            {
                Load();
            }
        }


        /// <summary>
        /// Create an initial XML configuration file with default settings and zero remote folders.
        /// </summary>
        private void CreateInitialConfig()
        {
            // Get the user name.
            string userName = "Unknown";
            if (Backend.Platform == PlatformID.Unix ||
                Backend.Platform == PlatformID.MacOSX)
            {
                userName = Environment.UserName;
                if (string.IsNullOrEmpty(userName))
                {
                    userName = String.Empty;
                }
                else
                {
                    userName = userName.TrimEnd(",".ToCharArray());
                }
            }
            else
            {
                userName = Environment.UserName;
            }

            if (string.IsNullOrEmpty(userName))
            {
                userName = "Unknown";
            }
            // Define the default XML configuration file.
            configXml = new SyncConfig()
            {
                Folders = new List<SyncConfig.Folder>(),
                User = new User()
                {
                    EMail = "Unknown",
                    Name = userName
                },
                Notifications = true,
                Log4Net = createDefaultLog4NetElement(),
                DeviceId = Guid.NewGuid()
            };

            // Save it as an XML file.
            Save();
        }


        /// <summary>
        /// Log4net configuration, as an XML tree readily usable by Log4net.
        /// </summary>
        /// <returns></returns>
        public XmlElement GetLog4NetConfig()
        {
            return configXml.Log4Net as XmlElement;
        }

        /// <summary>
        /// Sets a new XmlNode as Log4NetConfig. Is useful for config migration
        /// </summary>
        /// <param name="node"></param>
        public void SetLog4NetConfig(XmlNode node)
        {
            this.configXml.Log4Net = node;
        }


        /// <summary>
        /// Add a synchronized folder to the configuration.
        /// </summary>
        public void AddFolder(RepoInfo repoInfo)
        {
            if (null == repoInfo)
            {
                return;
            }
            SyncConfig.Folder folder = new SyncConfig.Folder() {
                DisplayName = repoInfo.Name,
                LocalPath = repoInfo.TargetDirectory,
                IgnoredFolders = new List<IgnoredFolder>(),
                RemoteUrl = repoInfo.Address,
                RepositoryId = repoInfo.RepoID,
                RemotePath = repoInfo.RemotePath,
                UserName = repoInfo.User,
                ObfuscatedPassword = repoInfo.Password.ObfuscatedPassword,
                PollInterval = repoInfo.PollInterval,
                SupportedFeatures = null
            };
            foreach (string ignoredFolder in repoInfo.getIgnoredPaths())
            {
                folder.IgnoredFolders.Add(new IgnoredFolder(){Path = ignoredFolder});
            }
            this.configXml.Folders.Add(folder);

            Save();
        }


        /// <summary>
        /// Get the configured path to the log file.
        /// </summary>
        public string GetLogFilePath()
        {
            return Path.Combine(ConfigPath, "debug_log.txt");
        }


        /// <summary>
        /// Save the currently loaded (in memory) configuration back to the XML file.
        /// </summary>
        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SyncConfig));
            using (TextWriter textWriter = new StreamWriter(FullPath))
            {
                serializer.Serialize(textWriter, this.configXml);
            }
            HttpProxyUtils.SetDefaultProxy(this.configXml.Proxy);
        }


        private void Load()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(SyncConfig));
            using (TextReader textReader = new StreamReader(FullPath))
            {
                this.configXml = (SyncConfig)deserializer.Deserialize(textReader);
            }
            HttpProxyUtils.SetDefaultProxy(this.configXml.Proxy);
        }

        private XmlElement createDefaultLog4NetElement()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(XmlElement));
            using (TextReader textReader = new StringReader(@"
  <log4net>
    <appender name=""CmisSyncFileAppender"" type=""log4net.Appender.RollingFileAppender"">
      <file value=""" + GetLogFilePath() + @""" />
      <appendToFile value=""false"" />
      <rollingStyle value=""Size"" />
      <maxSizeRollBackups value=""10"" />
      <maximumFileSize value=""5MB"" />
      <staticLogFileName value=""true"" />
      <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""%date [%thread] %-5level %logger - %message%newline"" />
      </layout>
    </appender>
    <appender name=""ConsoleAppender"" type=""log4net.Appender.ConsoleAppender"">

      <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""%-4timestamp [%thread] %-5level %logger %ndc - %message%newline"" />
      </layout>
    </appender>
    <root>
      <level value=""INFO"" />
      <appender-ref ref=""CmisSyncFileAppender"" />
      <!-- <appender-ref ref=""ConsoleAppender"" /> -->
    </root>
  </log4net>"))
            {
                XmlElement result = (XmlElement)deserializer.Deserialize(textReader);
                return result;
            }
        }

        [XmlRoot("CmisSync", Namespace=null)]
        public class SyncConfig {
            /// <summary>
            /// Gets or sets a value indicating whether notifications are enabled or not.
            /// </summary>
            /// <value><c>true</c> if notifications; otherwise, <c>false</c>.</value>
            [XmlElement("notifications")]
            public Boolean Notifications { get; set; }
            /// <summary>
            /// Gets or sets the log4net config.
            /// </summary>
            /// <value>The log4 net.</value>
            [XmlAnyElement("log4net")]
            public XmlNode Log4Net { get; set; }
            /// <summary>
            /// List of the CmisSync synchronized folders.
            /// </summary>
            [XmlArray("folders")]
            [XmlArrayItem("folder")]
            public List<SyncConfig.Folder> Folders { get; set; }
            /// <summary>
            /// Gets or sets the user.
            /// </summary>
            /// <value>The user.</value>
            [XmlElement("user", typeof(User))]
            public User User { get; set; }
            /// <summary>
            /// Gets or sets the device identifier.
            /// </summary>
            /// <value>The device identifier.</value>
            [XmlElement("deviceId")]
            public Guid DeviceId { get; set; }
            /// <summary>
            /// Gets or sets the proxy settings.
            /// </summary>
            /// <value>The proxy.</value>
            [XmlElement("network")]
            public ProxySettings Proxy{ get; set;}
            /// <summary>
            /// Gets or sets the ignored folder names.
            /// </summary>
            /// <value>
            /// The ignored folder names.
            /// </value>
            [XmlElement("ignoreFolderNames")]
            public List<string> IgnoreFolderNames { get; set; }
            /// <summary>
            /// Gets or sets the ignored file names.
            /// </summary>
            /// <value>
            /// The ignored file names.
            /// </value>
            [XmlElement("ignoreFileNames")]
            public List<string> IgnoreFileNames { get; set; }

            public class Folder {
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
                /// </summary>
                /// <value>The remote URL.</value>
                [XmlElement("url")]
                public XmlUri RemoteUrl { get; set; }
                /// <summary>
                /// Gets or sets the repository identifier.
                /// </summary>
                /// <value>The repository identifier.</value>
                [XmlElement("repository")]
                public string RepositoryId { get; set; }
                /// <summary>
                /// Gets or sets the remote path.
                /// </summary>
                /// <value>The remote path.</value>
                [XmlElement("remoteFolder")]
                public string RemotePath { get; set; }
                /// <summary>
                /// Gets or sets the name of the user.
                /// </summary>
                /// <value>The name of the user.</value>
                [XmlElement("user")]
                public string UserName { get; set; }
                /// <summary>
                /// Gets or sets the obfuscated password.
                /// </summary>
                /// <value>The obfuscated password.</value>
                [XmlElement("password")]
                public string ObfuscatedPassword { get; set; }
                /// <summary>
                /// Gets or sets the type of the authentication.
                /// </summary>
                /// <value>The type of the auth.</value>
                [XmlElement("authType")]
                public AuthenticationType authType { get; set; }

                private double pollInterval = DefaultPollInterval;
                /// <summary>
                /// Gets or sets the poll interval.
                /// </summary>
                /// <value>The poll interval.</value>
                [XmlElement("pollinterval"), System.ComponentModel.DefaultValue(DefaultPollInterval)]
                public double PollInterval
                {
                    get { return pollInterval; }
                    set {
                        if( value <= 0 )
                        {
                            pollInterval = DefaultPollInterval;
                        }
                        else
                        {
                            pollInterval = value;
                        }
                } }

                private int uploadRetries = 2;
                /// <summary>
                /// Gets or sets the upload retries.
                /// </summary>
                /// <value>The upload retries.</value>
                [XmlElement("maxUploadRetries", IsNullable=true)]
                public int? UploadRetries
                {
                    get { return uploadRetries; }
                    set {
                        if( value==null || value < 0 )
                            uploadRetries = 2;
                        else
                            uploadRetries = (int) value;
                    }
                }

                private int downloadRetries = 2;
                /// <summary>
                /// Gets or sets download retry counter.
                /// </summary>
                /// <value>Down load retries.</value>
                [XmlElement("maxDownloadRetries", IsNullable=true)]
                public int? DownLoadRetries
                {
                    get { return downloadRetries; }
                    set {
                        if( value == null || value < 0 )
                            downloadRetries = 2;
                        else
                            downloadRetries = (int) value;
                    }
                }

                private int deletionRetries = 2;
                /// <summary>
                /// Gets or sets the deletion retry counter.
                /// </summary>
                /// <value>The deletion retries.</value>
                [XmlElement("maxDeletionRetries", IsNullable=true)]
                public int? DeletionRetries
                {
                    get { return deletionRetries; }
                    set {
                        if ( value == null || value < 0 )
                            deletionRetries = 2;
                        else
                            deletionRetries = (int) value;
                    }
                }

                /// <summary>
                /// Gets or sets the supported features.
                /// </summary>
                /// <value>The supported features.</value>
                [XmlElement("features")]
                public Feature SupportedFeatures { get; set;}

                /// <summary>
                /// Gets or sets the ignored folders.
                /// </summary>
                /// <value>The ignored folders.</value>
                [XmlElement("ignoreFolder")]
                public List<IgnoredFolder> IgnoredFolders { get; set; }

                private long chunkSize = DefaultChunkSize;
                /// <summary>
                /// Gets or sets the size of a chunk.
                /// </summary>
                /// <value>The size of the chunk.</value>
                [XmlElement("chunkSize"), System.ComponentModel.DefaultValue(DefaultChunkSize)]
                public long ChunkSize
                {
                    get { return chunkSize; }
                    set
                    {
                        if (value < 0)
                        {
                            chunkSize = 0;
                        }
                        else
                        {
                            chunkSize = value;
                        }
                    }
                }

                /// <summary>
                /// Gets or sets the ignored folder names.
                /// </summary>
                /// <value>
                /// The ignored folder names.
                /// </value>
                [XmlElement("ignoreFolderNames")]
                public List<string> IgnoreFolderNames { get; set; }

                /// <summary>
                /// Gets or sets the ignored file names.
                /// </summary>
                /// <value>
                /// The ignored file names.
                /// </value>
                [XmlElement("ignoreFileNames")]
                public List<string> IgnoreFileNames { get; set; }

                /// <summary>
                /// Get all the configured info about a synchronized folder.
                /// </summary>
                public RepoInfo GetRepoInfo()
                {
                    RepoInfo repoInfo = new RepoInfo(DisplayName, ConfigManager.CurrentConfig.ConfigPath);
                    repoInfo.User = UserName;
                    repoInfo.Password = new Credentials.Password();
                    repoInfo.Password.ObfuscatedPassword = ObfuscatedPassword;
                    repoInfo.Address = RemoteUrl;
                    repoInfo.RepoID = RepositoryId;
                    repoInfo.RemotePath = RemotePath;
                    repoInfo.TargetDirectory = LocalPath;
                    repoInfo.MaxUploadRetries = uploadRetries;
                    repoInfo.MaxDownloadRetries = downloadRetries;
                    repoInfo.MaxDeletionRetries = deletionRetries;
                    if (PollInterval < 1) PollInterval = DefaultPollInterval;
                        repoInfo.PollInterval = PollInterval;
                    foreach (IgnoredFolder ignoredFolder in IgnoredFolders)
                    {
                        repoInfo.addIgnorePath(ignoredFolder.Path);
                    }
                    if(SupportedFeatures != null && SupportedFeatures.ChunkedSupport != null && SupportedFeatures.ChunkedSupport == true)
                    {
                        repoInfo.ChunkSize = ChunkSize;
                        repoInfo.DownloadChunkSize = ChunkSize;
                    }
                    else
                    {
                        repoInfo.ChunkSize = 0;
                        repoInfo.DownloadChunkSize = 0;
                    }
                    if(SupportedFeatures != null && SupportedFeatures.ChunkedDownloadSupport!=null && SupportedFeatures.ChunkedDownloadSupport == true)
                        repoInfo.DownloadChunkSize = ChunkSize;
                    return repoInfo;
                }
            }
        }
            
        [Serializable]
        public struct IgnoredFolder
        {
            [XmlAttribute("path")]
            public string Path { get; set; }
        }

        [Serializable]
        public struct User {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            [XmlElement("name")]
            public string Name { get; set; }
            /// <summary>
            /// Gets or sets the E mail.
            /// </summary>
            /// <value>The E mail.</value>
            [XmlElement("email")]
            public string EMail { get; set; }
        }

        [Serializable]
        public class Feature {
            /// <summary>
            /// Gets or sets the getFolderTree support.
            /// </summary>
            /// <value>The getFolderTree support.</value>
            [XmlElement("getFolderTree")]
            public bool? GetFolderTreeSupport {get; set;}
            /// <summary>
            /// Gets or sets the getDescendants support.
            /// </summary>
            /// <value>The getDescendants support.</value>
            [XmlElement("getDescendants")]
            public bool? GetDescendantsSupport {get; set;}
            /// <summary>
            /// Gets or sets the getContentChanges support.
            /// </summary>
            /// <value>The getContentChanges support.</value>
            [XmlElement("getContentChanges")]
            public bool? GetContentChangesSupport {get; set;}
            /// <summary>
            /// Gets or sets the fileSystemWatcher support.
            /// </summary>
            /// <value>The fileSystemWatcher support.</value>
            [XmlElement("fileSystemWatcher")]
            public bool? FileSystemWatcherSupport {get; set;}
            /// <summary>
            /// Gets or sets the max number of content changes.
            /// </summary>
            /// <value>The max number of content changes.</value>
            [XmlElement("maxContentChanges")]
            public int? MaxNumberOfContentChanges {get; set;}
            /// <summary>
            /// Gets or sets the chunked support.
            /// </summary>
            /// <value>The chunked support.</value>
            [XmlElement("chunkedSupport")]
            public bool? ChunkedSupport {get;set;}
            /// <summary>
            /// Gets or sets the chunked download support.
            /// </summary>
            /// <value>The chunked download support.</value>
            [XmlElement("chunkedDownloadSupport")]
            public bool? ChunkedDownloadSupport {get;set;}
        }

        [Serializable]
        public enum AuthenticationType {
            /// <summary>
            /// The default auth mechanism is HTTP Basic Auth.
            /// </summary>
            BASIC,
            /// <summary>
            /// NTLM auth mechanism.
            /// </summary>
            NTLM,
            /// <summary>
            /// The Kerberos auth mechanism.
            /// </summary>
            KERBEROS,
            /// <summary>
            /// The OAuth mechanism. It is not implemented yet.
            /// </summary>
            OAUTH,
            /// <summary>
            /// The SHIBBOLETH auth mechanism. It is not implemented yet.
            /// </summary>
            SHIBBOLETH,
            /// <summary>
            /// The x501 auth mechanism. It is not implemented yet.
            /// </summary>
            X501,
            /// <summary>
            /// The PGP based auth mechanism. It is not implemented/specified/invented yet.
            /// </summary>
            PGP
        }

        [Serializable]
        public struct ProxySettings {
            [XmlAttribute("selected")]
            [DefaultValue(ProxySelection.SYSTEM)]
            public ProxySelection Selection {get; set;}
            [XmlElement("server")]
            [DefaultValue(null)]
            public XmlUri Server { get; set;}
            [XmlAttribute("loginRequired")]
            [DefaultValue(false)]
            public bool LoginRequired { get; set; }
            [XmlElement("username")]
            [DefaultValue(null)]
            public string Username {get;set;}
            [XmlElement("password")]
            [DefaultValue(null)]
            public string ObfuscatedPassword { get; set; }
        }

        [Serializable]
        public enum ProxySelection {
            /// <summary>
            /// Use the system settings.
            /// </summary>
            SYSTEM,
            /// <summary>
            /// Only connect without proxy.
            /// </summary>
            NOPROXY,
            /// <summary>
            /// Use custom proxy settings.
            /// </summary>
            CUSTOM
        }

        [Serializable]
        public class XmlUri : IXmlSerializable
        {
            private Uri _Value;

            /// <summary>
            /// Initializes a new instance of the <see cref="CmisSync.Lib.Config+XmlUri"/> class.
            /// </summary>
            public XmlUri() { }
            /// <summary>
            /// Initializes a new instance of the <see cref="CmisSync.Lib.Config+XmlUri"/> class.
            /// </summary>
            /// <param name="source">Source.</param>
            public XmlUri(Uri source) { _Value = source; }

            public static implicit operator Uri(XmlUri o)
            {
                return o == null ? null : o._Value;
            }

            public static implicit operator XmlUri(Uri o)
            {
                return o == null ? null : new XmlUri(o);
            }

            /// <summary>
            /// Gets the schema.
            /// </summary>
            /// <returns>The schema.</returns>
            public System.Xml.Schema.XmlSchema GetSchema()
            {
                return null;
            }

            /// <summary>
            /// Reads the xml.
            /// </summary>
            /// <param name="reader">Reader.</param>
            public void ReadXml(XmlReader reader)
            {
                _Value = new Uri(reader.ReadElementContentAsString());
            }

            /// <summary>
            /// Writes the xml.
            /// </summary>
            /// <param name="writer">Writer.</param>
            public void WriteXml(XmlWriter writer)
            {
                writer.WriteValue(_Value.ToString());
            }
        }
    }
}
