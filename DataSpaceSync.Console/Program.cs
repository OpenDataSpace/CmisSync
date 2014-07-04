//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="GRAU DATA AG">
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
        private static Mutex programMutex = new Mutex(false, "DataSpaceSync");

        /// <summary>
        /// Logging.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// Logging also to the command line if true, default is false
        /// </summary>
        private static bool verbose = false;

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static void Main(string[] args)
        {
            // Only allow one instance of DataSpace Sync (on Windows)
            if (!programMutex.WaitOne(0, false)) {
                System.Console.WriteLine("DataSpaceSync is already running.");
                Environment.Exit(-1);
            }

            if (File.Exists(ConfigManager.CurrentConfigFile)) {
                ConfigMigration.Migrate();
            }

            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
            CmisSync.Lib.Utils.EnsureNeededDependenciesAreAvailable();
            if (args.Length != 0) {
                foreach (string arg in args) {
                    // Check, if the user would like to read console logs
                    if (arg.Equals("-v") || arg.Equals("--verbose")) {
                        verbose = true;
                    }
                }
            }

            // Add Console Logging if user wants to
            if (verbose) {
                BasicConfigurator.Configure();
            }

            Logger.Info("Starting.");

            List<CmisRepo> repositories = new List<CmisRepo>();

            foreach (RepoInfo repoInfo in ConfigManager.CurrentConfig.Folders) {
                string path = repoInfo.LocalPath;
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                CmisRepo repo = new CmisRepo(repoInfo, new ActivityListener());
                repositories.Add(repo);
                repo.Initialize();
            }

            while (true) {
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
