//-----------------------------------------------------------------------
// <copyright file="DBreezeBenchmarkTest.cs" company="GRAU DATA AG">
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
using CmisSync.Lib.PathMatcher;
using CmisSync.Lib.Storage.Database.Entities;
using CmisSync.Lib.Storage.FileSystem;
using System.IO;

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Diagnostics;

    using CmisSync.Lib.Storage.Database;

    using DBreeze;

    using log4net;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class DBreezeBenchmarkTest : IsTestWithConfiguredLog4Net
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DBreezeBenchmarkTest));

        private readonly string requestedId = "requestedId";
        private readonly Guid requestedGuid = Guid.NewGuid();

        [Test, Category("Slow"), Category("Benchmark"), TestCaseSource(typeof(ITUtils), "DBreeze")]
        public void GetObjectByRemoteObjectId(string name, string inMemory, string path, string count) {
            var config = new DBreezeConfiguration();
            if (inMemory.Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) {
                config.Storage = DBreezeConfiguration.eStorage.MEMORY;
                config.DBreezeDataFolderName = String.Empty;
            } else {
                config.Storage = DBreezeConfiguration.eStorage.DISK;
                config.DBreezeDataFolderName = path;
            }

            int i;
            int.TryParse(count, out i);

            using (var engine = new DBreezeEngine(config)) {
                MetaDataStorage underTest = new MetaDataStorage(engine, Mock.Of<IPathMatcher>());
                this.fillDatabaseWithEntries(i, underTest);
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var obj = underTest.GetObjectByRemoteId(requestedId);
                watch.Stop();
                Logger.Debug(string.Format("Requesting one object by remote id took: {0} ms", watch.ElapsedMilliseconds));
                Assert.That(obj, Is.Not.Null);
            }
        }

        [Test, Category("Slow"), Category("Benchmark"), TestCaseSource(typeof(ITUtils), "DBreeze")]
        public void GetObjectByGuid(string name, string inMemory, string path, string count) {
            var config = new DBreezeConfiguration();
            if (inMemory.Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) {
                config.Storage = DBreezeConfiguration.eStorage.MEMORY;
                config.DBreezeDataFolderName = String.Empty;
            } else {
                config.Storage = DBreezeConfiguration.eStorage.DISK;
                config.DBreezeDataFolderName = path;
            }

            int i;
            int.TryParse(count, out i);

            using (var engine = new DBreezeEngine(config)) {
                MetaDataStorage underTest = new MetaDataStorage(engine, Mock.Of<IPathMatcher>());
                this.fillDatabaseWithEntries(i, underTest);
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var obj = underTest.GetObjectByGuid(this.requestedGuid);
                watch.Stop();
                Logger.Debug(string.Format("Requesting one object by remote GUID took: {0} ms", watch.ElapsedMilliseconds));
                Assert.That(obj, Is.Not.Null);
            }
        }

        [Test, Category("Slow"), Category("Benchmark"), TestCaseSource(typeof(ITUtils), "DBreeze")]
        public void GetObjectByLocalPath(string name, string inMemory, string path, string count) {
            var config = new DBreezeConfiguration();
            if (inMemory.Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) {
                config.Storage = DBreezeConfiguration.eStorage.MEMORY;
                config.DBreezeDataFolderName = String.Empty;
            } else {
                config.Storage = DBreezeConfiguration.eStorage.DISK;
                config.DBreezeDataFolderName = path;
            }

            int i;
            int.TryParse(count, out i);

            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.LocalTargetRootPath).Returns(Path.GetTempPath());
            matcher.Setup(m => m.CanCreateRemotePath(It.IsAny<string>())).Returns(true);
            matcher.Setup(m => m.GetRelativeLocalPath(It.IsAny<string>())).Returns(string.Empty);


            using (var engine = new DBreezeEngine(config)) {
                MetaDataStorage underTest = new MetaDataStorage(engine, matcher.Object);
                this.fillDatabaseWithEntries(i, underTest);
                Stopwatch watch = new Stopwatch();
                var localPath = Mock.Of<IFileSystemInfo>(
                    p =>
                    p.FullName == Path.GetTempPath());
                watch.Start();
                underTest.GetObjectByLocalPath(localPath);
                watch.Stop();
                Logger.Debug(string.Format("Requesting one object by remote id took: {0} ms", watch.ElapsedMilliseconds));
            }
        }

        private void fillDatabaseWithEntries(int count, IMetaDataStorage storage) {
            Logger.Debug(string.Format("Adding {0} entries to storage", count));
            Stopwatch watch = new Stopwatch();
            Stopwatch tempwatch = new Stopwatch();
            var parentFolder = new MappedObject("/", "rootId", MappedObjectType.Folder, null, "token") { Guid = Guid.NewGuid() };
            storage.SaveMappedObject(parentFolder);
            long min = long.MaxValue;
            long max = long.MinValue;

            for (int i = 0; i < count; i++) {
                var file = new MappedObject(
                    "name_" + i.ToString(),
                    i == count-1 ? this.requestedId : "id_" + i.ToString(),
                    MappedObjectType.File,
                    parentFolder.RemoteObjectId,
                    "token",
                    0)
                {
                    Guid = ((i == count - 1) ? this.requestedGuid : Guid.NewGuid())
                };
                watch.Start();
                tempwatch.Restart();
                storage.SaveMappedObject(file);
                watch.Stop();
                tempwatch.Stop();
                min = Math.Min(min, tempwatch.ElapsedMilliseconds);
                max = Math.Max(max, tempwatch.ElapsedMilliseconds);
            }

            Logger.Debug(string.Format("Added {0} objects to storage in {1} (min: {2} max: {3}) ms", count, watch.ElapsedMilliseconds, min, max));
        }
    }
}

