using System;

using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Strategy;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;
using System.IO;

namespace TestLibrary.SyncStrategiesTests.SituationDetectionTests
{
    [TestFixture]
    public class LocalSituationDetectionTest
    {

        [Test, Category("Fast"), Category("SituationDetection")]
        public void DefaultConstructorTest()
        {
            new LocalSituationDetection();
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void ConstructorWithIFileSystemInfoFactory()
        {
            new LocalSituationDetection(new Mock<IFileSystemInfoFactory>().Object);
        }

        // Incomplete
        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void NoChangeDetectionTest()
        {
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var storage = new Mock<IMetaDataStorage>();
            IFileInfo fileInfo = new Mock<IFileInfo>().Object;
            var detection = new LocalSituationDetection(fsFactory.Object);
            detection.Analyse(storage.Object, fileInfo);
            Assert.Fail ("TODO");
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void NotSavedAndNotExistingElementProduceNoChange()
        {
            string NonExistingFileOrFolderName = "DOESNOTEXIST";
            string NonExistingFileOrFolderFullName = Path.Combine("testfolder", NonExistingFileOrFolderName);
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var storage = new Mock<IMetaDataStorage>();
            var fileSystemInfo = new Mock<IFileSystemInfo>();
            fileSystemInfo.Setup(file => file.Exists).Returns(false);
            fileSystemInfo.Setup(file => file.Name).Returns(NonExistingFileOrFolderName);
            fileSystemInfo.Setup(file => file.FullName).Returns(NonExistingFileOrFolderFullName);
            storage.Setup(s => s.ContainsFile(It.Is<string>(path => path == NonExistingFileOrFolderFullName))).Returns(false);
            storage.Setup(s => s.ContainsFolder(It.Is<string>(path => path == NonExistingFileOrFolderFullName))).Returns(false);

            var detection = new LocalSituationDetection(fsFactory.Object);
            Assert.That(detection.Analyse(storage.Object, fileSystemInfo.Object), Is.EqualTo(SituationType.NOCHANGE));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileAddedDetection()
        {
            string ExistingFileName = "DOESEXIST";
            string ExistingFileFullName = Path.Combine("testfolder", ExistingFileName);
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var storage = new Mock<IMetaDataStorage>();
            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(file => file.Exists).Returns(true);
            fileInfo.Setup(file => file.Name).Returns(ExistingFileName);
            fileInfo.Setup(file => file.FullName).Returns(ExistingFileFullName);
            storage.Setup(s => s.ContainsFile(It.Is<string>(path => path == ExistingFileFullName))).Returns(false);
            storage.Setup(s => s.ContainsFolder(It.Is<string>(path => path == ExistingFileFullName))).Returns(false);

            var detection = new LocalSituationDetection(fsFactory.Object);
            Assert.That(detection.Analyse(storage.Object, fileInfo.Object), Is.EqualTo(SituationType.ADDED));
        }

        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderAddedDetectionTest()
        {
            Assert.Fail ("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileChangedDetectionTest()
        {
            Assert.Fail ("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileRenamedDetectionTest()
        {
            Assert.Fail ("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderRenamedDetectionTest()
        {
            Assert.Fail ("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileMovedDetectionTest()
        {
            Assert.Fail ("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderMovedDetectionTest()
        {
            Assert.Fail ("TODO");
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileRemovedDetectionTest()
        {
            string NonExistingFileOrFolderName = "DOESNOTEXIST";
            string NonExistingFileOrFolderFullName = Path.Combine("testfolder", NonExistingFileOrFolderName);
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var storage = new Mock<IMetaDataStorage>();
            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(file => file.Exists).Returns(false);
            fileInfo.Setup(file => file.Name).Returns(NonExistingFileOrFolderName);
            fileInfo.Setup(file => file.FullName).Returns(NonExistingFileOrFolderFullName);
            storage.Setup(s => s.ContainsFile(It.Is<string>(path => path == NonExistingFileOrFolderFullName))).Returns(true);
            storage.Setup(s => s.ContainsFolder(It.Is<string>(path => path == NonExistingFileOrFolderFullName))).Returns(false);

            var detection = new LocalSituationDetection(fsFactory.Object);
            Assert.That(detection.Analyse(storage.Object, fileInfo.Object), Is.EqualTo(SituationType.REMOVED));
        }

        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderRemovedDetectionTest()
        {
            Assert.Fail ("TODO");
        }
    }
}

