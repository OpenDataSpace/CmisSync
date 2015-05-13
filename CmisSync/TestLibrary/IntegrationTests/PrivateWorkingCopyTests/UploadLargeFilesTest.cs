
namespace TestLibrary.IntegrationTests.PrivateWorkingCopyTests {
    using System;
    using System.IO;
    using System.Linq;

    using DotCMIS.Client;

    using NUnit.Framework;

    [TestFixture, TestName("PWC-Upload")]
    public class UploadLargeFilesTest : BaseFullRepoTest {
        [Test, Category("Slow"), TestCase(1024 * 1024)]
        public void Upload(int kbyte) {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            string fileName = "file.iso";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            byte[] kilo = new byte[1024];
            using (var stream = fileInfo.OpenWrite()) {
                for (int i = 0; i < kbyte; i++) {
                    stream.Write(kilo, 0, 1024);
                }
            }

            this.InitializeAndRunRepo();
            this.remoteRootDir.Refresh();
            var remoteFile = this.remoteRootDir.GetChildren().First() as IDocument;
            Assert.That(remoteFile.ContentStreamLength, Is.EqualTo(1024 * kbyte));
        }
    }
}