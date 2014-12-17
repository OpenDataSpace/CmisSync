
namespace TestLibrary.IntegrationTests.SelectiveIgnoreTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Timeout(900000), TestName("SelectiveIgnore")]
    public class FileCrud : BaseFullRepoTest
    {
        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void LocalFileIsCreatedInIgnoredFolder() {
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            var folder = this.remoteRootDir.CreateFolder("ignored");

            this.repo.Initialize();
            this.repo.Run();

            folder.Refresh();
            folder.IgnoreAllChildren();

            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            var localFolder = this.localRootDir.GetDirectories()[0].FullName;
            var fileInfo = new FileInfo(Path.Combine(localFolder, "file.txt"));
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine("content");
            }

            this.WaitUntilQueueIsNotEmpty();
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            folder.Refresh();
            Assert.Throws<CmisObjectNotFoundException>(() => this.session.GetObjectByPath(folder.Path + "/file.txt"));
        }
    }
}