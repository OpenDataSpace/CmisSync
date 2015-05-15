
namespace DancingQueen {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Storage.Database;

    using DBreeze;

    using DotCMIS.Client;

    using log4net;

    class MainClass {
        public static void Main(string[] args) {
            var config = ConfigManager.CurrentConfig;

            foreach (var repoInfo in config.Folders) {
                using (var dbEngine = new DBreezeEngine(repoInfo.GetDatabasePath())) {
                    var storage = new MetaDataStorage(dbEngine, new PathMatcher(repoInfo.LocalPath, repoInfo.RemotePath), false);
                    Console.WriteLine(string.Format("Checking {0} and DB Path \"{1}\"", repoInfo.DisplayName, repoInfo.GetDatabasePath()));
                    storage.ValidateObjectStructure();
                    /*var treeBuilder = new DescendantsTreeBuilder(storage, null, null, null, null);
                    Console.WriteLine(string.Format("Creating local, stored and remote tree in \"{0}\"", Path.GetTempPath()));
                    var trees = treeBuilder.BuildTrees();
                    trees.LocalTree.ToDotFile(Path.Combine(Path.GetTempPath(), "LocalTree.dot"));
                    trees.StoredTree.ToDotFile(Path.Combine(Path.GetTempPath(), "StoredTree.dot"));
                    trees.RemoteTree.ToDotFile(Path.Combine(Path.GetTempPath(), "RemoteTree.dot"));*/
                }
            }
        }
    }
}