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
    public class SimpleFileUploaderTest : IDisposable
    {
        private bool disposed = false;
        private FileTransmissionEvent transmissionEvent;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private long fileLength;
        private Mock<MemoryStream> mockedMemStream;
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
            mockedMemStream = new Mock<MemoryStream>() { CallBase = true };
            mockedDocument = new Mock<IDocument> ();
            mockedStream = new Mock<IContentStream> ();
            mockedStream.Setup (stream => stream.Length).Returns (fileLength);
            mockedStream.Setup (stream => stream.Stream).Returns (mockedMemStream.Object);
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
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (mockedMemStream.Object))
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
                Assert.AreEqual (localContent.Length, mockedMemStream.Object.Length);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localContent), hashAlg.Hash);
                mockedMemStream.Object.Seek (0, SeekOrigin.Begin);
                Assert.AreEqual (SHA1Managed.Create().ComputeHash(mockedMemStream.Object), hashAlg.Hash);
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

        [Test, Category("Fast")]
        public void AbortTest()
        {
            mockedDocument.Setup (doc => doc.SetContentStream (It.IsAny<IContentStream> (), It.Is<bool> (b => b == true), It.Is<bool> (b => b == true)))
                .Callback<IContentStream, bool, bool> ((s, b, r) => s.Stream.CopyTo (mockedMemStream.Object))
                .Returns (new Mock<IObjectId>().Object);
            mockedMemStream.Setup (memstream => memstream.Write (It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback (() => Thread.Sleep(100));
            transmissionEvent.TransmissionStatus += delegate (object sender, TransmissionProgressEventArgs e)
            {
                Assert.AreEqual (null, e.Completed);
            };
            try
            {
                Task t;
                IFileUploader uploader = new SimpleFileUploader();
                t = Task.Factory.StartNew (() => uploader.UploadFile(mockedDocument.Object, localFileStream, transmissionEvent, hashAlg));
                t.Wait(10);
                transmissionEvent.ReportProgress (new TransmissionProgressEventArgs() { Aborting = true });
                t.Wait();
                Assert.Fail();
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf (typeof(UploadFailedException), e.InnerException);
                Assert.IsInstanceOf (typeof(AbortException), e.InnerException.InnerException);
                Assert.True (transmissionEvent.Status.Aborted.GetValueOrDefault());
                Assert.AreEqual (false, transmissionEvent.Status.Aborting);
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
                    disposed = true;
                }
            }
        }

        #endregion
    }
}

