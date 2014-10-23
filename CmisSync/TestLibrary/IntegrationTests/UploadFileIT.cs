
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;

    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Ignore("Future Issue")]
    public class UploadFileIT : NeedsLocalFileSystemFolder
    {
        [TestFixtureSetUp]
        public void SetUpFixture() {
            this.TestFixtureSetUp();
        }

        [SetUp]
        public void SetUpTestDir() {
            this.InitLocalTestDir();
        }

        [TearDown]
        public void CleanUpTestDir() {
            this.RemoveLocalTestDir();
        }

        [Test, Category("Medium")]
        public void UploadWhileAnotherProcessIsWritingToFile() {
            var fileName = "slowFile.txt";
            var chunkSize = 1024;
            var chunks = 100;
            byte[] chunk = new byte[chunkSize];
            var finalLength = chunks * chunkSize;
            var file = new FileInfo(Path.Combine(this.LocalTestDir.FullName, fileName));
            var mockedDocument = new Mock<IDocument>();
            var transmissionStatus = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, fileName);
            mockedDocument.Setup(doc => doc.Name).Returns(fileName);
            using (var remoteStream = new MemoryStream()) {
                mockedDocument.Setup(doc => doc.SetContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b == true), It.Is<bool>(b => b == true)))
                    .Callback<IContentStream, bool, bool>((s, b, r) => s.Stream.CopyTo(remoteStream))
                        .Returns(new Mock<IObjectId>().Object);
                using (var fileStream = file.Open(FileMode.CreateNew, FileAccess.Write, FileShare.Read)) {
                    using (var task = Task.Factory.StartNew(() => {
                        var newFileHandle = new FileInfo(file.FullName);
                        using (var hashAlg = new SHA1Managed())
                        using (var readingFileStream = newFileHandle.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var uploader = new SimpleFileUploader()) {
                            uploader.UploadFile(mockedDocument.Object, readingFileStream, transmissionStatus, hashAlg);
                        }

                        Assert.That(remoteStream.Length, Is.EqualTo(finalLength));
                    })) {
                        for (int i = 0; i < chunks; i++) {
                            Thread.Sleep(10);
                            fileStream.Write(chunk, 0, chunkSize);
                        }

                        task.Wait();
                    }
                }
            }
        }
    }
}