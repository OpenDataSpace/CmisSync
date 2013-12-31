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


        [SetUp]
        public void SetUp ()
        {
            transmissionEvent = new FileTransmissionEvent (FileTransmissionType.UPLOAD_NEW_FILE, "testfile");
            fileLength = 1024 * 1024;
            ChunkSize = 1024;
            lastChunk = 0;
            localContent = new byte[fileLength];
            if(localFileStream!=null)
                localFileStream.Dispose();
            localFileStream = new MemoryStream (localContent);
            if(hashAlg!=null)
                hashAlg.Dispose();
            hashAlg = new SHA1Managed ();
            using (RandomNumberGenerator random = RandomNumberGenerator.Create()) {
                random.GetBytes(localContent);
            }
            if(remoteStream!=null)
                remoteStream.Dispose();
            remoteStream = new MemoryStream ();
        }

        [Test, Category("Fast")]
        public void ContructorTest() 
        {
            using (var uploader = new ChunkedUploader()) {
                Assert.Greater(uploader.ChunkSize, 0);
            }
            using (var uploader = new ChunkedUploader(ChunkSize)) {
                Assert.AreEqual(ChunkSize, uploader.ChunkSize);
            }
            try{
                using(var uploader = new ChunkedUploader(0));
                Assert.Fail ();
            }catch(ArgumentException){}
            try{
                using(var uploader = new ChunkedUploader(-1));
                Assert.Fail ();
            }catch(ArgumentException){}
        }


        [Test, Category("Fast")]
        public void NormalUploadTest ()
        {
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            var returnedAppendCotentStreamDocument = new Mock<IDocument>();
            mockedStream.Setup (stream => stream.Length)
                .Returns (fileLength);
            mockedStream.Setup (stream => stream.Stream)
                .Returns (remoteStream);
            mock.Setup (doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>()))
                .Callback<IContentStream, bool>((s, b) => s.Stream.CopyTo(remoteStream))
                .Returns (returnedAppendCotentStreamDocument.Object);
            mock.Setup (doc => doc.Name)
                .Returns ("test.txt");
            mock.Setup (doc => doc.ContentStreamId)
                .Returns((string) null);
            returnedAppendCotentStreamDocument
                .Setup (doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b == true)))
                .Callback<IContentStream, bool>((s, b) => s.Stream.CopyTo(remoteStream))
                .Returns (returnedAppendCotentStreamDocument.Object)
                    .Callback(()=>lastChunk++);
            returnedAppendCotentStreamDocument
                .Setup (doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b == false)))
                .Callback<IContentStream, bool>((s, b) => s.Stream.CopyTo(remoteStream))
                .Returns (returnedAppendCotentStreamDocument.Object);

            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                transmissionEvent.TransmissionStatus+= delegate(object sender, TransmissionProgressEventArgs e) {
                    if(e.Length!=null){
                        Assert.GreaterOrEqual(e.Length, 0);
                        Assert.LessOrEqual(e.Length, localContent.Length);
                    }
                    if(e.Percent != null) {
                        Assert.GreaterOrEqual(e.Percent, 0);
                        Assert.LessOrEqual(e.Percent, 100);
                    }
                    if(e.ActualPosition!=null) {
                        Assert.GreaterOrEqual(e.ActualPosition, 0);
                        Assert.LessOrEqual(e.ActualPosition, localContent.Length);
                    }
                };
                uploader.UploadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (localContent.Length, remoteStream.Length);
                //Assert.AreEqual (localContent, remoteStream.ToArray());
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localContent), hashAlg.Hash);
                remoteStream.Seek(0,SeekOrigin.Begin);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteStream), hashAlg.Hash);
                Assert.AreEqual(1, lastChunk);
            }
        }

        // Resumes to upload a file half uploaded in the past
        [Test, Category("Fast")]
        public void ResumeUploadTest ()
        {
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            var returnedAppendCotentStreamDocument = new Mock<IDocument>();
            mockedStream.Setup (stream => stream.Length)
                .Returns (fileLength);
            mockedStream.Setup (stream => stream.Stream)
                .Returns (remoteStream);
            mock.Setup (doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>()))
                .Callback<IContentStream, bool>((s, b) => s.Stream.CopyTo(remoteStream))
                .Returns (returnedAppendCotentStreamDocument.Object);
            mock.Setup (doc => doc.Name)
                .Returns ("test.txt");
            mock.Setup (doc => doc.ContentStreamId)
                .Returns("test");
            returnedAppendCotentStreamDocument
                .Setup (doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b == true)))
                .Callback<IContentStream, bool>((s, b) => s.Stream.CopyTo(remoteStream))
                .Returns (returnedAppendCotentStreamDocument.Object)
                    .Callback(()=>lastChunk++);
            returnedAppendCotentStreamDocument
                .Setup (doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b == false)))
                .Callback<IContentStream, bool>((s, b) => s.Stream.CopyTo(remoteStream))
                .Returns (returnedAppendCotentStreamDocument.Object);

            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                int pos = localContent.Length/2;
                transmissionEvent.TransmissionStatus+= delegate(object sender, TransmissionProgressEventArgs e) {
                    if(e.Length!=null){
                        Assert.GreaterOrEqual(e.Length, pos);
                        Assert.LessOrEqual(e.Length, localContent.Length - pos);
                    }
                    if(e.Percent != null) {
                        Assert.GreaterOrEqual(e.Percent, 50);
                        Assert.LessOrEqual(e.Percent, 100);
                    }
                    if(e.ActualPosition!=null) {
                        Assert.GreaterOrEqual(e.ActualPosition, pos);
                        Assert.LessOrEqual(e.ActualPosition, localContent.Length);
                    }
                };
                // Copy half of data before start uploading
                byte[] buffer = new byte[pos];
                localFileStream.Read(buffer, 0, pos);
                remoteStream.Write(buffer, 0, pos);
                hashAlg.TransformBlock(buffer,0,pos,buffer,0);
                uploader.UploadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (localContent.Length, remoteStream.Length);
                //Assert.AreEqual (localContent, remoteStream.ToArray());
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localContent), hashAlg.Hash);
                remoteStream.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteStream), hashAlg.Hash);
                Assert.AreEqual(1, lastChunk);
            }
        }

        //TODO not yet implemented
        [Ignore]
        [Test, Category("Fast")]
        public void ExceptionTest() {
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            var returnedDocument = new Mock<IDocument>().Object;
            mockedStream.Setup (stream => stream.Length).Returns (fileLength);
            mockedStream.Setup (stream => stream.Stream).Returns (remoteStream);
            mock.Setup (doc => doc.AppendContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b==true)))
                .Throws<IOException>();
            mock.Setup (doc => doc.Name).Returns ("test.txt");
            using (IFileUploader uploader = new ChunkedUploader(ChunkSize)) {
                try {
                    uploader.UploadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                    Assert.Fail();
                }catch(IOException) {
                }
            }
        }

        #region boilerplate

         // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
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
            if(disposing) {
                if(!disposed) {
                    if(this.localFileStream != null)
                        this.localFileStream.Dispose();
                    if(this.remoteStream != null)
                        this.remoteStream.Dispose();
                    if(hashAlg!=null)
                        this.hashAlg.Dispose();
                    disposed = true;
                }
            }
        }
#endregion
    }
}

