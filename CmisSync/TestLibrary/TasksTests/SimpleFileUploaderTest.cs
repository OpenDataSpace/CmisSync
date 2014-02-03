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

namespace TestLibrary.TasksTests
{
    [TestFixture]
    public class SimpleFileUploaderTest : IDisposable
    {
        private bool disposed = false;
        private FileTransmissionEvent transmissionEvent;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private long fileLength;
        private MemoryStream remoteStream;
        private byte[] localContent;
        private Mock<IDocument> mockedDocument;
        private Mock<IContentStream> mockedStream;

        [SetUp]
        public void SetUp ()
        {
            transmissionEvent = new FileTransmissionEvent (FileTransmissionType.UPLOAD_NEW_FILE, "testfile");
            fileLength = 1024 * 1024;
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
            mockedStream.Setup (stream => stream.Length).Returns (fileLength);
            mockedStream.Setup (stream => stream.Stream).Returns (remoteStream);
            mockedDocument.Setup (doc => doc.Name).Returns ("test.txt");
        }

        [Test, Category("Fast")]
        public void ConstructorTest ()
        {
            using (new SimpleFileUploader())
                ;
        }

        [Test, Category("Medium")]
        public void NormalUploadTest ()
        {
            mockedDocument.Setup (doc => doc.SetContentStream (It.IsAny<IContentStream> (), It.Is<bool> (b => b == true), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (remoteStream))
                .Returns (new Mock<IObjectId> ().Object);
            using (IFileUploader uploader = new SimpleFileUploader()) {
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
                IDocument result = uploader.UploadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (result, mockedDocument.Object);
                Assert.AreEqual (localContent.Length, remoteStream.Length);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localContent), hashAlg.Hash);
                remoteStream.Seek (0, SeekOrigin.Begin);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteStream), hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        public void IOExceptionTest ()
        {
            mockedDocument.Setup (doc => doc.SetContentStream (It.IsAny<IContentStream> (), It.IsAny<bool> (), It.Is<bool> (b => b == true)))
                .Throws<IOException> ();
            using (IFileUploader uploader = new SimpleFileUploader()) {
                try {
                    uploader.UploadFile (mockedDocument.Object, localFileStream, transmissionEvent, hashAlg);
                    Assert.Fail ();
                } catch (UploadFailedException e) {
                    Assert.IsInstanceOf (typeof(IOException), e.InnerException);
                    Assert.AreEqual (mockedDocument.Object, e.LastSuccessfulDocument);
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

