//-----------------------------------------------------------------------
// <copyright file="ChunkedFileUploaderTest.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
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
    public class ChunkedFileUploaderTest : IDisposable
    {
        private readonly long fileLength = 1024 * 1024;
        private readonly long chunkSize = 1024;
        private bool disposed = false;
        private FileTransmissionEvent transmissionEvent;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private MemoryStream remoteStream;
        private byte[] localContent;
        private int lastChunk;
        private Mock<IDocument> mockedDocument;
        private Mock<IContentStream> mockedStream;
        private Mock<IObjectId> returnedObjectId;

        [SetUp]
        public void SetUp()
        {
            this.transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, "testfile");
            this.lastChunk = 0;
            this.localContent = new byte[this.fileLength];
            if (this.localFileStream != null) {
                this.localFileStream.Dispose();
            }

            this.localFileStream = new MemoryStream(this.localContent);
            if (this.hashAlg != null) {
                this.hashAlg.Dispose();
            }

            this.hashAlg = new SHA1Managed();
            using (RandomNumberGenerator random = RandomNumberGenerator.Create()) {
                random.GetBytes(this.localContent);
            }

            if (this.remoteStream != null) {
                this.remoteStream.Dispose();
            }

            this.remoteStream = new MemoryStream();
            this.mockedDocument = new Mock<IDocument>();
            this.mockedStream = new Mock<IContentStream>();
            this.returnedObjectId = new Mock<IObjectId>();
            this.mockedStream.Setup(stream => stream.Length).Returns(this.fileLength);
            this.mockedStream.Setup(stream => stream.Stream).Returns(this.remoteStream);
            this.mockedDocument.Setup(doc => doc.Name).Returns("test.txt");
            this.mockedDocument.Setup(doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b == true), It.Is<bool>(b => b == true)))
                .Callback<IContentStream, bool, bool>((s, b, r) => s.Stream.CopyTo(this.remoteStream))
                    .Returns(this.returnedObjectId.Object)
                    .Callback(() => this.lastChunk++);
            this.mockedDocument.Setup(doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b == false), It.Is<bool>(b => b == true)))
                .Callback<IContentStream, bool, bool>((s, b, r) => s.Stream.CopyTo(this.remoteStream))
                    .Returns(this.returnedObjectId.Object);
        }

        [Test, Category("Fast")]
        public void ContructorWorksWithValidInput()
        {
            using (var uploader = new ChunkedUploader()) {
                Assert.Greater(uploader.ChunkSize, 0);
            }

            using (var uploader = new ChunkedUploader(this.chunkSize)) {
                Assert.AreEqual(this.chunkSize, uploader.ChunkSize);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsWithZeroChunkSize()
        {
            using (new ChunkedUploader(0))
            {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsWithNegativeChunkSize()
        {
            using (new ChunkedUploader(-1))
            {
            }
        }

        [Test, Category("Fast")]
        public void NormalUpload()
        {
            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                this.AssertThatProgressFitsMinimumLimits(e, 0, 0, 0);
            };

            using (IFileUploader uploader = new ChunkedUploader(this.chunkSize)) {
                uploader.UploadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
            }

            this.AssertThatLocalAndRemoteContentAreEqualToHash();
            Assert.AreEqual(1, this.lastChunk);
        }

        // Resumes to upload a file half uploaded in the past
        [Test, Category("Fast")]
        public void ResumeUpload()
        {
            double successfulUploadPart = 0.5;
            int successfulUploaded = (int)(this.fileLength * successfulUploadPart);
            double minPercent = 100 * successfulUploadPart;
            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                this.AssertThatProgressFitsMinimumLimits(e, successfulUploaded, minPercent, successfulUploaded);
            };

            // Copy half of data before start uploading
            this.InitRemoteChunkWithSize(successfulUploaded);
            this.hashAlg.TransformBlock(this.localContent, 0, successfulUploaded, this.localContent, 0);
            this.localFileStream.Seek(successfulUploaded, SeekOrigin.Begin);

            using (IFileUploader uploader = new ChunkedUploader(this.chunkSize)) {
                uploader.UploadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
            }

            this.AssertThatLocalAndRemoteContentAreEqualToHash();
            Assert.AreEqual(1, this.lastChunk);
        }

        [Test, Category("Fast")]
        public void ResumeUploadWithUtils()
        {
            double successfulUploadPart = 0.2;
            int successfulUploaded = (int)(this.fileLength * successfulUploadPart);
            double minPercent = 100 * successfulUploadPart;
            this.InitRemoteChunkWithSize(successfulUploaded);
            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                this.AssertThatProgressFitsMinimumLimits(e, successfulUploaded, minPercent, successfulUploaded);
            };

            using (IFileUploader uploader = new ChunkedUploader(this.chunkSize)) {
                ContentTaskUtils.PrepareResume(successfulUploaded, this.localFileStream, this.hashAlg);
                uploader.UploadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
            }

            this.AssertThatLocalAndRemoteContentAreEqualToHash();
            Assert.AreEqual(1, this.lastChunk);
        }

        [Test, Category("Fast")]
        public void IOExceptionOnUploadTest()
        {
            this.mockedDocument.Setup(doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.Is<bool>(b => b == true)))
                .Throws(new IOException());
            using (IFileUploader uploader = new ChunkedUploader(this.chunkSize)) {
                try {
                    uploader.UploadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.IsInstanceOf(typeof(UploadFailedException), e);
                    Assert.IsInstanceOf(typeof(IOException), e.InnerException);
                    Assert.AreEqual(this.mockedDocument.Object, ((UploadFailedException)e).LastSuccessfulDocument);
                }
            }
        }

        [Test, Category("Fast")]
        public void NormalUploadReplacesRemoteStreamIfRemoteStreamExists()
        {
            this.mockedDocument.Setup(doc => doc.ContentStreamId).Returns("StreamId");
            this.mockedDocument.Setup(doc => doc.DeleteContentStream(It.IsAny<bool>())).Callback(() => {
                if (this.remoteStream != null) {
                    this.remoteStream.Dispose();
                }

                this.remoteStream = new MemoryStream();
            }).Returns(this.mockedDocument.Object);

            this.remoteStream.WriteByte(1);
            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                this.AssertThatProgressFitsMinimumLimits(e, 0, 0, 0);
            };

            using (IFileUploader uploader = new ChunkedUploader(this.chunkSize)) {
                uploader.UploadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
            }

            this.mockedDocument.Verify(doc => doc.DeleteContentStream(It.IsAny<bool>()), Times.Once());
            this.AssertThatLocalAndRemoteContentAreEqualToHash();
            Assert.AreEqual(1, this.lastChunk);
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

                    if (this.remoteStream != null) {
                        this.remoteStream.Dispose();
                    }

                    if (this.hashAlg != null) {
                        this.hashAlg.Dispose();
                    }

                    this.disposed = true;
                }
            }
        }
        #endregion

        private void InitRemoteChunkWithSize(int successfulUploaded)
        {
            byte[] buffer = new byte[successfulUploaded];
            this.localFileStream.Read(buffer, 0, successfulUploaded);
            this.remoteStream.Write(buffer, 0, successfulUploaded);
            this.localFileStream.Seek(0, SeekOrigin.Begin);
        }

        private void AssertThatProgressFitsMinimumLimits(TransmissionProgressEventArgs args, long minLength, double minPercent, long minPos)
        {
            // Console.WriteLine(e.ToString());
            if (args.Length != null) {
                Assert.GreaterOrEqual(args.Length, minLength);
                Assert.LessOrEqual(args.Length, this.localContent.Length);
            }

            if (args.Percent != null) {
                Assert.GreaterOrEqual(args.Percent, minPercent);
                Assert.LessOrEqual(args.Percent, 100);
            }

            if (args.ActualPosition != null) {
                Assert.GreaterOrEqual(args.ActualPosition, minPos);
                Assert.LessOrEqual(args.ActualPosition, this.localContent.Length);
            }
        }

        private void AssertThatLocalAndRemoteContentAreEqualToHash() {
                Assert.AreEqual(this.localContent.Length, this.remoteStream.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localContent), this.hashAlg.Hash);
                this.remoteStream.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.remoteStream), this.hashAlg.Hash);
        }
    }
}
