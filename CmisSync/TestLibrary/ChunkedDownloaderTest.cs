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

namespace TestLibrary
{
    [TestFixture]
    public class ChunkedDownloaderTest
    {

        private FileTransmissionEvent transmissionEvent;
        private MemoryStream localFileStream;
        private HashAlgorithm hashAlg;
        private long remoteLength;
        private byte[] remoteContent;
        private string ContentStreamId = "dummyID";
        private long chunksize = 1024;
        private RandomNumberGenerator random;

        [SetUp]
        public void SetUp ()
        {
            transmissionEvent = new FileTransmissionEvent (FileTransmissionType.DOWNLOAD_NEW_FILE, "testfile");
            localFileStream = new MemoryStream ();
            hashAlg = new SHA1CryptoServiceProvider ();
            random = RandomNumberGenerator.Create();
            remoteLength = 1024 * 1024;
            remoteContent = new byte[remoteLength];
            random.GetBytes(remoteContent);
        }

        [Test, Category("Fast")]
        public void ConstructorTest() {
            using(ChunkedDownloader downloader = new ChunkedDownloader(chunksize)){
                Assert.AreEqual(chunksize,downloader.ChunkSize);
            }
            using(ChunkedDownloader downloader = new ChunkedDownloader()){
                Assert.Greater(downloader.ChunkSize, 0);
            }
            try{
                using(ChunkedDownloader downloader = new ChunkedDownloader(-1));
                Assert.Fail();
            }catch(ArgumentException){}
            try{
                using(ChunkedDownloader downloader = new ChunkedDownloader(0));
                Assert.Fail();
            }catch(ArgumentException){}

        }



        [Test, Category("Fast")]
        public void FullDownloadTest ()
        {
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            mockedStream.Setup (stream => stream.Length).Returns (remoteLength);
            mockedStream.Setup (stream => stream.Stream).Returns (new MemoryStream(remoteContent));
            mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
            mock.Setup (doc => doc.ContentStreamId).Returns (ContentStreamId);
            mock.Setup (doc => doc.GetContentStream (
                It.Is<string> ((string s) => s.Equals (ContentStreamId)),
                It.Is<long?> ((long? l) => (l == null|| l == 0 )),
                It.Is<long?> ((long? l) => l != null)))
                .Returns (mockedStream.Object);
            using (FileDownloader downloader = new ChunkedDownloader(chunksize)) {
                downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (remoteContent.Length, localFileStream.Length);
//                Assert.AreEqual (remoteContent, localFileStream.ToArray());
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteContent), hashAlg.Hash);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localFileStream.ToArray ()), hashAlg.Hash);
            }
        }


        [Test, Category("Fast")]
        public void ResumeDownloadTest ()
        {
            long startPos = remoteLength / 2;
            byte[] remoteChunk = new byte[remoteLength - startPos];
            for(int i=0; i < remoteChunk.Length; i++)
                remoteChunk[i] = remoteContent[i+startPos];
            localFileStream.Write(remoteContent, 0, (int) startPos);
            localFileStream.Seek(0,SeekOrigin.Begin);
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            mockedStream.Setup (stream => stream.Length).Returns (remoteChunk.Length);
            mockedStream.Setup (stream => stream.Stream).Returns (new MemoryStream(remoteChunk));
            mock.Setup (doc => doc.ContentStreamLength).Returns (remoteLength);
            mock.Setup (doc => doc.ContentStreamId).Returns (ContentStreamId);
            mock.Setup (doc => doc.GetContentStream (
                It.Is<string> ((string s) => s.Equals (ContentStreamId)),
                It.Is<long?> ((long? l) => (l == startPos )),
                It.Is<long?> ((long? l) => l == remoteChunk.Length)))
                .Returns (mockedStream.Object);
            using (FileDownloader downloader = new ChunkedDownloader(chunksize)) {
                downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (remoteContent.Length, localFileStream.Length);
//                Assert.AreEqual (remoteContent, localFileStream.ToArray());
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteContent), hashAlg.Hash);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localFileStream.ToArray ()), hashAlg.Hash);
            }
        }


        // TODO Test is not yet implemented correctly
        [Ignore]
        [Test, Category("Fast")]
        public void FullDownloadWithoutLengthTest ()
        {
            var mock = new Mock<IDocument> ();
            var mockedStream = new Mock<IContentStream> ();
            byte[] remoteChunk = new byte[chunksize];
            for(int i = 0; i < remoteChunk.Length; i++)
                remoteChunk[i] = 1;
            var chunkedMemStream = new MemoryStream(remoteChunk);
            mockedStream.Setup (stream => stream.Length).Returns (remoteChunk.Length);
            mockedStream.Setup (stream => stream.Stream).Returns (chunkedMemStream)
                .Callback(() => chunkedMemStream = new MemoryStream(remoteChunk));
            mock.Setup (doc => doc.ContentStreamLength).Returns (delegate{long? l = null; return l;});
            mock.Setup (doc => doc.ContentStreamId).Returns (ContentStreamId);
            mock.Setup (doc => doc.GetContentStream (
                It.Is<string> ((string s) => s.Equals (ContentStreamId)),
                It.Is<long?> ((long? offset) => (offset != null )),
                It.Is<long?> ((long? l) => (l != null ))
                )).Returns (mockedStream.Object);
            using (FileDownloader downloader = new ChunkedDownloader(chunksize)) {
                downloader.DownloadFile (mock.Object, localFileStream, transmissionEvent, hashAlg);
                Assert.AreEqual (remoteContent.Length, localFileStream.Length);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (remoteContent), hashAlg.Hash);
                Assert.AreEqual (SHA1Managed.Create ().ComputeHash (localFileStream.ToArray ()), hashAlg.Hash);
            }
        }

    }
}

