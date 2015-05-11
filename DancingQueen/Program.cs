
namespace DancingQueen {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Storage.Database;

    using DBreeze;

    using log4net;

    class MainClass {
        public static void Main(string[] args) {
            var config = ConfigManager.CurrentConfig;

            foreach (var repoInfo in config.Folders) {
                using (var dbEngine = new DBreezeEngine(repoInfo.GetDatabasePath())) {
                    var storage = new MetaDataStorage(dbEngine, new PathMatcher(repoInfo.LocalPath, repoInfo.RemotePath));
                    Console.WriteLine(string.Format("Checking {0} and DB Path \"{1}\"", repoInfo.DisplayName, repoInfo.GetDatabasePath()));
                    storage.ValidateObjectStructure();
                }
            }
        }
    }
}