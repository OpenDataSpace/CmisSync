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
    public class ChunkedDownloaderTest : IDisposable
    {
        private bool disposed = false;
        private FileTransmissionEvent transmissionEvent;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private readonly long remoteLength = 1024 * 1024;
        private byte[] remoteContent;
        private string ContentStreamId = "dummyID";
        private readonly long chunkSize = 1024;
        private Mock<IDocument> mock;
        private Mock<IContentStream> mockedStream;

        [SetUp]
        public void SetUp ()
        {
            transmissionEvent = new FileTransmissionEvent (FileTransmissionType.DOWNLOAD_NEW_FILE, "testfile");
            if (localFileStream != null)
                localFileStream.Dispose ();
            localFileStream = new MemoryStream ();
            if (hashAlg != null)
                hashAlg.Dispose ();
            hashAlg = new SHA1CryptoServiceProvider ();
            remoteContent = new byte[remoteLength];
            using(var random = RandomNumberGenerator.Create()) {
                random.GetBytes (remoteContent);
            }
            mock = new Mock<IDocument> ();
            mockedStream = new Mock<IContentStream> ();
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInputTest ()
        {
            using (var downloader = new ChunkedDownloader(chunkSize)) {
                Assert.AreEqual (chunkSize, downloader.ChunkSize);
            }
            using (var downloader = new ChunkedDownloader()) {
                Assert.Greater (downloader.ChunkSize, 0);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException (typeof(ArgumentException))]
        public void ConstructorFailsWithNegativeChunkSize ()
        {
            using (new ChunkedDownloader(-1));
        }

        [Test, Category("Fast")]
        [ExpectedException (typeof(ArgumentException))]
        public void ConstructorFailsWithZeroChunkSize ()
        {
            using (new ChunkedDownloader(0));
        }

        [Test, Category("Fast")]
        public void FullDownloadTest ()
        {
            mockedStream.Setup (stream => stream.Length).Returns (remoteLength);
            using(var memorystream = new MemoryStream (remoteContent)){
                mockedStream.Setup (stream => stream.Stream).Returns (memorystream);
                mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
                mock.Setup (doc => doc.ContentStreamId).Returns (ContentStreamId);
                mock.Setup (doc => doc.GetContentStream (
                            It.Is<string> ((string s) => s.Equals (ContentStreamId)),
                            It.Is<long?> ((long? l) => (l == null || l == 0)),
                            It.Is<long?> ((long? l) => l != null))
                        )
                    .Returns (mockedStream.Object);
                transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                    //                Console.WriteLine(e.ToString());
                    if (e.ActualPosition != null) {
                        Assert.GreaterOrEqual ((long)e.ActualPosition, 0);
                        Assert.LessOrEqual ((long)e.ActualPosition, remoteLength);
                    }
                    if (e.Percent != null) {
                        Assert.GreaterOrEqual (e.Percent, 0);
                        Assert.LessOrEqual (e.Percent, 100);
                    }
                    if (e.Length != null) {
                        Assert.GreaterOrEqual (e.Length, 0);
                        Assert.LessOrEqual (e.Length, remoteLength);
                    }

                };
                using (IFileDownloader downloader = new ChunkedDownloader(chunkSize)) {
                    downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                    Assert.AreEqual (remoteContent.Length, localFileStream.Length);
                    //                Assert.AreEqual (remoteContent, localFileStream.ToArray());
                    Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteContent), hashAlg.Hash);
                    Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localFileStream.ToArray ()), hashAlg.Hash);
                }
            }
        }

        [Test, Category("Fast")]
        public void ResumeDownloadTest ()
        {
            long startPos = remoteLength / 2;
            byte[] remoteChunk = new byte[remoteLength - startPos];
            for (int i=0; i < remoteChunk.Length; i++)
                remoteChunk [i] = remoteContent [i + startPos];
            localFileStream.Write (remoteContent, 0, (int)startPos);
            localFileStream.Seek (0, SeekOrigin.Begin);
            Assert.AreEqual (remoteChunk.Length, localFileStream.Length);
            mockedStream.Setup (stream => stream.Length).Returns (remoteChunk.Length);
            using(var memorystream = new MemoryStream (remoteChunk)){
                mockedStream.Setup (stream => stream.Stream).Returns (memorystream);
                mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
                mock.Setup (doc => doc.ContentStreamId).Returns (ContentStreamId);
                mock.Setup (doc => doc.GetContentStream (
                            It.Is<string> ((string s) => s.Equals (ContentStreamId)),
                            It.Is<long?> ((long? l) => (l == startPos)),
                            It.Is<long?> ((long? l) => l == remoteChunk.Length))
                        )
                    .Returns (mockedStream.Object);
                transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                    //                Console.WriteLine(e.ToString());
                    if (e.ActualPosition != null) {
                        Assert.GreaterOrEqual ((long)e.ActualPosition, startPos);
                        Assert.LessOrEqual ((long)e.ActualPosition, remoteLength);
                    }
                    if (e.Percent != null) {
                        Assert.GreaterOrEqual (e.Percent, 50);
                        Assert.LessOrEqual (e.Percent, 100);
                    }
                    if (e.Length != null) {
                        Assert.GreaterOrEqual (e.Length, startPos);
                        Assert.LessOrEqual (e.Length, remoteLength);
                    }

                };
                using (IFileDownloader downloader = new ChunkedDownloader(chunkSize)) {
                    downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                    Assert.AreEqual (remoteContent.Length, localFileStream.Length);
                    //                Assert.AreEqual (remoteContent, localFileStream.ToArray());
                    Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteContent), hashAlg.Hash);
                    Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localFileStream.ToArray ()), hashAlg.Hash);
                }
            }
        }

        [Test, Category("Fast")]
        public void ResumeDownloadWithUtils()
        {
            long successfulLength = 1024;
            localFileStream.Write (remoteContent, 0, (int)successfulLength);
            localFileStream.Seek (0, SeekOrigin.Begin);


            byte[] remoteChunk = new byte[remoteLength - successfulLength];
            for (int i=0; i < remoteChunk.Length; i++)
                remoteChunk [i] = remoteContent [i + successfulLength];

            mockedStream.Setup (stream => stream.Length).Returns (remoteChunk.Length);
            mockedStream.Setup (stream => stream.Stream).Returns (new MemoryStream (remoteChunk));
            mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
            mock.Setup (doc => doc.ContentStreamId).Returns (ContentStreamId);
            mock.Setup (doc => doc.GetContentStream (
                It.Is<string> ((string s) => s.Equals (ContentStreamId)),
                It.Is<long?> ((long? l) => (l == successfulLength)),
                It.Is<long?> ((long? l) => l == remoteChunk.Length))
            )
                .Returns (mockedStream.Object);

            transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
//                Console.WriteLine(e.ToString());
                if (e.ActualPosition != null) {
                    Assert.GreaterOrEqual ((long)e.ActualPosition, successfulLength);
                    Assert.LessOrEqual ((long)e.ActualPosition, remoteLength);
                }
                if (e.Percent != null) {
                    Assert.Greater (e.Percent, 0);
                    Assert.LessOrEqual (e.Percent, 100);
                }
                if (e.Length != null) {
                    Assert.GreaterOrEqual (e.Length, successfulLength);
                    Assert.LessOrEqual (e.Length, remoteLength);
                }
            };

            using (IFileDownloader downloader = new ChunkedDownloader(chunkSize))
            {
                ContentTaskUtils.PrepareResume(successfulLength, localFileStream, hashAlg);
                downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (remoteContent.Length, localFileStream.Length);
//                Assert.AreEqual (remoteContent, localFileStream.ToArray());
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteContent), hashAlg.Hash);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localFileStream.ToArray ()), hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        public void FullDownloadWithoutLengthTest ()
        {
            mockedStream.Setup (stream => stream.Length).Returns ((long?)null);
            var mockedMemoryStream = new Mock<MemoryStream> (remoteContent){CallBase=true};
            mockedMemoryStream.Setup (ms => ms.Length).Throws (new NotSupportedException ());
            mockedStream.Setup (stream => stream.Stream).Returns (mockedMemoryStream.Object);
            mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
            mock.Setup (doc => doc.ContentStreamId).Returns (ContentStreamId);
            mock.Setup (doc => doc.GetContentStream (
                It.Is<string> ((string s) => s.Equals (ContentStreamId)),
                It.Is<long?> ((long? l) => (l == null || l == 0)),
                It.Is<long?> ((long? l) => l != null))
            )
                .Returns (mockedStream.Object);
            transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
//                Console.WriteLine(e.ToString());
                if (e.ActualPosition != null) {
                    Assert.GreaterOrEqual ((long)e.ActualPosition, 0);
                    Assert.LessOrEqual ((long)e.ActualPosition, remoteLength);
                }
                if (e.Percent != null) {
                    Assert.IsTrue (e.Percent == 0 || e.Percent == 100);
                }
                if (e.Length != null) {
                    Assert.IsTrue (e.Length == 0 || e.Length == remoteContent.Length);
                }
            };
            using (IFileDownloader downloader = new ChunkedDownloader(chunkSize)) {
                downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (remoteContent.Length, localFileStream.Length);
//                Assert.AreEqual (remoteContent, localFileStream.ToArray());
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteContent), hashAlg.Hash);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localFileStream.ToArray ()), hashAlg.Hash);
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
                    if (hashAlg != null)
                        hashAlg.Dispose ();
                    disposed = true;
                }
            }
        }
        #endregion
    }
}

