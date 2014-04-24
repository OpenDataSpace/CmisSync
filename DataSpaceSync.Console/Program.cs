
namespace DataSpaceSync.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using CmisSync.Lib;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Sync;

    using log4net;
    using log4net.Config;


    class ActivityListener : IActivityListener
    {
        public void ActivityStarted()
        {
        }

        public void ActivityStopped()
        {
        }
    }

    class Program
    {
        /// <summary>
        /// Mutex checking whether CmisSync is already running or not.
        /// </summary>
        private static Mutex program_mutex = new Mutex(false, "DataSpaceSync");

        /// <summary>
        /// Logging.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// Logging also to the command line if true, default is false
        /// </summary>
        private static bool verbose = false;

        static void Main(string[] args)
        {

            // Only allow one instance of DataSpace Sync (on Windows)
            if (!program_mutex.WaitOne(0, false))
            {
                System.Console.WriteLine("DataSpaceSync is already running.");
                Environment.Exit(-1);
            }
            if (File.Exists(ConfigManager.CurrentConfigFile))
                ConfigMigration.Migrate();

            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
            CmisSync.Lib.Utils.EnsureNeededDependenciesAreAvailable();
            if (args.Length != 0)
            {
                foreach(string arg in args) {
                    // Check, if the user would like to read console logs
                    if (arg.Equals("-v") || arg.Equals("--verbose"))
                        verbose = true;
                }
            }
            // Add Console Logging if user wants to
            if (verbose)
                BasicConfigurator.Configure();

            Logger.Info("Starting.");

            List<CmisRepo> repositories = new List<CmisRepo>();

            foreach (RepoInfo repoInfo in ConfigManager.CurrentConfig.Folders)
            {
                string path = repoInfo.LocalPath;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                CmisRepo repo = new CmisRepo(repoInfo, new ActivityListener());
                repositories.Add(repo);
                repo.Initialize();
            }

            while(true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
