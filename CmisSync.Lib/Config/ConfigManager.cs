//-----------------------------------------------------------------------
// <copyright file="ConfigManager.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// A static class that allows easy access to the configuration of CmisSync.
    /// </summary>
    public static class ConfigManager {
        /// <summary>
        /// The CmisSync configuration.
        /// Following the singleton design pattern.
        /// </summary>
        private static Config config;

        /// <summary>
        /// Lock to provide threadsafe singleton creation
        /// </summary>
        private static object configlock = new object();

        /// <summary>
        /// Gets the CmisSync configuration.
        /// Following the singleton design pattern.
        /// </summary>
        public static Config CurrentConfig {
            get {
                if (config == null) {
                    lock (configlock) {
                        // Load the configuration if it has not been done yet.
                        // If no configuration file exists, it will create a default one.
                        if (config == null) {
                            config = Config.CreateOrLoadByPath(CurrentConfigFile);
                        }
                    }
                }

                // return the loaded configuration.
                return config;
            }
        }

        /// <summary>
        /// Gets the filesystem path to the XML configuration file.
        /// </summary>
        /// <returns>
        /// Platform specific absolut path to config file
        /// </returns>
        public static string CurrentConfigFile {
            get {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dataSpaceSync", "config.xml");
            }
        }
    }
}