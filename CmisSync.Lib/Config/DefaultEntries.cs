//-----------------------------------------------------------------------
// <copyright file="DefaultEntries.cs" company="GRAU DATA AG">
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
    using System.Configuration;

    using log4net;

    /// <summary>
    /// Default entries loaded from program.exe.config file.
    /// </summary>
    public class DefaultEntries {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DefaultEntries));
        private static DefaultEntries defaults;

        /// <summary>
        /// Lock to provide threadsafe singleton creation
        /// </summary>
        private static object configlock = new object();
        private KeyValueConfigurationCollection loadedDefaultConfig;

        private DefaultEntries() {
            Configuration exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            this.loadedDefaultConfig = exeConfig.AppSettings.Settings;
            Logger.Debug(string.Format("Loading application settings from {0}", exeConfig.FilePath));
            this.Url = "https://";
            this.Name = Environment.UserName;
            this.Binding = null;
            this.CanModifyUrl = true;
            if (this.loadedDefaultConfig["Url"] != null) {
                this.Url = this.loadedDefaultConfig["Url"].Value ?? "https://";
            }

            if (this.loadedDefaultConfig["Name"] != null) {
                this.Name = this.loadedDefaultConfig["Name"].Value ?? Environment.UserName;
            }

            if (this.loadedDefaultConfig["Binding"] != null) {
                this.Binding = this.loadedDefaultConfig["Binding"].Value;
            }

            if (this.loadedDefaultConfig["UrlModificationAllowed"] != null) {
                bool canModify;
                if (Boolean.TryParse(this.loadedDefaultConfig["UrlModificationAllowed"].Value, out canModify)) {
                    this.CanModifyUrl = canModify;
                }
            }
        }

        /// <summary>
        /// Gets the defaults as singleton.
        /// </summary>
        /// <value>The defaults.</value>
        public static DefaultEntries Defaults {
            get {
                if (defaults == null) {
                    lock (configlock) {
                        // Load the configuration if it has not been done yet.
                        // If no configuration file exists, it will create a default one.
                        if (defaults == null) {
                            defaults = new DefaultEntries();
                        }
                    }
                }

                // return the loaded configuration.
                return defaults;
            }
        }

        /// <summary>
        /// Gets the default URL scheme for new connections.
        /// </summary>
        /// <value>The default URL.</value>
        public string Url { get; private set; }

        /// <summary>
        /// Gets the default user name for new connections.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the default binding for new connections.
        /// </summary>
        /// <value>The binding.</value>
        public string Binding { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the Url instance can be modified.
        /// </summary>
        /// <value><c>true</c> if the user should be able to modify URL; otherwise, <c>false</c>.</value>
        public bool CanModifyUrl { get; private set; }
    }
}