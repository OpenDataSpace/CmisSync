//-----------------------------------------------------------------------
// <copyright file="SimpleFileDownloaderTest.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace TestLibrary.ContentTasksTests
{
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
        public void SetUp()
        {
            this.transmissionEvent = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, "testfile");
            if (this.localFileStream != null) {
                this.localFileStream.Dispose();
            }

            this.localFileStream = new MemoryStream();
            if (this.hashAlg != null) {
                this.hashAlg.Dispose();
            }

            this.hashAlg = new SHA1Managed();
            this.remoteLength = 1024 * 1024;
            this.remoteContent = new byte[this.remoteLength];
            if (this.random != null) {
                this.random.Dispose();
            }

            this.random = RandomNumberGenerator.Create();
            this.random.GetBytes(this.remoteContent);
            this.mockedMemStream = new Mock<MemoryStream>(this.remoteContent) { CallBase = true };
            this.mockedStream = new Mock<IContentStream>();
            this.mockedStream.Setup(stream => stream.Length).Returns(this.remoteLength);
            this.mockedStream.Setup(stream => stream.Stream).Returns(this.mockedMemStream.Object);
            this.mockedDocument = new Mock<IDocument>();
            this.mockedDocument.Setup(doc => doc.ContentStreamLength).Returns(this.remoteLength);
            this.mockedDocument.Setup(doc => doc.GetContentStream()).Returns(this.mockedStream.Object);
        }

        [Test, Category("Fast")]
        public void NormalDownloadTest()
        {
            double lastPercent = 0;
            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                if (e.ActualPosition != null) {
                    Assert.GreaterOrEqual((long)e.ActualPosition, 0);
                    Assert.LessOrEqual((long)e.ActualPosition, this.remoteLength);
                }

                if (e.Percent != null) {
                    Assert.GreaterOrEqual(e.Percent, 0);
                    Assert.LessOrEqual(e.Percent, 100);
                    Assert.GreaterOrEqual(e.Percent, lastPercent);
                    lastPercent = (double)e.Percent;
                }

                if (e.Length != null) {
                    Assert.GreaterOrEqual(e.Length, 0);
                    Assert.LessOrEqual(e.Length, this.remoteLength);
                }
            };

            using (IFileDownloader downloader = new SimpleFileDownloader())
            {
                downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(CmisConnectionException))]
        public void ServerFailedExceptionTest()
        {
            this.mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws<CmisConnectionException>();
            using (IFileDownloader downloader = new SimpleFileDownloader())
            {
                downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(IOException))]
        public void IOExceptionTest()
        {
            this.mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws<IOException>();
            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
            }
        }

        [Test, Category("Fast")]
        public void DisposeWhileDownloadTest()
        {
            this.mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(() => Thread.Sleep(1)).Returns(1);
            try {
                Task t;
                using (IFileDownloader downloader = new SimpleFileDownloader()) {
                    t = Task.Factory.StartNew(() => downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg));
                }

                t.Wait();
                Assert.Fail();
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf(typeof(ObjectDisposedException), e.InnerException);
            }
        }

        [Test, Category("Fast")]
        public void AbortWhileDownloadTest()
        {
            this.mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(() => Thread.Sleep(1)).Returns(1);
            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e)
            {
                Assert.AreEqual(null, e.Completed);
            };
            try
            {
                Task t;
                IFileDownloader downloader = new SimpleFileDownloader();
                t = Task.Factory.StartNew(() => downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg));
                t.Wait(100);
                this.transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Aborting = true });
                t.Wait();
                Assert.Fail();
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf(typeof(AbortException), e.InnerException);
                Assert.True(this.transmissionEvent.Status.Aborted.GetValueOrDefault());
                Assert.AreEqual(false, this.transmissionEvent.Status.Aborting);
                return;
            }

            Assert.Fail();
        }

        #region boilerplate

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            this.Dispose(true);
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
            if (disposing) {
                if (!this.disposed) {
                    if (this.localFileStream != null) {
                        this.localFileStream.Dispose();
                    }

                    if (this.hashAlg != null) {
                        this.hashAlg.Dispose();
                    }

                    if (this.random != null) {
                        this.random.Dispose();
                    }

                    this.disposed = true;
                }
            }
        }
        #endregion
    }
}
