using System;

using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Data;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;
using System.IO;
using TestLibrary.TestUtils;
using CmisSync.Lib.Events;

namespace TestLibrary.SyncStrategiesTests.SituationDetectionTests
{
    [TestFixture]
    public class LocalSituationDetectionTest
    {

        [Test, Category("Fast"), Category("SituationDetection")]
        public void NoChangeOnFileDetection()
        {
            var storage = new Mock<IMetaDataStorage>();
            IFileInfo fileInfo = Mock.Of<IFileInfo>();
            var detection = new LocalSituationDetection();
            var fileEvent = new FileEvent(fileInfo);
            fileEvent.Local = MetaDataChangeType.NONE;
            Assert.AreEqual(SituationType.NOCHANGE, detection.Analyse(storage.Object, fileEvent));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileAddedDetection()
        {
            var storage = new Mock<IMetaDataStorage>();
            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(file => file.Exists).Returns(true);
            var fileEvent = new FileEvent(fileInfo.Object) { Local = MetaDataChangeType.CREATED };

            var detection = new LocalSituationDetection();
            Assert.That(detection.Analyse(storage.Object, fileEvent), Is.EqualTo(SituationType.ADDED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileRemovedDetection()
        {
            var storage = new Mock<IMetaDataStorage>();
            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(file => file.Exists).Returns(false);
            var fileEvent = new FileEvent(fileInfo.Object) {Local = MetaDataChangeType.DELETED};

            var detection = new LocalSituationDetection();
            Assert.That(detection.Analyse(storage.Object, fileEvent), Is.EqualTo(SituationType.REMOVED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderAddedDetection()
        {
            var storage = new Mock<IMetaDataStorage>();
            Mock<IDirectoryInfo> dirInfo = new Mock<IDirectoryInfo>();

            var folderEvent = new FolderEvent(dirInfo.Object) {Local = MetaDataChangeType.CREATED};

            var detection = new LocalSituationDetection();
            Assert.That(detection.Analyse(storage.Object, folderEvent), Is.EqualTo(SituationType.ADDED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderRemovedDetection()
        {
            var storage = new Mock<IMetaDataStorage>();
            Mock<IDirectoryInfo> dirInfo = new Mock<IDirectoryInfo>();

            var folderEvent = new FolderEvent(dirInfo.Object) {Local = MetaDataChangeType.DELETED};

            var detection = new LocalSituationDetection();
            Assert.That(detection.Analyse(storage.Object, folderEvent), Is.EqualTo(SituationType.REMOVED));
        }

        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderRenamedDetection()
        {
            var storage = new Mock<IMetaDataStorage>();
            Mock<IDirectoryInfo> dirInfo = new Mock<IDirectoryInfo>();

            var folderEvent = new FolderEvent(dirInfo.Object) {Local = MetaDataChangeType.CHANGED};

            var detection = new LocalSituationDetection();
            Assert.That(detection.Analyse(storage.Object, folderEvent), Is.EqualTo(SituationType.RENAMED));
        }
    }
}

