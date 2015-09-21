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

[assembly: System.CLSCompliant(true)]
namespace DiagnoseTool {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DBreeze;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;

    using log4net;

    class MainClass {
        public static void Main(string[] args) {
            try {
                var config = ConfigManager.CurrentConfig;

                foreach (var repoInfo in config.Folders) {
                    using (var dbEngine = new DBreezeEngine(repoInfo.GetDatabasePath())) {
                        Console.WriteLine(string.Format("Checking {0} and DB Path \"{1}\"", repoInfo.DisplayName, repoInfo.GetDatabasePath()));
                        try {
                            var storage = new MetaDataStorage(dbEngine, new PathMatcher(repoInfo.LocalPath, repoInfo.RemotePath), false);
                            storage.ValidateObjectStructure();
                        } catch (Exception e) {
                            Console.WriteLine("Database object structure is invalid: " + e.Message + Environment.NewLine + e.StackTrace);
                        }
                    }
                }

                foreach (var repoInfo in config.Folders) {
                    try {
                        using (var dbEngine = new DBreezeEngine(repoInfo.GetDatabasePath())) {
                            var storage = new MetaDataStorage(dbEngine, new PathMatcher(repoInfo.LocalPath, repoInfo.RemotePath), false);
                            var ignoreStorage = new IgnoredEntitiesStorage(new IgnoredEntitiesCollection(), storage);
                            var session = SessionFactory.NewInstance().CreateSession(repoInfo, "DSS-DIAGNOSE-TOOL");
                            var remoteFolder = session.GetObjectByPath(repoInfo.RemotePath) as IFolder;
                            var filterAggregator = new FilterAggregator(
                                new IgnoredFileNamesFilter(),
                                new IgnoredFolderNameFilter(),
                                new InvalidFolderNameFilter(),
                                new IgnoredFoldersFilter());
                            var treeBuilder = new DescendantsTreeBuilder(
                                storage,
                                remoteFolder,
                                new DirectoryInfoWrapper(new DirectoryInfo(repoInfo.LocalPath)),
                                filterAggregator,
                                ignoreStorage);
                            Console.WriteLine(string.Format("Creating local, stored and remote tree in \"{0}\"", Path.GetTempPath()));
                            var trees = treeBuilder.BuildTrees();
                            var suffix = string.Format("{0}-{1}", repoInfo.DisplayName.Replace(Path.DirectorySeparatorChar,'_'), Guid.NewGuid().ToString());
                            var localTree = Path.Combine(Path.GetTempPath(), string.Format("LocalTree-{0}.dot", suffix));
                            var remoteTree = Path.Combine(Path.GetTempPath(), string.Format("StoredTree-{0}.dot", suffix));
                            var storedTree = Path.Combine(Path.GetTempPath(), string.Format("RemoteTree-{0}.dot", suffix));
                            trees.LocalTree.ToDotFile(localTree);
                            trees.StoredObjects.ObjectListToDotFile(remoteTree);
                            trees.RemoteTree.ToDotFile(storedTree);
                            Console.WriteLine(string.Format("Written to:\n{0}\n{1}\n{2}", localTree, remoteTree, storedTree));
                            CreateDiffs(trees, storage);
                        }
                    } catch (Exception ex) {
                        Console.Error.WriteLine(ex.Message);
                    }
                }
            } catch (Exception e) {
                Console.Error.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private static void CreateDiffs(DescendantsTreeCollection trees, IMetaDataStorage storage) {
            Console.WriteLine("==== Creating Diff Events ====");
            var crawlEventGenerator = new CrawlEventGenerator(storage, new FileSystemInfoFactory(ignoreReadOnlyByDefault: false));
            var eventsCollection = crawlEventGenerator.GenerateEvents(trees);
            Console.WriteLine("====   Creation Events:   ====");
            foreach (var createEvent in eventsCollection.creationEvents) {
                Console.WriteLine(createEvent);
            }

            Console.WriteLine("====  Mergeable Events:   ====");
            foreach (var mergeable in eventsCollection.mergableEvents) {
                var value = mergeable.Value;
                if (value.Item1 != null) {
                    Console.WriteLine(value.Item1);
                }

                if (value.Item2 != null) {
                    Console.WriteLine(value.Item2);
                }
            }

            Console.WriteLine("==== Finished Diff Events ====");
        }
    }
}