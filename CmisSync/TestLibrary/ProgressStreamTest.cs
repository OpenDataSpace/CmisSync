using System;
using System.IO;

using CmisSync.Lib;
using CmisSync.Lib.Events;
using CmisSync.Lib.Streams;

using NUnit.Framework;

namespace TestLibrary
{
    using Moq;
    [TestFixture]
    public class ProgressStreamTest
    {
        private int LengthCalls;
        private int PositionCalls;
        private long Length;
        private readonly string Filename = "filename";
        private readonly FileTransmissionType TransmissionType = FileTransmissionType.DOWNLOAD_NEW_FILE;
        private long Position;
        private double Percent;

        [SetUp]
        public void Setup ()
        {
            LengthCalls = 0;
            PositionCalls = 0;
            Length = 0;
            Position = 0;
            Percent = 0;
        }

        [Test, Category("Fast")]
        public void ConstructorTest ()
        {
            var mockedStream = new Mock<Stream> ();
            var mockedEvent = new Mock<FileTransmissionEvent> (TransmissionType, Filename);
            using (new ProgressStream(mockedStream.Object, mockedEvent.Object)) {
            }
            try {
                using (new ProgressStream(null, null)) {
                }
                Assert.Fail ();
            } catch (ArgumentNullException) {
            }
            try {
                using (new ProgressStream(mockedStream.Object, null)) {
                }
                Assert.Fail ();
            } catch (ArgumentNullException) {
            }
            try {
                using (new ProgressStream(null, mockedEvent.Object)) {
                }
                Assert.Fail ();
            } catch (ArgumentNullException) {
            }
        }

        [Test, Category("Fast")]
        public void SetLengthTest ()
        {
            var mockedStream = new Mock<Stream> ();
            FileTransmissionEvent TransmissionEvent = new FileTransmissionEvent (TransmissionType, Filename);
            TransmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs args) {
                if (args.Length != null)
                    LengthCalls++;
            };
            mockedStream.Setup (s => s.SetLength (It.IsAny<long> ()));
            using (ProgressStream progress = new ProgressStream(mockedStream.Object, TransmissionEvent)) {
                progress.SetLength (100);
                progress.SetLength (100);
                Assert.AreEqual (1, LengthCalls);
            }
        }

        [Test, Category("Fast")]
        public void PositionTest ()
        {
            var mockedStream = new Mock<Stream> ();
            FileTransmissionEvent TransmissionEvent = new FileTransmissionEvent (TransmissionType, Filename);
            TransmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs args) {
                if (args.Length != null && args.Length != Length) {
                    LengthCalls++;
                    Length = (long)args.Length;
                }
                if (args.ActualPosition != null)
                    PositionCalls++;
            };
            mockedStream.Setup (s => s.SetLength (It.IsAny<long> ()));
            mockedStream.SetupProperty (s => s.Position);
            using (ProgressStream progress = new ProgressStream(mockedStream.Object, TransmissionEvent)) {
                progress.SetLength (100);
                Assert.AreEqual (1, LengthCalls);
                Assert.AreEqual (0, PositionCalls);
                progress.Position = 50;
                progress.Position = 50;
                Assert.AreEqual (1, PositionCalls);
                progress.Position = 55;
                Assert.AreEqual (2, PositionCalls);
                Assert.AreEqual (1, LengthCalls);
            }
        }

        [Test, Category("Fast")]
        public void ReadTest ()
        {
            using (Stream stream = new MemoryStream()) {
                FileTransmissionEvent TransmissionEvent = new FileTransmissionEvent (TransmissionType, Filename);
                TransmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs args) {
                    if (args.ActualPosition != null) {
                        PositionCalls ++;
                        Position = (long)args.ActualPosition;
                        Percent = (double)args.Percent;
                    }
                };
                byte[] buffer = new byte[10];
                using (ProgressStream progress = new ProgressStream(stream, TransmissionEvent)) {
                    progress.SetLength (buffer.Length * 10);
                    progress.Read (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length, Position);
                    Assert.AreEqual (10, Percent);
                    progress.Read (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 2, Position);
                    Assert.AreEqual (20, Percent);
                    progress.Read (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 3, Position);
                    Assert.AreEqual (30, Percent);
                    progress.Read (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 4, Position);
                    Assert.AreEqual (40, Percent);
                    progress.Read (buffer, 0, buffer.Length / 2);
                    Assert.AreEqual (buffer.Length * 4 + buffer.Length / 2, Position);
                    Assert.AreEqual (45, Percent);
                    progress.Read (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 5 + buffer.Length / 2, Position);
                    Assert.AreEqual (55, Percent);
                    progress.SetLength (buffer.Length * 100);
                    progress.Read (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 6 + buffer.Length / 2, Position);
                    Assert.AreEqual (6.5, Percent);
                }
            }
        }

        [Test, Category("Fast")]
        public void WriteTest ()
        {
            using (Stream stream = new MemoryStream()) {
                FileTransmissionEvent TransmissionEvent = new FileTransmissionEvent (TransmissionType, Filename);
                TransmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs args) {
                    if (args.ActualPosition != null) {
                        PositionCalls ++;
                        Position = (long)args.ActualPosition;
                        Percent = (double)args.Percent;
                    }
                };
                byte[] buffer = new byte[10];
                using (ProgressStream progress = new ProgressStream(stream, TransmissionEvent)) {
                    progress.SetLength (buffer.Length * 10);
                    progress.Write (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length, Position);
                    Assert.AreEqual (10, Percent);
                    progress.Write (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 2, Position);
                    Assert.AreEqual (20, Percent);
                    progress.Write (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 3, Position);
                    Assert.AreEqual (30, Percent);
                    progress.Write (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 4, Position);
                    Assert.AreEqual (40, Percent);
                    progress.Write (buffer, 0, buffer.Length / 2);
                    Assert.AreEqual (buffer.Length * 4 + buffer.Length / 2, Position);
                    Assert.AreEqual (45, Percent);
                    progress.Write (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 5 + buffer.Length / 2, Position);
                    Assert.AreEqual (55, Percent);
                    progress.SetLength (buffer.Length * 100);
                    progress.Write (buffer, 0, buffer.Length);
                    Assert.AreEqual (buffer.Length * 6 + buffer.Length / 2, Position);
                    Assert.AreEqual (6.5, Percent);
                }
            }
        }

        [Test, Category("Fast")]
        public void SeekTest ()
        {
            using (Stream stream = new MemoryStream()) {
                FileTransmissionEvent TransmissionEvent = new FileTransmissionEvent (TransmissionType, Filename);
                TransmissionEvent.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs args) {
                    if (args.ActualPosition != null) {
                        PositionCalls ++;
                        Position = (long)args.ActualPosition;
                        Percent = (double)args.Percent;
                    }
                };
                using (ProgressStream progress = new ProgressStream(stream, TransmissionEvent)) {
                    progress.SetLength (100);
                    progress.Seek (10, SeekOrigin.Begin);
                    Assert.AreEqual (10, Position);
                    Assert.AreEqual (10, Percent);
                    progress.Seek (10, SeekOrigin.Current);
                    Assert.AreEqual (20, Position);
                    Assert.AreEqual (20, Percent);
                    progress.Seek (10, SeekOrigin.Current);
                    Assert.AreEqual (30, Position);
                    Assert.AreEqual (30, Percent);
                    progress.Seek (10, SeekOrigin.Current);
                    Assert.AreEqual (40, Position);
                    Assert.AreEqual (40, Percent);
                    progress.Seek (5, SeekOrigin.Current);
                    Assert.AreEqual (45, Position);
                    Assert.AreEqual (45, Percent);
                    progress.Seek (10, SeekOrigin.Current);
                    Assert.AreEqual (55, Position);
                    Assert.AreEqual (55, Percent);
                    progress.SetLength (1000);
                    progress.Seek (10, SeekOrigin.Current);
                    Assert.AreEqual (65, Position);
                    Assert.AreEqual (6.5, Percent);

                    progress.Seek(0, SeekOrigin.End);
                    Assert.AreEqual(100, Percent);
                    Assert.AreEqual(1000, Position);
                }
            }
        }
    }
}

