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
            if (this.loadedDefaultConfig["Url"] != null) {
                this.Url = this.loadedDefaultConfig["Url"].Value ?? "https://";
            }

            if (this.loadedDefaultConfig["Name"] != null) {
                this.Name = this.loadedDefaultConfig["Name"].Value ?? Environment.UserName;
            }

            if (this.loadedDefaultConfig["Binding"] != null) {
                this.Binding = this.loadedDefaultConfig["Binding"].Value;
            }
        }

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

        public string Url { get; private set; }

        public string Name { get; private set; }

        public string Binding { get; private set; }
    }
}