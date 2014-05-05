using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using CmisSync.Lib;
using CmisSync.Lib.Cmis;
using CmisSync.Lib.ContentTasks;
using CmisSync.Lib.Events;

using DotCMIS.Client;
using DotCMIS.Data;
using DotCMIS.Exceptions;

using Moq;

using NUnit.Framework;

namespace TestLibrary.ContentTasksTests
{
    [TestFixture]
    public class SimpleFileDownloaderTest : IDisposable
    {
        private bool disposed = false;
        private FileTransmissionEvent transmissionEvent;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private long remoteLength;
        private byte[] remoteContent;
        private RandomNumberGenerator random;
        private Mock<IDocument> mockedDocument;
        private Mock<IContentStream> mockedStream;
        private Mock<MemoryStream> mockedMemStream;

        [SetUp]
        public void SetUp ()
        {
            transmissionEvent = new FileTransmissionEvent (FileTransmissionType.DOWNLOAD_NEW_FILE, "testfile");
            if (localFileStream != null)
                localFileStream.Dispose ();
            localFileStream = new MemoryStream ();
            if (hashAlg != null)
                hashAlg.Dispose ();
            hashAlg = new SHA1Managed ();
            remoteLength = 1024 * 1024;
            remoteContent = new byte[remoteLength];
            if (random != null)
                random.Dispose ();
            random = RandomNumberGenerator.Create ();
            random.GetBytes (remoteContent);
            mockedMemStream = new Mock<MemoryStream>(remoteContent) { CallBase = true };
            mockedStream = new Mock<IContentStream> ();
            mockedStream.Setup(stream => stream.Length).Returns(remoteLength);
            mockedStream.Setup(stream => stream.Stream).Returns(mockedMemStream.Object);
            mockedDocument = new Mock<IDocument>();
            mockedDocument.Setup(doc => doc.ContentStreamLength).Returns(remoteLength);
            mockedDocument.Setup(doc => doc.GetContentStream ()).Returns(mockedStream.Object);
        }

        [Test, Category("Fast")]
        public void NormalDownloadTest ()
        {
            double lastPercent = 0;
            transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
//                Console.WriteLine(e);
                if (e.ActualPosition != null) {
                    Assert.GreaterOrEqual ((long)e.ActualPosition, 0);
                    Assert.LessOrEqual ((long)e.ActualPosition, remoteLength);
                }
                if (e.Percent != null) {
                    Assert.GreaterOrEqual (e.Percent, 0);
                    Assert.LessOrEqual (e.Percent, 100);
                    Assert.GreaterOrEqual (e.Percent, lastPercent);
                    lastPercent = (double)e.Percent;
                }
                if (e.Length != null) {
                    Assert.GreaterOrEqual (e.Length, 0);
                    Assert.LessOrEqual (e.Length, remoteLength);
                }
            };
            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                downloader.DownloadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (remoteContent.Length, localFileStream.Length);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteContent), hashAlg.Hash);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localFileStream.ToArray ()), hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        public void ServerFailedExceptionTest ()
        {
            mockedMemStream.Setup (memstream => memstream.Read (It.IsAny<byte[]> (), It.IsAny<int> (), It.IsAny<int> ())).Throws<CmisConnectionException> ();
            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                try {
                    downloader.DownloadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
                    Assert.Fail ();
                } catch (CmisConnectionException) {
                }
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(IOException))]
        public void IOExceptionTest ()
        {
            mockedMemStream.Setup (memstream => memstream.Read (It.IsAny<byte[]> (), It.IsAny<int> (), It.IsAny<int> ())).Throws<IOException> ();
            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                downloader.DownloadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
            }
        }

        [Test, Category("Fast")]
        public void DisposeWhileDownloadTest ()
        {
            mockedMemStream.Setup (memstream => memstream.Read (It.IsAny<byte[]> (), It.IsAny<int> (), It.IsAny<int> ())).Callback (() => Thread.Sleep (1)).Returns (1);
            try {
                Task t;
                using (IFileDownloader downloader = new SimpleFileDownloader()) {
                    t = Task.Factory.StartNew (() => downloader.DownloadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg));
                }
                t.Wait ();
                Assert.Fail ();
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf(typeof(ObjectDisposedException), e.InnerException);
            }
        }

        [Test, Category("Fast")]
        public void AbortWhileDownloadTest()
        {
            //long position = 0;
            //int ret = 0;
            //mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
            //    .Callback((byte[] buffer, int offset, int count) => { Thread.Sleep(1); for (ret = 0; ret < count && position < remoteLength; ++ret, ++position) { buffer[ret] = remoteContent[position]; } })
            //    .Returns((byte[] buffer, int offset, int count) => ret);
            mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(() => Thread.Sleep(1)).Returns(1);
            transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e)
            {
                Assert.AreEqual(null, e.Completed);
            };
            try
            {
                Task t;
                IFileDownloader downloader = new SimpleFileDownloader();
                t = Task.Factory.StartNew(() => downloader.DownloadFile(mockedDocument.Object, localFileStream, transmissionEvent, hashAlg));
                t.Wait(100);
                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Aborting = true });
                t.Wait();
                Assert.Fail();
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf(typeof(AbortException), e.InnerException);
                Assert.True(transmissionEvent.Status.Aborted.GetValueOrDefault());
                Assert.AreEqual(false, transmissionEvent.Status.Aborting);
                return;
            }
            Assert.Fail();
        }

        #region boilerplate

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose ()
        {
            Dispose (true);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose (bool disposing)
        {
            if (disposing) {
                if (!disposed) {
                    if (this.localFileStream != null)
                        this.localFileStream.Dispose ();
                    if (hashAlg != null)
                        this.hashAlg.Dispose ();
                    if (this.random != null)
                        this.random.Dispose ();
                    disposed = true;
                }
            }
        }
        #endregion
    }
}

