//-----------------------------------------------------------------------
// <copyright file="MainWindow.cs" company="GRAU DATA AG">
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
﻿
namespace DiagnoseTool {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DBreeze;

    using DotCMIS.Client;
    using DotCMIS.Client.Impl;

    using MonoMac.AppKit;
    using MonoMac.Foundation;

    public partial class MainWindow : MonoMac.AppKit.NSWindow {
        #region Constructors

        // Called when created from unmanaged code
        public MainWindow(IntPtr handle) : base(handle) {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public MainWindow(NSCoder coder) : base(coder) {
            Initialize();
        }

        // Shared initialization code
        void Initialize() {
        }

        #endregion
        public override void AwakeFromNib() {
            base.AwakeFromNib();

            var config = ConfigManager.CurrentConfig;
            this.folderSelection.RemoveAllItems();
            this.output.Editable = false;
            this.RunButton.Activated += (object sender, EventArgs e) => {
                var folder = config.Folders.Find(f => f.DisplayName == this.folderSelection.SelectedItem.Title);
                using (var dbEngine = new DBreezeEngine(folder.GetDatabasePath())) {
                    var storage = new MetaDataStorage(dbEngine, new PathMatcher(folder.LocalPath, folder.RemotePath), false);
                    try {
                        storage.ValidateObjectStructure();
                        this.output.StringValue = string.Format("{0}: DB structure of {1} is fine", DateTime.Now, folder.GetDatabasePath());
                    } catch(Exception ex) {
                        this.output.StringValue = ex.ToString();
                    }
                }
            };
            this.DumpTree.Activated += (object sender, EventArgs e) => {
                this.output.StringValue = string.Empty;
                var folder = config.Folders.Find(f => f.DisplayName == this.folderSelection.SelectedItem.Title);
                using (var dbEngine = new DBreezeEngine(folder.GetDatabasePath())) {
                    var storage = new MetaDataStorage(dbEngine, new PathMatcher(folder.LocalPath, folder.RemotePath), false);
                    try {
                        var ignoreStorage = new IgnoredEntitiesStorage(new IgnoredEntitiesCollection(), storage);
                        var session = SessionFactory.NewInstance().CreateSession(folder, "DSS-DIAGNOSE-TOOL");
                        var remoteFolder = session.GetObjectByPath(folder.RemotePath) as IFolder;
                        var filterAggregator = new FilterAggregator(
                            new IgnoredFileNamesFilter(),
                            new IgnoredFolderNameFilter(),
                            new InvalidFolderNameFilter(),
                            new IgnoredFoldersFilter());
                        var treeBuilder = new DescendantsTreeBuilder(
                            storage,
                            remoteFolder,
                            new DirectoryInfoWrapper(new DirectoryInfo(folder.LocalPath)),
                            filterAggregator,
                            ignoreStorage);
                        var trees = treeBuilder.BuildTrees();
                        var suffix = string.Format("{0}-{1}", folder.DisplayName.Replace(Path.DirectorySeparatorChar,'_'), Guid.NewGuid().ToString());
                        var localTree = Path.Combine(Path.GetTempPath(), string.Format("LocalTree-{0}.dot", suffix));
                        var remoteTree = Path.Combine(Path.GetTempPath(), string.Format("StoredTree-{0}.dot", suffix));
                        var storedTree = Path.Combine(Path.GetTempPath(), string.Format("RemoteTree-{0}.dot", suffix));
                        trees.LocalTree.ToDotFile(localTree);
                        trees.StoredTree.ToDotFile(remoteTree);
                        trees.RemoteTree.ToDotFile(storedTree);
                        this.output.StringValue = string.Format("Written to:\n{0}\n{1}\n{2}", localTree, remoteTree, storedTree);
                    } catch(Exception ex) {
                        this.output.StringValue = ex.ToString();
                    }
                }
            };

            foreach (var folder in config.Folders) {
                this.folderSelection.AddItem(folder.DisplayName);
            }
        }
    }
}