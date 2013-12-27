using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using CmisSync.Lib;
using CmisSync.Lib.Cmis;
using CmisSync.Lib.Tasks;
using CmisSync.Lib.Events;

using DotCMIS.Client;
using DotCMIS.Data;
using DotCMIS.Exceptions;

using Moq;

using NUnit.Framework;

namespace TestLibrary
{
    [TestFixture]
    public class SimpleFileDownloaderTest : IDisposable
    {
        private bool disposed = false;
        private FileTransmissionEvent transmissionEvent;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private long remoteLength;
        private Stream remoteStream;
        private byte[] remoteContent;
        private RandomNumberGenerator random;

        [SetUp]
        public void SetUp ()
        {
            transmissionEvent = new FileTransmissionEvent (FileTransmissionType.DOWNLOAD_NEW_FILE, "testfile");
            localFileStream = new MemoryStream ();
            hashAlg = new SHA1Managed ();
            remoteLength = 1024 * 1024;
            remoteContent = new byte[remoteLength];
            random = RandomNumberGenerator.Create();
            random.GetBytes(remoteContent);
            remoteStream = new MemoryStream (remoteContent);
        }

        [Test, Category("Fast")]
        public void NormalDownloadTest ()
        {
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            mockedStream.Setup (stream => stream.Length).Returns (remoteLength);
            mockedStream.Setup (stream => stream.Stream).Returns (remoteStream);
            mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
            mock.Setup (doc => doc.GetContentStream ()).Returns (mockedStream.Object);
            using (FileDownloader downloader = new SimpleFileDownloader()) {
                downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (remoteContent.Length, localFileStream.Length);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteContent), hashAlg.Hash);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localFileStream.ToArray ()), hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        public void ServerFailedExceptionTest ()
        {
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            var mockedMemStream = new Mock<MemoryStream> (remoteContent){CallBase = true};
            mockedMemStream.Setup (memstream => memstream.Read (It.IsAny<byte[]> (), It.IsAny<int> (), It.IsAny<int> ())).Throws<CmisConnectionException>();
            mockedStream.Setup (stream => stream.Length).Returns (remoteLength);
            mockedStream.Setup (stream => stream.Stream).Returns (mockedMemStream.Object);
            mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
            mock.Setup (doc => doc.GetContentStream ()).Returns (mockedStream.Object);
            using (FileDownloader downloader = new SimpleFileDownloader()) {
                try {
                    downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                    Assert.Fail ();
                } catch (CmisConnectionException) {
                }
            }
        }

        [Test, Category("Fast")]
        public void IOExceptionTest() {
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            var mockedMemStream = new Mock<MemoryStream> (remoteContent){CallBase = true};
            mockedMemStream.Setup (memstream => memstream.Read (It.IsAny<byte[]> (), It.IsAny<int> (), It.IsAny<int> ())).Throws<IOException>();
            mockedStream.Setup (stream => stream.Length).Returns (remoteLength);
            mockedStream.Setup (stream => stream.Stream).Returns (mockedMemStream.Object);
            mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
            mock.Setup (doc => doc.GetContentStream ()).Returns (mockedStream.Object);
            using (FileDownloader downloader = new SimpleFileDownloader()) {
                try {
                    downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                    Assert.Fail ();
                } catch (IOException) {
                }
            }
        }

        [Ignore]
        [Test, Category("Fast")]
        public void DisposeWhileDownloadTest ()
        {
            throw new NotImplementedException();
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            var mockedMemStream = new Mock<MemoryStream> (remoteContent){CallBase = true};
            mockedMemStream.Setup (memstream => memstream.Read (It.IsAny<byte[]> (), It.IsAny<int> (), It.IsAny<int> ())).Callback(() => Thread.Sleep(1)).Returns(1);
            mockedStream.Setup (stream => stream.Length).Returns (remoteLength);
            mockedStream.Setup (stream => stream.Stream).Returns (mockedMemStream.Object);
            mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
            mock.Setup (doc => doc.GetContentStream ()).Returns (mockedStream.Object);
            try {
                using (FileDownloader downloader = new SimpleFileDownloader()) {
                    Action download = delegate{
                        downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                    };
                    new Task(download).Start();
                }
                Assert.Fail();
            } catch (ObjectDisposedException) {
            }
        }

        #region boilerplate

         // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(!disposed) {
                    if(this.localFileStream != null)
                        this.localFileStream.Dispose();
                    if(this.remoteStream != null)
                        this.remoteStream.Dispose();
                    disposed = true;
                }
            }
        }
        #endregion
    }
}

