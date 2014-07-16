//-----------------------------------------------------------------------
// <copyright file="SimpleFileUploaderTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.FileTransmissionTests
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Events;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

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
        public void SetUp()
        {
            this.transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, "testfile");
            this.fileLength = 1024 * 1024;
            this.localContent = new byte[this.fileLength];
            if (this.localFileStream != null) {
                this.localFileStream.Dispose();
            }

            this.localFileStream = new MemoryStream(this.localContent);
            if (this.hashAlg != null) {
                this.hashAlg.Dispose();
            }

            this.hashAlg = new SHA1Managed();
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(this.localContent);
            }

            this.mockedMemStream = new Mock<MemoryStream>() { CallBase = true };
            this.mockedDocument = new Mock<IDocument>();
            this.mockedStream = new Mock<IContentStream>();
            this.mockedStream.Setup(stream => stream.Length).Returns(this.fileLength);
            this.mockedStream.Setup(stream => stream.Stream).Returns(this.mockedMemStream.Object);
            this.mockedDocument.Setup(doc => doc.Name).Returns("test.txt");
        }

        [Test, Category("Fast")]
        public void ConstructorTest()
        {
            using (new SimpleFileUploader())
            {
            }
        }

        [Test, Category("Medium")]
        public void NormalUploadTest()
        {
            this.mockedDocument.Setup(doc => doc.SetContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b == true), It.Is<bool>(b => b == true)))
                .Callback<IContentStream, bool, bool>((s, b, r) => s.Stream.CopyTo(this.mockedMemStream.Object))
                .Returns(new Mock<IObjectId>().Object);
            using (IFileUploader uploader = new SimpleFileUploader()) {
                this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e) {
                    if (e.Length != null) {
                        Assert.GreaterOrEqual(e.Length, 0);
                        Assert.LessOrEqual(e.Length, this.localContent.Length);
                    }

                    if (e.Percent != null) {
                        Assert.GreaterOrEqual(e.Percent, 0);
                        Assert.LessOrEqual(e.Percent, 100);
                    }

                    if (e.ActualPosition != null) {
                        Assert.GreaterOrEqual(e.ActualPosition, 0);
                        Assert.LessOrEqual(e.ActualPosition, this.localContent.Length);
                    }
                };

                IDocument result = uploader.UploadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                Assert.AreEqual(result, this.mockedDocument.Object);
                Assert.AreEqual(this.localContent.Length, this.mockedMemStream.Object.Length);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.localContent), this.hashAlg.Hash);
                this.mockedMemStream.Object.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(SHA1Managed.Create().ComputeHash(this.mockedMemStream.Object), this.hashAlg.Hash);
            }
        }

        [Test, Category("Fast")]
        public void IOExceptionTest()
        {
            this.mockedDocument.Setup(doc => doc.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.Is<bool>(b => b == true)))
                .Throws<IOException>();
            using (IFileUploader uploader = new SimpleFileUploader()) {
                try {
                    uploader.UploadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg);
                    Assert.Fail();
                } catch (UploadFailedException e) {
                    Assert.IsInstanceOf(typeof(IOException), e.InnerException);
                    Assert.AreEqual(this.mockedDocument.Object, e.LastSuccessfulDocument);
                }
            }
        }

        [Test, Category("Fast")]
        public void AbortTest()
        {
            this.mockedDocument.Setup(doc => doc.SetContentStream(It.IsAny<IContentStream>(), It.Is<bool>(b => b == true), It.Is<bool>(b => b == true)))
                .Callback<IContentStream, bool, bool>((s, b, r) => s.Stream.CopyTo(this.mockedMemStream.Object))
                .Returns(new Mock<IObjectId>().Object);
            this.mockedMemStream.Setup(memstream => memstream.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback(() => Thread.Sleep(100));
            this.transmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs e)
            {
                Assert.AreEqual(null, e.Completed);
            };
            try
            {
                Task t;
                IFileUploader uploader = new SimpleFileUploader();
                t = Task.Factory.StartNew(() => uploader.UploadFile(this.mockedDocument.Object, this.localFileStream, this.transmissionEvent, this.hashAlg));
                t.Wait(10);
                this.transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Aborting = true });
                t.Wait();
                Assert.Fail();
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf(typeof(UploadFailedException), e.InnerException);
                Assert.IsInstanceOf(typeof(AbortException), e.InnerException.InnerException);
                Assert.True(this.transmissionEvent.Status.Aborted.GetValueOrDefault());
                Assert.AreEqual(false, this.transmissionEvent.Status.Aborting);
                return;
            }

            Assert.Fail();
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

                    if (this.hashAlg != null) {
                        this.hashAlg.Dispose();
                    }

                    this.disposed = true;
                }
            }
        }

        #endregion
    }
}
