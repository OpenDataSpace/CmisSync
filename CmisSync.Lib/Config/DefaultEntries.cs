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