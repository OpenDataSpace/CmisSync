namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Sync.Solver;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    using Moq;

    [TestFixture]
    public class RemoteObjectAddedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectAdded();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAdded()
        {
            string path = Path.Combine(Path.GetTempPath(), "a");
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            var session = new Mock<ISession>();

            var storage = new Mock<IMetaDataStorage>();

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);

            Mock<IFolder> remoteObject = MockSessionUtil.CreateRemoteFolderMock(id, path, parentId, lastChangeToken);

            var solver = new RemoteObjectAdded();
            
            solver.Solve(session.Object, storage.Object, dirInfo.Object, remoteObject.Object);
            dirInfo.Verify(d => d.Create(), Times.Once());
            storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Once());
/*            storage.Verify(s => s.SaveMappedObject(It.Is<IMappedObject>(f =>
                            f.RemoteObjectId == id &&
                            f.Name == folderName &&
                            f.ParentId == parentId &&
                            f.LastChangeToken == lastChangeToken &&
                            f.Type == MappedObjectType.Folder)
                    ), Times.Once());
                    */
        }
    }
}
