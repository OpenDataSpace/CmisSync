//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="GRAU DATA AG">
//
//  CmisSync, a collaboration and sharing tool.
//  Copyright (C) 2010  Hylke Bons &lt;hylkebons@gmail.com&gt;
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace CmisSync.Lib.Config
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Authentication type.
    /// </summary>
    [Serializable]
    public enum AuthenticationType
    {
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

    /// <summary>
    /// User informations.
    /// </summary>
    [Serializable]
    public struct User
    {
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

    /// <summary>
    /// Configuration of a CmisSync synchronized folder.
    /// It can be found in the XML configuration file.
    /// </summary>
    [XmlRoot("CmisSync", Namespace = null)]
    public class Config
    {
        /// <summary>
        /// The default size of a chunk.
        /// </summary>
        public const long DefaultChunkSize = 1024 * 1024;

        /// <summary>
        /// The default poll interval.
        /// </summary>
        public const int DefaultPollInterval = 5000;

        /// <summary>
        /// The default connection timeout is two hours.
        /// </summary>
        public const int DefaultConnectionTimeout = 2 * 60 * 60 * 1000;

        /// <summary>
        /// The default read timeout is ten minutes.
        /// </summary>
        public const int DefaultReadTimeout = 600000;

        private string fullpath;
        private string configPath;
        private Guid deviceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Config.Config"/> class.
        /// This constructor should only be called by the serializer.
        /// </summary>
        [Obsolete("This constructor should only be called by the serializer.", true)]
        public Config()
        {
        }

        private Config(string fullPath)
        {
            this.fullpath = fullPath;
            this.configPath = Path.GetDirectoryName(this.fullpath);
        }

        private bool notifications = true;

        /// <summary>
        /// Gets or sets a value indicating whether notifications are enabled or not.
        /// </summary>
        /// <value><c>true</c> if notifications; otherwise, <c>false</c>.</value>
        [XmlElement("notifications")]
        public bool Notifications
        {
            get
            {
                return notifications;
            }
            set
            {
                notifications = value;
            }
        }

        private int transmissionLimit = 100;

        /// <summary>
        /// Gets or sets a value for the transmission history list limit number
        /// </summary>
        [XmlElement("TransmissionLimit")]
        public int TransmissionLimit
        {
            get
            {
                return transmissionLimit;
            }
            set
            {
                transmissionLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets the log4net config.
        /// </summary>
        /// <value>The log4 net.</value>
        [XmlAnyElement("log4net")]
        public XmlNode Log4Net { get; set; }

        /// <summary>
        /// Gets or sets the list of the CmisSync synchronized folders.
        /// </summary>
        /// <value>
        /// The folders.
        /// </value>
        [XmlArray("folders")]
        [XmlArrayItem("folder")]
        public List<RepoInfo> Folders { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>The user.</value>
        [XmlElement("user", typeof(User))]
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the device identifier. If no ID has been created yet, a new one is generated and saved.
        /// </summary>
        /// <value>The device identifier.</value>
        [XmlElement("deviceId")]
        public Guid DeviceId
        {
            get
            {
                if(this.deviceId.Equals(Guid.Empty))
                {
                    this.deviceId = Guid.NewGuid();
                    this.Save();
                }

                return this.deviceId;
            }

            set
            {
                this.deviceId = value;
            }
        }

        /// <summary>
        /// Gets or sets the proxy settings.
        /// </summary>
        /// <value>The proxy.</value>
        [XmlElement("network")]
        public ProxySettings Proxy { get; set; }

        /// <summary>
        /// Gets or sets the list of folder name wildcards which should be ignored on sync
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
        /// Gets or sets the hidden repo names.
        /// </summary>
        /// <value>
        /// The hidden repo names.
        /// </value>
        [XmlArray("hideRepoNames")]
        [XmlArrayItem("pattern")]
        public List<string> HiddenRepoNames { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [XmlAttribute("version")]
        public double Version { get; set; }

        /// <summary>
        /// Creates the config or loads the config by path.
        /// </summary>
        /// <returns>
        /// The config instance.
        /// </returns>
        /// <param name='fullPath'>
        /// Full path.
        /// </param>
        public static Config CreateOrLoadByPath(string fullPath)
        {
            string configPath = Path.GetDirectoryName(fullPath);
            Config config;

            // Create configuration folder if it does not exist yet.
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }

            // Create an empty XML configuration file if none is present yet.
            if (!File.Exists(fullPath))
            {
                Config conf = CreateInitialConfig(fullPath);
                conf.Save();
            }

            // Load the XML configuration.
            try
            {
                config = Load(fullPath);
            }
            catch (TypeInitializationException)
            {
                config = CreateInitialConfig(fullPath);
            }
            catch (FileNotFoundException)
            {
                config = CreateInitialConfig(fullPath);
            }
            catch (XmlException)
            {
                FileInfo file = new FileInfo(fullPath);

                // If the XML configuration file exists but with file size zero, then recreate it.
                if (file.Length == 0)
                {
                    File.Delete(fullPath);
                    config = CreateInitialConfig(fullPath);
                }
                else
                {
                    throw new XmlException(fullPath + " does not contain a valid config XML structure.");
                }
            }
            finally
            {
                config = Load(fullPath);
            }

            return config;
        }

        /// <summary>
        /// Create an initial XML configuration file with default settings and zero remote folders.
        /// </summary>
        /// <param name="fullPath">
        /// Absolute full path to config file
        /// </param>
        /// <returns>the newly created config instance</returns>
        public static Config CreateInitialConfig(string fullPath)
        {
            // Get the user name.
            string userName = "Unknown";
            if (Backend.Platform == PlatformID.Unix ||
                Backend.Platform == PlatformID.MacOSX)
            {
                userName = Environment.UserName;
                if (string.IsNullOrEmpty(userName))
                {
                    userName = string.Empty;
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

            return new Config(fullPath)
            {
                Folders = new List<RepoInfo>(),
                User = new User
                {
                    EMail = "Unknown",
                    Name = userName
                },
                Notifications = true,
                Log4Net = CreateDefaultLog4NetElement(GetLogFilePath(Path.GetDirectoryName(fullPath)), GetOperationsLogFilePath(Path.GetDirectoryName(fullPath))),
                DeviceId = Guid.NewGuid(),
                IgnoreFileNames = CreateInitialListOfGloballyIgnoredFileNames(),
                IgnoreFolderNames = CreateInitialListOfGloballyIgnoredFolderNames(),
                HiddenRepoNames = CreateInitialListOfGloballyHiddenRepoNames(),
                Version = 1.1,
                fullpath = fullPath
            };
        }

        /// <summary>
        /// Creates the initial list of globally ignored file names.
        /// </summary>
        /// <returns>The initial list of globally ignored file names.</returns>
        public static List<string> CreateInitialListOfGloballyIgnoredFileNames()
        {
            List<string> list = new List<string>();
            list.Add(".*");
            list.Add("*~");
            list.Add("~*");
            list.Add("*.autosave*");
            list.Add(".DS_Store");
            list.Add("*.tmp");
            list.Add("*.~lock");
            list.Add("*.part");
            list.Add("*.crdownload");
            list.Add("*.un~");
            list.Add("*.swp");
            list.Add("*.swo");
            return list;
        }

        /// <summary>
        /// Creates the initial list of globally ignored folder names.
        /// </summary>
        /// <returns>The initial list of globally ignored folder names.</returns>
        public static List<string> CreateInitialListOfGloballyIgnoredFolderNames()
        {
            List<string> list = new List<string>();
            list.Add(".*");
            return list;
        }

        /// <summary>
        /// Creates the initial list of globally hidden repo names.
        /// </summary>
        /// <returns>The initial list of globally hidden repo names.</returns>
        public static List<string> CreateInitialListOfGloballyHiddenRepoNames()
        {
            List<string> list = new List<string>();
            list.Add("config");
            return list;
        }

        /// <summary>
        /// Gets the log file path.
        /// </summary>
        /// <returns>The log file path.</returns>
        /// <param name="configPath">Config path.</param>
        public static string GetLogFilePath(string configPath)
        {
            return Path.Combine(configPath, "debug_log.txt");
        }

        /// <summary>
        /// Gets the operations log file path.
        /// </summary>
        /// <returns>The operations log file path.</returns>
        /// <param name="configPath">Config path.</param>
        public static string GetOperationsLogFilePath(string configPath)
        {
            return Path.Combine(configPath, "operations_log.txt");
        }

        /// <summary>
        /// Gets the full path to this config file.
        /// </summary>
        /// <returns>
        /// The full path.
        /// </returns>
        public string GetFullPath()
        {
            return this.fullpath;
        }

        /// <summary>
        /// Path of the folder where configuration files are.
        /// These files are in particular the XML configuration file, the database files, and the log file.
        /// </summary>
        /// <returns>Absolute path</returns>
        public string GetConfigPath()
        {
            return this.configPath;
        }

        /// <summary>
        /// Gets the configured folder with the given name or null if no folder with this name exists.
        /// </summary>
        /// <returns>The folder.</returns>
        /// <param name="displayName">The name of the repoInfo Name.</param>
        public RepoInfo GetRepoInfo(string displayName)
        {
            foreach (RepoInfo repo in this.Folders)
            {
                if (repo.DisplayName.Equals(displayName))
                {
                    return repo;
                }
            }

            return null;
        }

        /// <summary>
        /// Path to the user's home folder.
        /// </summary>
        /// <returns>
        /// The path to the user's home folder
        /// </returns>
        public string GetHomePath()
        {
            if (Backend.Platform == PlatformID.Win32NT)
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            else
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
        }

        /// <summary>
        /// Path where the synchronized folders are stored by default.
        /// </summary>
        /// <returns>
        /// platform depending absolute path
        /// </returns>
        public string GetFoldersPath()
        {
            return Path.Combine(this.GetHomePath(), "DataSpace");
        }

        /// <summary>
        /// Log4net configuration, as an XML tree readily usable by Log4net.
        /// </summary>
        /// <returns>the log4net xml section</returns>
        public XmlElement GetLog4NetConfig()
        {
            return this.Log4Net as XmlElement;
        }

        /// <summary>
        /// Sets a new XmlNode as Log4NetConfig. Is useful for config migration
        /// </summary>
        /// <param name="node">log4net xml config section</param>
        public void SetLog4NetConfig(XmlNode node)
        {
            this.Log4Net = node;
        }

        /// <summary>
        /// Get the configured path to the log file.
        /// </summary>
        /// <returns>
        /// The default path to the log file.
        /// </returns>
        public string GetLogFilePath()
        {
            return GetLogFilePath(this.configPath);
        }

        /// <summary>
        /// Save the currently loaded (in memory) configuration back to the XML file.
        /// </summary>
        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (TextWriter textWriter = new StreamWriter(this.fullpath))
            {
                serializer.Serialize(textWriter, this);
            }

            HttpProxyUtils.SetDefaultProxy(this.Proxy);
        }

        private static Config Load(string fullPath)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(Config));
            Config config;
            using (TextReader textReader = new StreamReader(fullPath)) {
                config = (Config)deserializer.Deserialize(textReader);
            }

            config.fullpath = fullPath;
            config.configPath = Path.GetDirectoryName(fullPath);
            HttpProxyUtils.SetDefaultProxy(config.Proxy);
            return config;
        }

        private static XmlElement CreateDefaultLog4NetElement(string logFilePath, string operationsLogFilePath)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(XmlElement));
            using (TextReader textReader = new StringReader(@"
  <log4net>
    <appender name=""CmisSyncFileAppender"" type=""log4net.Appender.RollingFileAppender"">
      <file value=""" + logFilePath + @""" />
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
        <conversionPattern value=""%-4timestamp [%thread] %-5level %logger - %message%newline"" />
      </layout>
    </appender>
    <appender name=""OperationsAppender"" type=""log4net.Appender.RollingFileAppender"">
      <file value=""" + operationsLogFilePath + @""" />
      <appendToFile value=""true"" />
       <rollingStyle value=""Size"" />
      <maxSizeRollBackups value=""10"" />
      <maximumFileSize value=""5MB"" />
      <staticLogFileName value=""true"" />
      <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""%date - %property{repo}: %message%newline"" />
      </layout>
    </appender>
    <root>
      <level value=""INFO"" />
      <appender-ref ref=""CmisSyncFileAppender"" />
      <!-- <appender-ref ref=""ConsoleAppender"" /> -->
    </root>
    <logger name=""OperationsLogger"">
      <level value=""INFO"" />
      <appender-ref ref=""OperationsAppender"" />
    </logger>
  </log4net>"))
            {
                XmlElement result = (XmlElement)deserializer.Deserialize(textReader);
                return result;
            }
        }
    }
}
