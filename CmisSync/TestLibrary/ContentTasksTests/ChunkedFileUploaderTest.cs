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
        private long fileLength;
        private MemoryStream remoteStream;
        private byte[] localContent;
        private long ChunkSize;
        private int lastChunk;
        private Mock<IDocument> mockedDocument;
        private Mock<IContentStream> mockedStream;
        private Mock<IObjectId> returnedObjectId;

        [SetUp]
        public void SetUp ()
        {
            transmissionEvent = new FileTransmissionEvent (FileTransmissionType.UPLOAD_NEW_FILE, "testfile");
            fileLength = 1024 * 1024;
            ChunkSize = 1024;
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
            mockedDocument.Setup (doc => doc.Name).Returns ("test.txt");
            mockedStream = new Mock<IContentStream> ();
            returnedObjectId = new Mock<IObjectId> ();
            mockedStream.Setup (stream => stream.Length).Returns (fileLength);
            mockedStream.Setup (stream => stream.Stream).Returns (remoteStream);
        }

        [Test, Category("Fast")]
        public void ContructorWithValidInputTest ()
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
            using (new ChunkedUploader(0))
                ;
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsWithNegativeChunkSize ()
        {
            using (new ChunkedUploader(-1))
                ;
        }

        [Test, Category("Fast")]
        public void NormalUploadTest ()
        {
            mockedDocument.Setup (doc => doc.AppendContentStream (It.IsAny<IContentStream> (), It.IsAny<bool> (), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (remoteStream))
                .Returns (returnedObjectId.Object);
            mockedDocument.Setup (doc => doc.ContentStreamId)
                .Returns ((string)null);
            mockedDocument.Setup (doc => doc.AppendContentStream (It.IsAny<IContentStream> (), It.Is<bool> (b => b == true), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (remoteStream))
                .Returns (returnedObjectId.Object)
                    .Callback (() => lastChunk++);
            mockedDocument.Setup (doc => doc.AppendContentStream (It.IsAny<IContentStream> (), It.Is<bool> (b => b == false), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (remoteStream))
                .Returns (returnedObjectId.Object);

            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
//                    Console.WriteLine(e.ToString());
                    if (e.Length != null) {
                        Assert.GreaterOrEqual (e.Length, 0);
                        Assert.LessOrEqual (e.Length, localContent.Length);
                    }
                    if (e.Percent != null) {
                        Assert.GreaterOrEqual (e.Percent, 0);
                        Assert.LessOrEqual (e.Percent, 100);
                    }
                    if (e.ActualPosition != null) {
                        Assert.GreaterOrEqual (e.ActualPosition, 0);
                        Assert.LessOrEqual (e.ActualPosition, localContent.Length);
                    }
                };
                uploader.UploadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (localContent.Length, remoteStream.Length);
                //Assert.AreEqual (localContent, remoteStream.ToArray());
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localContent), hashAlg.Hash);
                remoteStream.Seek (0, SeekOrigin.Begin);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteStream), hashAlg.Hash);
                Assert.AreEqual (1, lastChunk);
            }
        }

        // Resumes to upload a file half uploaded in the past
        [Test, Category("Fast")]
        public void ResumeUploadTest ()
        {
            mockedDocument.Setup (doc => doc.AppendContentStream (It.IsAny<IContentStream> (), It.IsAny<bool> (), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (remoteStream))
                .Returns (returnedObjectId.Object);
            mockedDocument.Setup (doc => doc.AppendContentStream (It.IsAny<IContentStream> (), It.Is<bool> (b => b == true), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (remoteStream))
                .Returns (returnedObjectId.Object)
                    .Callback (() => lastChunk++);
            mockedDocument.Setup (doc => doc.AppendContentStream (It.IsAny<IContentStream> (), It.Is<bool> (b => b == false), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (remoteStream))
                .Returns (returnedObjectId.Object);

            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                int pos = localContent.Length / 2;
                transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
//                    Console.WriteLine(e.ToString());
                    if (e.Length != null) {
                        Assert.GreaterOrEqual (e.Length, pos);
                        Assert.LessOrEqual (e.Length, localContent.Length);
                    }
                    if (e.Percent != null) {
                        Assert.GreaterOrEqual (e.Percent, 50);
                        Assert.LessOrEqual (e.Percent, 100);
                    }
                    if (e.ActualPosition != null) {
                        Assert.GreaterOrEqual (e.ActualPosition, pos);
                        Assert.LessOrEqual (e.ActualPosition, localContent.Length);
                    }
                };
                // Copy half of data before start uploading
                byte[] buffer = new byte[pos];
                localFileStream.Read (buffer, 0, pos);
                remoteStream.Write (buffer, 0, pos);
                hashAlg.TransformBlock (buffer, 0, pos, buffer, 0);
                uploader.UploadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (localContent.Length, remoteStream.Length);
                //Assert.AreEqual (localContent, remoteStream.ToArray());
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localContent), hashAlg.Hash);
                remoteStream.Seek (0, SeekOrigin.Begin);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteStream), hashAlg.Hash);
                Assert.AreEqual (1, lastChunk);
            }
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

