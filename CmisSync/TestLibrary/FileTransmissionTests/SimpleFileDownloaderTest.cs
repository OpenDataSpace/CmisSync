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

namespace TestLibrary.FileTransmissionTests {
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.HashAlgorithm;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class SimpleFileDownloaderTest : IDisposable {
        private bool disposed = false;
        private Transmission transmission;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private long remoteLength;
        private byte[] remoteContent;
        private RandomNumberGenerator random;
        private Mock<IDocument> mockedDocument;
        private Mock<IContentStream> mockedStream;
        private Mock<MemoryStream> mockedMemStream;

        [SetUp]
        public void SetUp() {
            SetUp(1024 * 1024);
        }

        private void SetUp(long length) {
            this.transmission = new Transmission(TransmissionType.DOWNLOAD_NEW_FILE, "testfile");
            this.transmission.AddDefaultConstraints();
            if (this.localFileStream != null) {
                this.localFileStream.Dispose();
            }

            this.localFileStream = new MemoryStream();
            if (this.hashAlg != null) {
                this.hashAlg.Dispose();
            }

            this.hashAlg = new SHA1Managed();
            this.remoteLength = length;
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
        public void NormalDownload() {
            double lastPercent = 0;
            this.transmission.AddPositionConstraint(Is.LessThanOrEqualTo(this.remoteLength));
            this.transmission.AddLengthConstraint(Is.EqualTo(this.remoteLength).Or.EqualTo(0));

            this.transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                var t = sender as Transmission;
                if (e.PropertyName == Utils.NameOf(() => t.Percent)) {
                    Assert.That(t.Percent, Is.Null.Or.GreaterThanOrEqualTo(lastPercent));
                    lastPercent = t.Percent.GetValueOrDefault();
                }
            };

            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmission, this.hashAlg);
                Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
            }
        }

        /// <summary>
        /// downloader will save the checksum to database for every 1M byte download
        /// 1 byte will save the database one time
        /// </summary>
        [Test, Category("Fast")]
        public void DownloadWithOneUpdate() {
            SetUp(1);
            this.hashAlg = new SHA1Reuse();
            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                int count = 0;
                downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmission, this.hashAlg, (byte[] checksum, long length) => { ++count; });
                Assert.AreEqual(1, count);
                Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
            }
        }

        /// <summary>
        /// downloader will save the checksum to database for every 1M byte download
        /// 2M bytes will save the database two times
        /// </summary>
        [Test, Category("Fast")]
        public void DownloadWithTwoUpdate() {
            SetUp(2 * 1024 * 1024);
            this.hashAlg = new SHA1Reuse();
            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                int count = 0;
                downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmission, this.hashAlg, (byte[] checksum, long length) => { ++count; });
                Assert.AreEqual(2, count);
                Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
            }
        }

        /// <summary>
        /// downloader will save the checksum to database for every 1M byte download
        /// 2M + 1 bytes will save the database three times
        /// </summary>
        [Test, Category("Fast")]
        public void DownloadWithThreeUpdate() {
            SetUp(2 * 1024 * 1024 + 1);
            this.hashAlg = new SHA1Reuse();
            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                int count = 0;
                downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmission, this.hashAlg, (byte[] checksum, long length) => { ++count; });
                Assert.AreEqual(3, count);
                Assert.AreEqual(this.remoteContent.Length, this.localFileStream.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteContent), this.hashAlg.Hash);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localFileStream.ToArray()), this.hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        public void ServerFailedException() {
            this.mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws<CmisConnectionException>();
            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                Assert.Throws<CmisConnectionException>(() => downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmission, this.hashAlg));
            }
        }

        [Test, Category("Fast")]
        public void IOExceptionThrownIfIOExceptionOccursOnRead() {
            this.mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws<IOException>();
            using (IFileDownloader downloader = new SimpleFileDownloader()) {
                Assert.Throws<IOException>(() => downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmission, this.hashAlg));
            }
        }

        [Test, Category("Fast")]
        public void DisposeWhileDownload() {
            this.mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(() => Thread.Sleep(1)).Returns(1);
            try {
                Task t;
                using (IFileDownloader downloader = new SimpleFileDownloader()) {
                    t = Task.Factory.StartNew(() => downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmission, this.hashAlg));
                }

                t.Wait();
                Assert.Fail();
            } catch (AggregateException e) {
                Assert.IsInstanceOf(typeof(ObjectDisposedException), e.InnerException);
            }
        }

        [Test, Category("Medium")]
        public void AbortWhileDownload() {
            this.mockedMemStream.Setup(memstream => memstream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(() => Thread.Sleep(1)).Returns(1);
            this.transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                Assert.That((sender as Transmission).Status, Is.Not.EqualTo(TransmissionStatus.FINISHED));
            };

            try {
                Task t;
                IFileDownloader downloader = new SimpleFileDownloader();
                t = Task.Factory.StartNew(() => downloader.DownloadFile(this.mockedDocument.Object, this.localFileStream, this.transmission, this.hashAlg));
                t.Wait(100);
                this.transmission.Abort();
                t.Wait();
                Assert.Fail();
            } catch (AggregateException e) {
                Assert.IsInstanceOf(typeof(AbortException), e.InnerException);
                Assert.That(this.transmission.Status, Is.EqualTo(TransmissionStatus.ABORTED));
                return;
            }

            Assert.Fail();
        }

        #region boilerplate

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose() {
            this.Dispose(true);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing) {
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