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
    public class ChunkedFileUploaderTest : IDisposable
    {

        private bool disposed = false;
        private FileTransmissionEvent transmissionEvent;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private readonly long fileLength = 1024 * 1024;
        private MemoryStream remoteStream;
        private byte[] localContent;
        private readonly long ChunkSize = 1024;
        private int lastChunk;
        private Mock<IDocument> mockedDocument;
        private Mock<IContentStream> mockedStream;
        private Mock<IObjectId> returnedObjectId;

        [SetUp]
        public void SetUp ()
        {
            transmissionEvent = new FileTransmissionEvent (FileTransmissionType.UPLOAD_NEW_FILE, "testfile");
            lastChunk = 0;
            localContent = new byte[fileLength];
            if (localFileStream != null)
                localFileStream.Dispose ();
            localFileStream = new MemoryStream (localContent);
            if (hashAlg != null)
                hashAlg.Dispose ();
            hashAlg = new SHA1Managed ();
            using (RandomNumberGenerator random = RandomNumberGenerator.Create()) {
                random.GetBytes (localContent);
            }
            if (remoteStream != null)
                remoteStream.Dispose ();
            remoteStream = new MemoryStream ();
            mockedDocument = new Mock<IDocument> ();
            mockedStream = new Mock<IContentStream> ();
            returnedObjectId = new Mock<IObjectId> ();
            mockedStream.Setup (stream => stream.Length).Returns (fileLength);
            mockedStream.Setup (stream => stream.Stream).Returns (remoteStream);
            mockedDocument.Setup (doc => doc.Name).Returns ("test.txt");
            mockedDocument.Setup (doc => doc.AppendContentStream (It.IsAny<IContentStream> (), It.Is<bool> (b => b == true), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (remoteStream))
                .Returns (returnedObjectId.Object)
                    .Callback (() => lastChunk++);
            mockedDocument.Setup (doc => doc.AppendContentStream (It.IsAny<IContentStream> (), It.Is<bool> (b => b == false), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (remoteStream))
                .Returns (returnedObjectId.Object);

        }

        [Test, Category("Fast")]
        public void ContructorWorksWithValidInput ()
        {
            using (var uploader = new ChunkedUploader()) {
                Assert.Greater (uploader.ChunkSize, 0);
            }
            using (var uploader = new ChunkedUploader(ChunkSize)) {
                Assert.AreEqual (ChunkSize, uploader.ChunkSize);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsWithZeroChunkSize ()
        {
            using (new ChunkedUploader(0));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsWithNegativeChunkSize ()
        {
            using (new ChunkedUploader(-1));
        }

        [Test, Category("Fast")]
        public void NormalUpload ()
        {
            transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                AssertThatProgressFitsMinimumLimits(e, 0, 0, 0);
            };

            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                uploader.UploadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
            }

            AssertThatLocalAndRemoteContentAreEqualToHash();
            Assert.AreEqual (1, lastChunk);

        }

        // Resumes to upload a file half uploaded in the past
        [Test, Category("Fast")]
        public void ResumeUpload ()
        {
            double successfulUploadPart = 0.5;
            int successfulUploaded = (int) (fileLength * successfulUploadPart);
            double minPercent = 100 * successfulUploadPart;
            transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                AssertThatProgressFitsMinimumLimits(e, successfulUploaded, minPercent, successfulUploaded);
            };
            // Copy half of data before start uploading
            InitRemoteChunkWithSize(successfulUploaded);
            hashAlg.TransformBlock (localContent, 0, successfulUploaded, localContent, 0);
            localFileStream.Seek(successfulUploaded, SeekOrigin.Begin);

            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                uploader.UploadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
            }

            AssertThatLocalAndRemoteContentAreEqualToHash();
            Assert.AreEqual (1, lastChunk);

        }

        [Test, Category("Fast")]
        public void ResumeUploadWithUtils ()
        {
            double successfulUploadPart = 0.2;
            int successfulUploaded = (int) (fileLength * successfulUploadPart);
            double minPercent = 100 * successfulUploadPart;
            InitRemoteChunkWithSize (successfulUploaded);
            transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                AssertThatProgressFitsMinimumLimits(e, successfulUploaded, minPercent, successfulUploaded);
            };

            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                ContentTaskUtils.PrepareResume(successfulUploaded, localFileStream, hashAlg);
                uploader.UploadFile(mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
            }

            AssertThatLocalAndRemoteContentAreEqualToHash();
            Assert.AreEqual (1, lastChunk);

        }

        [Test, Category("Fast")]
        public void IOExceptionOnUploadTest ()
        {
            mockedDocument.Setup (doc => doc.AppendContentStream (It.IsAny<IContentStream> (), It.IsAny<bool> (), It.Is<bool> (b => b == true)))
                .Throws (new IOException ());
            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                try {
                    uploader.UploadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
                    Assert.Fail ();
                } catch (Exception e) {
                    Assert.IsInstanceOf (typeof(UploadFailedException), e);
                    Assert.IsInstanceOf (typeof(IOException), e.InnerException);
                    Assert.AreEqual (mockedDocument.Object, ((UploadFailedException)e).LastSuccessfulDocument);
                }
            }
        }

        [Test, Category("Fast")]
        public void NormalUploadReplacesRemoteStreamIfRemoteStreamExists()
        {
            mockedDocument.Setup( doc => doc.ContentStreamId).Returns("StreamId");
            mockedDocument.Setup( doc => doc.DeleteContentStream(It.IsAny<bool>())).Callback(()=> {
                if (remoteStream != null)
                remoteStream.Dispose ();
                remoteStream = new MemoryStream ();
            }).Returns(mockedDocument.Object);

            remoteStream.WriteByte(1);
            transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                AssertThatProgressFitsMinimumLimits(e, 0, 0, 0);
            };

            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                uploader.UploadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
            }

            mockedDocument.Verify( doc => doc.DeleteContentStream(It.IsAny<bool>()), Times.Once());
            AssertThatLocalAndRemoteContentAreEqualToHash();
            Assert.AreEqual (1, lastChunk);

        }

        private void InitRemoteChunkWithSize (int successfulUploaded)
        {
            byte[] buffer = new byte[successfulUploaded];
            localFileStream.Read (buffer, 0, successfulUploaded);
            remoteStream.Write (buffer, 0, successfulUploaded);
            localFileStream.Seek (0, SeekOrigin.Begin);
        }


        private void AssertThatProgressFitsMinimumLimits(TransmissionProgressEventArgs args, long minLength, double minPercent, long minPos)
        {
            // Console.WriteLine(e.ToString());
            if (args.Length != null) {
                Assert.GreaterOrEqual (args.Length, minLength);
                Assert.LessOrEqual (args.Length, localContent.Length);
            }
            if (args.Percent != null) {
                Assert.GreaterOrEqual (args.Percent, minPercent);
                Assert.LessOrEqual (args.Percent, 100);
            }
            if (args.ActualPosition != null) {
                Assert.GreaterOrEqual (args.ActualPosition, minPos);
                Assert.LessOrEqual (args.ActualPosition, localContent.Length);
            }
        }

        private void AssertThatLocalAndRemoteContentAreEqualToHash() {
                Assert.AreEqual (localContent.Length, remoteStream.Length);
                //Assert.AreEqual (localContent, remoteStream.ToArray());
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localContent), hashAlg.Hash);
                remoteStream.Seek (0, SeekOrigin.Begin);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteStream), hashAlg.Hash);
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
                    if (this.remoteStream != null)
                        this.remoteStream.Dispose ();
                    if (hashAlg != null)
                        this.hashAlg.Dispose ();
                    disposed = true;
                }
            }
        }
#endregion
    }
}

