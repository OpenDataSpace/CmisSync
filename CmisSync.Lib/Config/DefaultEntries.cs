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
namespace CmisSync.Lib.Config
{
    using System;
    using System.Configuration;

    public class DefaultEntries
    {
        private static DefaultEntries defaults;

        /// <summary>
        /// Lock to provide threadsafe singleton creation
        /// </summary>
        private static object configlock = new object();
        private KeyValueConfigurationCollection loadedDefaultConfig;
        private DefaultEntries() {
            Configuration exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            this.loadedDefaultConfig = exeConfig.AppSettings.Settings;
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
    }
}