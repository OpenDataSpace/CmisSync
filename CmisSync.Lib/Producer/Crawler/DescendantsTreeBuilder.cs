//-----------------------------------------------------------------------
// <copyright file="DescendantsTreeBuilder.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Producer.Crawler {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Descendants tree builder.
    /// </summary>
    /// <exception cref='ArgumentNullException'>
    /// <attribution license="cc4" from="Microsoft" modified="false" /><para>The exception that is thrown when a null
    /// reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument. </para>
    /// </exception>
    public class DescendantsTreeBuilder : IDescendantsTreeBuilder {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DescendantsTreeBuilder));
        private IMetaDataStorage storage;
        private IFolder remoteFolder;
        private IDirectoryInfo localFolder;
        private IFilterAggregator filter;
        private IPathMatcher matcher;
        private IIgnoredEntitiesStorage ignoredStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Producer.Crawler.DescendantsTreeBuilder"/> class.
        /// </summary>
        /// <param name='storage'>
        /// The MetadataStorage.
        /// </param>
        /// <param name='remoteFolder'>
        /// Remote folder.
        /// </param>
        /// <param name='localFolder'>
        /// Local folder.
        /// </param>
        /// <param name='filter'>
        /// Aggregated Filters.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// <attribution license="cc4" from="Microsoft" modified="false" /><para>The exception that is thrown when a
        /// null reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument. </para>
        /// </exception>
        public DescendantsTreeBuilder(
            IMetaDataStorage storage,
            IFolder remoteFolder,
            IDirectoryInfo localFolder,
            IFilterAggregator filter,
            IIgnoredEntitiesStorage ignoredStorage)
        {
            if (remoteFolder == null) {
                throw new ArgumentNullException("remoteFolder");
            }

            if (localFolder == null) {
                throw new ArgumentNullException("localFolder");
            }

            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            if (filter == null) {
                throw new ArgumentNullException("filter");
            }

            if (ignoredStorage == null) {
                throw new ArgumentNullException("ignoredStorage");
            }

            this.storage = storage;
            this.remoteFolder = remoteFolder;
            this.localFolder = localFolder;
            this.filter = filter;
            this.matcher = new PathMatcher(localFolder.FullName, remoteFolder.Path);
            this.ignoredStorage = ignoredStorage;
        }

        /// <summary>
        /// Gets the local directory tree.
        /// </summary>
        /// <returns>The local directory tree.</returns>
        /// <param name="parent">Parent directory.</param>
        /// <param name="filter">Filter for files.</param>
        public static IObjectTree<IFileSystemInfo> GetLocalDirectoryTree(IDirectoryInfo parent, IFilterAggregator filter) {
            var children = new List<IObjectTree<IFileSystemInfo>>();
            try {
                foreach (var child in parent.GetDirectories()) {
                    string reason;
                    if (!filter.InvalidFolderNamesFilter.CheckFolderName(child.Name, out reason) && !filter.FolderNamesFilter.CheckFolderName(child.Name, out reason) && !filter.SymlinkFilter.IsSymlink(child, out reason)) {
                        children.Add(GetLocalDirectoryTree(child, filter));
                    } else {
                        Logger.Info(reason);
                    }
                }

                foreach (var file in parent.GetFiles()) {
                    string reason;
                    if (!filter.FileNamesFilter.CheckFile(file.Name, out reason) && !filter.SymlinkFilter.IsSymlink(file, out reason)) {
                        children.Add(new ObjectTree<IFileSystemInfo> {
                            Item = file,
                            Children = new List<IObjectTree<IFileSystemInfo>>()
                        });
                    } else {
                        Logger.Info(reason);
                    }
                }
            } catch (System.IO.PathTooLongException) {
                Logger.Fatal(string.Format("One or more children paths of \"{0}\" are to long to be synchronized, synchronization is impossible since the problem is fixed", parent.FullName));
                throw;
            }

            IObjectTree<IFileSystemInfo> tree = new ObjectTree<IFileSystemInfo> {
                Item = parent,
                Children = children
            };
            return tree;
        }

        /// <summary>
        /// Gets the remote directory tree.
        /// </summary>
        /// <returns>The remote directory tree.</returns>
        /// <param name="parent">Parent folder.</param>
        /// <param name="descendants">Descendants of remote object.</param>
        /// <param name="filter">Filter of ignored or invalid files and folder</param>
        public static IObjectTree<IFileableCmisObject> GetRemoteDirectoryTree(IFolder parent, IList<ITree<IFileableCmisObject>> descendants, IFilterAggregator filter, IIgnoredEntitiesStorage ignoredStorage, IPathMatcher matcher) {
            IList<IObjectTree<IFileableCmisObject>> children = new List<IObjectTree<IFileableCmisObject>>();
            if (descendants != null) {
                foreach (var child in descendants) {
                    if (child.Item is IFolder) {
                        string reason;
                        var folder = child.Item as IFolder;
                        if (!filter.FolderNamesFilter.CheckFolderName(folder.Name, out reason) && !filter.InvalidFolderNamesFilter.CheckFolderName(folder.Name, out reason)) {
                            if (folder.AreAllChildrenIgnored()) {
                                ignoredStorage.AddOrUpdateEntryAndDeleteAllChildrenFromStorage(new IgnoredEntity(folder, matcher));
                                Logger.Info(string.Format("Folder {0} with Id {1} is ignored", folder.Name, folder.Id));
                                children.Add(new ObjectTree<IFileableCmisObject> {
                                    Item = child.Item,
                                    Children = new List<IObjectTree<IFileableCmisObject>>()
                                });
                            } else {
                                ignoredStorage.Remove(folder.Id);
                                children.Add(GetRemoteDirectoryTree(folder, child.Children, filter, ignoredStorage, matcher));
                            }
                        } else {
                            Logger.Info(reason);
                        }
                    } else if (child.Item is IDocument) {
                        string reason;
                        if (!filter.FileNamesFilter.CheckFile(child.Item.Name, out reason)) {
                            children.Add(new ObjectTree<IFileableCmisObject> {
                                Item = child.Item,
                                Children = new List<IObjectTree<IFileableCmisObject>>()
                            });
                        } else {
                            Logger.Info(reason);
                        }
                    }
                }
            }

            var tree = new ObjectTree<IFileableCmisObject> {
                Item = parent,
                Children = children
            };

            return tree;
        }

        /// <summary>
        /// Builds the trees asynchronously by crawling storage, FileSystem and Server.
        /// </summary>
        /// <returns>
        /// The trees as a struct.
        /// </returns>
        public DescendantsTreeCollection BuildTrees() {
            IObjectTree<IMappedObject> storedTree = null;
            IObjectTree<IFileSystemInfo> localTree = null;
            IObjectTree<IFileableCmisObject> remoteTree = null;

            /*
            // Request 3 trees in parallel
            Task[] tasks = new Task[3];
            tasks[0] = Task.Factory.StartNew(() => );
            tasks[1] = Task.Factory.StartNew(() => );
            tasks[2] = Task.Factory.StartNew(() => );

            // Wait until all tasks are finished
            Task.WaitAll(tasks);
            */
            Logger.Debug("Crawling local fs");
            localTree = GetLocalDirectoryTree(this.localFolder, this.filter);
            Logger.Debug("Crawling remote fs");
            remoteTree = GetRemoteDirectoryTree(this.remoteFolder, this.remoteFolder.GetDescendants(-1), this.filter, this.ignoredStorage, this.matcher);
            Logger.Debug("Building stored tree");
            storedTree = this.storage.GetObjectTree();
            Logger.Debug("Finished building trees");
            return new DescendantsTreeCollection(storedTree, localTree, remoteTree);
        }
    }
}