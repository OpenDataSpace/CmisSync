//-----------------------------------------------------------------------
// <copyright file="ContentTaskUtilsTest.cs" company="GRAU DATA AG">
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
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.FileTransmission;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ContentTaskUtilsTest
    {
        private readonly long successfulLength = 1024 * 1024;

        [Test, Category("Fast")]
        public void CreateNewChunkedUploader() {
            long chunkSize = 1024;
            var uploader = ContentTaskUtils.CreateUploader(chunkSize);
            Assert.IsTrue(uploader is ChunkedUploader);
            Assert.AreEqual(chunkSize, (uploader as ChunkedUploader).ChunkSize);
        }

        [Test, Category("Fast")]
        public void CreateNewSimpleUploaderWithoutParam() {
            var uploader = ContentTaskUtils.CreateUploader();
            Assert.IsTrue(uploader is SimpleFileUploader);
        }

        [Test, Category("Fast")]
        public void CreateNewSimpleUploaderByPassingNegativeChunkSize()
        {
            var uploader = ContentTaskUtils.CreateUploader(-1);
            Assert.IsTrue(uploader is SimpleFileUploader);
        }

        [Test, Category("Fast")]
        public void CreateNewChunkedDownloader() {
            long chunkSize = 1024;
            var downloader = ContentTaskUtils.CreateDownloader(chunkSize);
            Assert.IsTrue(downloader is ChunkedDownloader);
            Assert.AreEqual(chunkSize, (downloader as ChunkedDownloader).ChunkSize);
        }

        [Test, Category("Fast")]
        public void CreateNewSimpleDownloaderWithoutParam() {
            var downloader = ContentTaskUtils.CreateDownloader();
            Assert.IsTrue(downloader is SimpleFileDownloader);
        }

        [Test, Category("Fast")]
        public void CreateNewSimpleDownloaderByPassingNegativeChunkSize()
        {
            var downloader = ContentTaskUtils.CreateDownloader(-1);
            Assert.IsTrue(downloader is SimpleFileDownloader);
        }

        [Test, Category("Fast")]
        public void PrepareResumeWithExactFittingStream()
        {
            byte[] localContent = new byte[this.successfulLength];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create()) {
                random.GetBytes(localContent);
            }

            byte[] localHash = new SHA1Managed().ComputeHash(localContent);

            using (MemoryStream stream = new MemoryStream(localContent))
            using (HashAlgorithm hashAlg = new SHA1Managed())
            {
                ContentTaskUtils.PrepareResume(this.successfulLength, stream, hashAlg);
                hashAlg.TransformFinalBlock(new byte[0], 0, 0);
                Assert.AreEqual(localHash, hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(IOException))]
        public void PrepareResumeFailsOnIOException()
        {
            var streamMock = new Mock<Stream>();
            streamMock.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new IOException());
            using (HashAlgorithm hashAlg = new SHA1Managed())
            {
                ContentTaskUtils.PrepareResume(this.successfulLength, streamMock.Object, hashAlg);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(IOException))]
        public void PrepareResumeFailsOnTooShortInputStream()
        {
            var streamMock = new Mock<Stream>();
            streamMock.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
            using (HashAlgorithm hashAlg = new SHA1Managed())
            {
                ContentTaskUtils.PrepareResume(this.successfulLength, streamMock.Object, hashAlg);
            }
        }

        [Test, Category("Fast")]
        public void PrepareResumeWithLongerLocalStream()
        {
            byte[] localContent = new byte[this.successfulLength];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create()) {
                random.GetBytes(localContent);
            }

            byte[] localHash = new SHA1Managed().ComputeHash(localContent);

            using (MemoryStream stream = new MemoryStream())
            using (HashAlgorithm hashAlg = new SHA1Managed())
            {
                stream.Write(localContent, 0, (int)this.successfulLength);
                stream.Write(localContent, 0, (int)this.successfulLength);
                stream.Seek(0, SeekOrigin.Begin);
                ContentTaskUtils.PrepareResume(this.successfulLength, stream, hashAlg);
                hashAlg.TransformFinalBlock(new byte[0], 0, 0);
                Assert.AreEqual(localHash, hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        public void PrepareResumeDoesNotChangeHashOnZeroLengthInputStream()
        {
            byte[] localContent = new byte[0];
            byte[] localHash = new SHA1Managed().ComputeHash(localContent);
            using (MemoryStream stream = new MemoryStream(localContent))
            using (HashAlgorithm hashAlg = new SHA1Managed())
            {
                ContentTaskUtils.PrepareResume(0, stream, hashAlg);
                hashAlg.TransformFinalBlock(new byte[0], 0, 0);
                Assert.AreEqual(localHash, hashAlg.Hash);
            }
        }
    }
}