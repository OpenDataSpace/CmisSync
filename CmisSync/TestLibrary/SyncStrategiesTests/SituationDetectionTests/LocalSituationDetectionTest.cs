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
        public void DefaultConstructorTest()
        {
            new LocalSituationDetection();
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void ConstructorWithIFileSystemInfoFactory()
        {
            new LocalSituationDetection(new Mock<IFileSystemInfoFactory>().Object);
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void NoChangeOnFileDetection()
        {
            string localFileName = "file.bin";
            string localFilePath = Path.Combine(Path.GetTempPath(), localFileName);

            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var storage = new Mock<IMetaDataStorage>();
            IFileInfo fileInfo = Mock.Of<IFileInfo>( f =>
                                                    f.FullName == localFilePath &&
                                                    f.Name == localFileName &&
                                                    f.Exists == true );
            IDirectoryInfo parentInfo = Mock.Of<IDirectoryInfo> ( d =>
                                                                 d.FullName == Path.GetTempPath() &&
                                                                 d.Exists == true );
            fsFactory.AddIFileInfo(fileInfo);
            fsFactory.AddIDirectoryInfo(parentInfo);
            storage.AddLocalFile(localFilePath, "id");
            var detection = new LocalSituationDetection(fsFactory.Object);
            var fileEvent = new FileEvent(fileInfo, parentInfo, null);
            fileEvent.Local = MetaDataChangeType.NONE;
            Assert.AreEqual(SituationType.NOCHANGE, detection.Analyse(storage.Object, fileEvent));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void NotSavedAndNotExistingElementProduceNoChange()
        {
            string NonExistingFileOrFolderName = "DOESNOTEXIST";
            string NonExistingFileOrFolderFullName = Path.Combine("testfolder", NonExistingFileOrFolderName);
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var storage = new Mock<IMetaDataStorage>();
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(file => file.Exists).Returns(false);
            fileInfo.Setup(file => file.Name).Returns(NonExistingFileOrFolderName);
            fileInfo.Setup(file => file.FullName).Returns(NonExistingFileOrFolderFullName);
            storage.Setup(s => s.GetObjectByLocalPath(It.Is<IFileInfo>(path => path.FullName == NonExistingFileOrFolderFullName))).Returns((AbstractMappedObject) null);
            var fileEvent = new FileEvent(fileInfo.Object, null, null);

            var detection = new LocalSituationDetection(fsFactory.Object);
            Assert.That(detection.Analyse(storage.Object, fileEvent), Is.EqualTo(SituationType.NOCHANGE));
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
            storage.Setup(s => s.GetObjectByLocalPath(It.Is<IFileSystemInfo>(path => path.FullName == ExistingFileFullName))).Returns((AbstractMappedObject) null);
            var fileEvent = new FileEvent(fileInfo.Object);

            var detection = new LocalSituationDetection(fsFactory.Object);
            Assert.That(detection.Analyse(storage.Object, fileEvent), Is.EqualTo(SituationType.ADDED));
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
            storage.AddLocalFile(NonExistingFileOrFolderFullName, "testId");
            var fileEvent = new FileEvent(fileInfo.Object);

            var detection = new LocalSituationDetection(fsFactory.Object);
            Assert.That(detection.Analyse(storage.Object, fileEvent), Is.EqualTo(SituationType.REMOVED));
        }

        [Ignore]
        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderRemovedDetectionTest()
        {
            Assert.Fail ("TODO");
        }
    }
}

