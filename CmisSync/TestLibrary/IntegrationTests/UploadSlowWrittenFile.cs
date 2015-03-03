
namespace TestLibrary.IntegrationTests {
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(180000), TestName("FullRepo")]
    public class UploadSlowWrittenFile : BaseFullRepoTest {
        [Test, Category("Slow")]
        public void SlowFileWriting([Values(true, false)]bool contentChanges) {
            int length = 10;
            this.ContentChangesActive = contentChanges;
            this.InitializeAndRunRepo(swallowExceptions: true);
            var file = new FileInfo(Path.Combine(this.localRootDir.FullName, "file"));
            using (var task = Task.Factory.StartNew(() => {
                using (var stream = file.Open(FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None)) {
                    for (int i = 0; i < length; i++) {
                        Thread.Sleep(1000);
                        stream.WriteByte((byte)'0');
                    }
                }
            })) {
                while (!task.Wait(1000)) {
                    this.AddStartNextSyncEvent();
                    this.repo.Run();
                }

                this.AddStartNextSyncEvent();
                this.repo.Run();
            }

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(length));
            Assert.That((this.remoteRootDir.GetChildren().First() as IDocument).ContentStreamLength, Is.EqualTo(length));
        }
    }
}