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

namespace TestLibrary.FileTransmissionTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;

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
        private string localFile;
        private HashAlgorithm hashAlg;
        private byte[] remoteContent;
        private byte[] remoteChunk;
        private string contentStreamId = "dummyID";
        private Mock<IDocument> mock;
        private Mock<IContentStream> mockedStream;
        private Mock<IFileTransmissionStorage> mockedStorage;

        [SetUp]
        public void SetUp()
        {
            this.transmissionEvent = new FileTransmissionEvent(TransmissionType.DOWNLOAD_NEW_FILE, "testfile");
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

            this.localFile = Path.GetTempFileName();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(this.localFile))
            {
                File.Delete(this.localFile);
            }
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

        [Test, Category("Medium")]
        public void FullDownloadTest()
        {
            SetupFullDownload();

            using (var memorystream = new MemoryStream(this.remoteContent))
            {
                this.mockedStream.Setup(stream => stream.Stream).Returns(memorystream);

                using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize))
                {
                    downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                    Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
                }
            }
        }

        [Test, Category("Medium")]
        public void FullDownloadWithoutLengthTest()
        {
            SetupFullDownload();

            this.mockedStream.Setup(stream => stream.Length).Returns((long?)null);
            var mockedMemoryStream = new Mock<MemoryStream>(this.remoteContent) { CallBase = true };
            mockedMemoryStream.Setup(ms => ms.Length).Throws(new NotSupportedException());
            this.mockedStream.Setup(stream => stream.Stream).Returns(mockedMemoryStream.Object);

            using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize))
            {
                downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
            }
        }

        [Test, Category("Medium")]
        public void ResumeDownloadTest()
        {
            long startPos = this.remoteLength / 2;
            SetupResumeDownload(startPos);

            using (var memorystream = new MemoryStream(this.remoteChunk))
            {
                this.mockedStream.Setup(stream => stream.Stream).Returns(memorystream);

                using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize))
                {
                    downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                    Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
                }
            }
        }

        [Ignore]
        [Test, Category("Medium")]
        public void ResumeDownloadByRightStorage()
        {
            long startPos = this.remoteLength / 2;
            SetupResumeDownload(startPos);
            SetupStorage(startPos);

            using (var memorystream = new MemoryStream(this.remoteChunk))
            {
                this.mockedStream.Setup(stream => stream.Stream).Returns(memorystream);

                using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize, this.mockedStorage.Object))
                {
                    downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                    Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
                }
            }
        }

        [Ignore]
        [Test, Category("Medium")]
        public void ResumeDownloadByWrongStorage()
        {
            long startPos = this.remoteLength / 2;
            SetupFullDownload();
            SetupStorage(startPos);

            this.localFileStream.Write(this.remoteContent, 0, (int)startPos);
            this.localFileStream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(this.remoteChunk.Length, this.remoteLength - this.localFileStream.Length);

            byte[] checksum = this.mockedStorage.Object.GetObjectList()[0].LastChecksum;
            checksum[0] = (byte)(checksum[0] + 1);

            using (var memorystream = new MemoryStream(this.remoteChunk))
            {
                this.mockedStream.Setup(stream => stream.Stream).Returns(memorystream);

                using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize, this.mockedStorage.Object))
                {
                    downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                    Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
                }
            }
        }

        [Test, Category("Medium")]
        public void ResumeDownloadWithUtils()
        {
            long startPos = this.remoteLength / 2;
            SetupResumeDownload(startPos);

            using (var memorystream = new MemoryStream(this.remoteChunk))
            {
                this.mockedStream.Setup(stream => stream.Stream).Returns(memorystream);

                using (IFileDownloader downloader = new ChunkedDownloader(this.chunkSize))
                {
                    ContentTaskUtils.PrepareResume(startPos, this.localFileStream, this.hashAlg);
                    downloader.DownloadFile(this.mock.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                    Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                    Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
                }
            }
        }

        [Ignore]
        [Test, Category("Medium")]
        public void ResumeDownloadWithUtilsByRightStorage()
        {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Medium")]
        public void ResumeDownloadWithUtilsByWrongStorage()
        {
            Assert.Fail("TODO");
        }

        private void SetupFullDownload()
        {
            this.mockedStream.Setup(stream => stream.Length).Returns(this.remoteLength);

            this.mock.Setup(doc => doc.ContentStreamLength).Returns(this.remoteLength);
            this.mock.Setup(doc => doc.ContentStreamId).Returns(this.contentStreamId);
            this.mock.Setup(doc => doc.GetContentStream(
                It.Is<string>((string s) => s.Equals(this.contentStreamId)),
                It.Is<long?>((long? l) => (l == null || l == 0)),
                It.Is<long?>((long? l) => l != null)))
                .Returns(this.mockedStream.Object);

            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e)
            {
                if (e.ActualPosition != null)
                {
                    Assert.GreaterOrEqual((long)e.ActualPosition, 0);
                    Assert.LessOrEqual((long)e.ActualPosition, this.remoteLength);
                }

                if (e.Percent != null)
                {
                    Assert.GreaterOrEqual(e.Percent, 0);
                    Assert.LessOrEqual(e.Percent, 100);
                }

                if (e.Length != null)
                {
                    Assert.GreaterOrEqual(e.Length, 0);
                    Assert.LessOrEqual(e.Length, this.remoteLength);
                }
            };
        }

        private void SetupResumeDownload(long startPos)
        {
            this.remoteChunk = new byte[this.remoteLength - startPos];
            for (int i = 0; i < this.remoteChunk.Length; i++)
            {
                this.remoteChunk[i] = this.remoteContent[i + startPos];
            }

            this.localFileStream.Write(this.remoteContent, 0, (int)startPos);
            this.localFileStream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(this.remoteChunk.Length, this.remoteLength - this.localFileStream.Length);

            this.mockedStream.Setup(stream => stream.Length).Returns(this.remoteChunk.Length);

            this.mock.Setup(doc => doc.ContentStreamLength).Returns(this.remoteLength);
            this.mock.Setup(doc => doc.ContentStreamId).Returns(this.contentStreamId);
            this.mock.Setup(doc => doc.GetContentStream(
                It.Is<string>((string s) => s.Equals(this.contentStreamId)),
                It.Is<long?>((long? l) => (l == startPos)),
                It.Is<long?>((long? l) => l == this.remoteChunk.Length)))
                .Returns(this.mockedStream.Object);

            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e)
            {
                if (e.ActualPosition != null)
                {
                    Assert.GreaterOrEqual((long)e.ActualPosition, startPos);
                    Assert.LessOrEqual((long)e.ActualPosition, this.remoteLength);
                }

                if (e.Percent != null)
                {
                    Assert.GreaterOrEqual(e.Percent, startPos * 100 / this.remoteLength);
                    Assert.LessOrEqual(e.Percent, 100);
                }

                if (e.Length != null)
                {
                    Assert.GreaterOrEqual(e.Length, startPos);
                    Assert.LessOrEqual(e.Length, this.remoteLength);
                }
            };
        }

        private void SetupStorage(long startPos)
        {
            mock.Setup(m => m.Id).Returns("RemoteObjectId");
            mock.Setup(m => m.ChangeToken).Returns("ChangeToken");

            Mock<IFileTransmissionObject> obj = new Mock<IFileTransmissionObject>();
            obj.Setup(m => m.Type).Returns(TransmissionType.DOWNLOAD_NEW_FILE);
            obj.Setup(m => m.LocalPath).Returns(this.localFile);
            byte[] checksum = SHA1Managed.Create().ComputeHash(this.remoteContent, 0, (int)startPos);
            obj.Setup(m => m.LastChecksum).Returns(checksum);
            obj.Setup(m => m.ChecksumAlgorithmName).Returns("SHA-1");
            obj.Setup(m => m.RemoteObjectId).Returns("RemoteObjectId");
            obj.Setup(m => m.LastChangeToken).Returns("ChangeToken");

            using (FileStream stream = File.OpenWrite(this.localFile))
            {
                stream.Write(this.remoteContent, 0, (int)startPos);
            }

            this.mockedStorage = new Mock<IFileTransmissionStorage>();
            this.mockedStorage.Setup(m => m.GetObjectList()).Returns(new List<IFileTransmissionObject>() { obj.Object });
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
