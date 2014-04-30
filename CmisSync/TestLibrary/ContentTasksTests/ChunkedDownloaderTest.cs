//-----------------------------------------------------------------------
// <copyright file="ChunkedDownloaderTest.cs" company="GRAU DATA AG">
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
    public class ChunkedDownloaderTest : IDisposable
    {
        private readonly long remoteLength = 1024 * 1024;
        private readonly long chunkSize = 1024;
        private bool disposed = false;
        private FileTransmissionEvent transmissionEvent;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private byte[] remoteContent;
        private string contentStreamId = "dummyID";
        private Mock<IDocument> mock;
        private Mock<IContentStream> mockedStream;

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

            this.hashAlg = new SHA1CryptoServiceProvider();
            this.remoteContent = new byte[this.remoteLength];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(this.remoteContent);
            }

            this.mock = new Mock<IDocument>();
            this.mockedStream = new Mock<IContentStream>();
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInputTest()
        {
            using (var downloader = new ChunkedDownloader(this.chunkSize))
            {
                Assert.AreEqual(this.chunkSize, downloader.ChunkSize);
            }

            using (var downloader = new ChunkedDownloader())
            {
                Assert.Greater(downloader.ChunkSize, 0);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsWithNegativeChunkSize()
        {
            using (new ChunkedDownloader(-1))
            {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsWithZeroChunkSize()
        {
            using (new ChunkedDownloader(0))
            {
            }
        }

        [Test, Category("Fast")]
        public void FullDownloadTest()
        {
            this.mockedStream.Setup(stream => stream.Length).Returns(this.remoteLength);
            using (var memorystream = new MemoryStream(this.remoteContent))
            {
                this.mockedStream.Setup(stream => stream.Stream).Returns(memorystream);
                this.mock.Setup(doc => doc.ContentStreamLength).Returns(this.remoteLength);
                this.mock.Setup(doc => doc.ContentStreamId).Returns(this.contentStreamId);
                this.mock.Setup(doc => doc.GetContentStream(
                    It.Is<string>((string s) => s.Equals(this.contentStreamId)),
                    It.Is<long?>((long? l) => (l == null || l == 0)),
                    It.Is<long?>((long? l) => l != null)))
                    .Returns(this.mockedStream.Object);
                this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e)
                {
                    if (e.ActualPosition != null) {
                        Assert.GreaterOrEqual((long)e.ActualPosition, 0);
                        Assert.LessOrEqual((long)e.ActualPosition, this.remoteLength);
                    }

                    if (e.Percent != null) {
                        Assert.GreaterOrEqual(e.Percent, 0);
                        Assert.LessOrEqual(e.Percent, 100);
                    }

                    if (e.Length != null) {
                        Assert.GreaterOrEqual(e.Length, 0);
                        Assert.LessOrEqual(e.Length, this.remoteLength);
                    }
                };

                using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize)) {
                    downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                    Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
                }
            }
        }

        [Test, Category("Fast")]
        public void ResumeDownloadTest()
        {
            long startPos = this.remoteLength / 2;
            byte[] remoteChunk = new byte[this.remoteLength - startPos];
            for (int i = 0; i < remoteChunk.Length; i++) {
                remoteChunk[i] = this.remoteContent[i + startPos];
            }

            this.localFileStream.Write(this.remoteContent, 0, (int)startPos);
            this.localFileStream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(remoteChunk.Length, this.localFileStream.Length);
            this.mockedStream.Setup(stream => stream.Length).Returns(remoteChunk.Length);
            using (var memorystream = new MemoryStream(remoteChunk))
            {
                this.mockedStream.Setup(stream => stream.Stream).Returns(memorystream);
                this.mock.Setup(doc => doc.ContentStreamLength).Returns(this.remoteLength);
                this.mock.Setup(doc => doc.ContentStreamId).Returns(this.contentStreamId);
                this.mock.Setup(doc => doc.GetContentStream(
                    It.Is<string>((string s) => s.Equals(this.contentStreamId)),
                    It.Is<long?>((long? l) => (l == startPos)),
                    It.Is<long?>((long? l) => l == remoteChunk.Length)))
                    .Returns(this.mockedStream.Object);
                this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                    if (e.ActualPosition != null) {
                        Assert.GreaterOrEqual((long)e.ActualPosition, startPos);
                        Assert.LessOrEqual((long)e.ActualPosition, this.remoteLength);
                    }

                    if (e.Percent != null) {
                        Assert.GreaterOrEqual(e.Percent, 50);
                        Assert.LessOrEqual(e.Percent, 100);
                    }

                    if (e.Length != null) {
                        Assert.GreaterOrEqual(e.Length, startPos);
                        Assert.LessOrEqual(e.Length, this.remoteLength);
                    }
                };

                using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize)) {
                    downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                    Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
                }
            }
        }

        [Test, Category("Fast")]
        public void ResumeDownloadWithUtils()
        {
            long successfulLength = 1024;
            this.localFileStream.Write(this.remoteContent, 0, (int)successfulLength);
            this.localFileStream.Seek(0, SeekOrigin.Begin);

            byte[] remoteChunk = new byte[this.remoteLength - successfulLength];
            for (int i = 0; i < remoteChunk.Length; i++) {
                remoteChunk[i] = this.remoteContent[i + successfulLength];
            }

            this.mockedStream.Setup(stream => stream.Length).Returns(remoteChunk.Length);
            this.mockedStream.Setup(stream => stream.Stream).Returns(new MemoryStream(remoteChunk));
            this.mock.Setup(doc => doc.ContentStreamLength).Returns(this.remoteLength);
            this.mock.Setup(doc => doc.ContentStreamId).Returns(this.contentStreamId);
            this.mock.Setup(doc => doc.GetContentStream(
                It.Is<string>((string s) => s.Equals(this.contentStreamId)),
                It.Is<long?>((long? l) => (l == successfulLength)),
                It.Is<long?>((long? l) => l == remoteChunk.Length)))
                .Returns(this.mockedStream.Object);

            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e)
            {
                if (e.ActualPosition != null) {
                    Assert.GreaterOrEqual((long)e.ActualPosition, successfulLength);
                    Assert.LessOrEqual((long)e.ActualPosition, this.remoteLength);
                }

                if (e.Percent != null) {
                    Assert.Greater(e.Percent, 0);
                    Assert.LessOrEqual(e.Percent, 100);
                }

                if (e.Length != null) {
                    Assert.GreaterOrEqual(e.Length, successfulLength);
                    Assert.LessOrEqual(e.Length, this.remoteLength);
                }
            };

            using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize))
            {
                ContentTaskUtils.PrepareResume(successfulLength, this.localFileStream, this.hashAlg);
                downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        public void FullDownloadWithoutLengthTest()
        {
            this.mockedStream.Setup(stream => stream.Length).Returns((long?)null);
            var mockedMemoryStream = new Mock<MemoryStream>(this.remoteContent) { CallBase = true };
            mockedMemoryStream.Setup(ms => ms.Length).Throws(new NotSupportedException());
            this.mockedStream.Setup(stream => stream.Stream).Returns(mockedMemoryStream.Object);
            this.mock.Setup(doc => doc.ContentStreamLength).Returns(this.remoteLength);
            this.mock.Setup(doc => doc.ContentStreamId).Returns(this.contentStreamId);
            this.mock.Setup(doc => doc.GetContentStream(
                It.Is<string>((string s) => s.Equals(this.contentStreamId)),
                It.Is<long?>((long? l) => (l == null || l == 0)),
                It.Is<long?>((long? l) => l != null)))
                .Returns(this.mockedStream.Object);
            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                if (e.ActualPosition != null) {
                    Assert.GreaterOrEqual((long)e.ActualPosition, 0);
                    Assert.LessOrEqual((long)e.ActualPosition, this.remoteLength);
                }

                if (e.Percent != null) {
                    Assert.IsTrue(e.Percent == 0 || e.Percent == 100);
                }

                if (e.Length != null) {
                    Assert.IsTrue(e.Length == 0 || e.Length == this.remoteContent.Length);
                }
            };

            using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize)) {
                downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
            }
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
            if (disposing)
            {
                if (!this.disposed)
                {
                    if (this.localFileStream != null) {
                        this.localFileStream.Dispose();
                    }

                    if (this.hashAlg != null) {
                        this.hashAlg.Dispose();
                    }

                    this.disposed = true;
                }
            }
        }
        #endregion
    }
}
