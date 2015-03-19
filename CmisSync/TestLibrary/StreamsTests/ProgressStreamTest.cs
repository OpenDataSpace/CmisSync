//-----------------------------------------------------------------------
// <copyright file="ProgressStreamTest.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
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

namespace TestLibrary.StreamsTests {
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using CmisSync.Lib;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Streams;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class ProgressStreamTest {
        private readonly string filename = "filename";
        private readonly TransmissionType transmissionType = TransmissionType.DOWNLOAD_NEW_FILE;

        private int lengthCalls;
        private int positionCalls;
        private long length;
        private long position;
        private double percent;

        [SetUp]
        public void Setup() {
            this.lengthCalls = 0;
            this.positionCalls = 0;
            this.length = 0;
            this.position = 0;
            this.percent = 0;
        }

        [Test, Category("Fast"), Category("Streams"), TestCaseSource("GetAllTypes")]
        public void ConstructorWorksWithNonNullParams(TransmissionType type) {
            using (new ProgressStream(new Mock<Stream>().Object, new Mock<Transmission>(type, this.filename, null).Object));
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnAllParameterNull() {
            using (new ProgressStream(null, null)) {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnStreamIsNull() {
            using (new ProgressStream(null, new Mock<Transmission>(this.transmissionType, this.filename, null).Object)) {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnTransmissionEventIsNull() {
            using (new ProgressStream(new Mock<Stream>().Object, null)) {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void SetLength() {
            var mockedStream = new Mock<Stream>();
            Transmission transmission = new Transmission(this.transmissionType, this.filename).AddDefaultConstraints();
            mockedStream.Setup(s => s.SetLength(It.IsAny<long>()));
            using (ProgressStream progress = new ProgressStream(mockedStream.Object, transmission)) {
                transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs args) {
                    if (args.PropertyName == Utils.NameOf((Transmission t) => t.Length)) {
                        this.lengthCalls++;
                    }
                };
                progress.SetLength(100);
                progress.SetLength(100);
                Assert.AreEqual(1, this.lengthCalls);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void Position() {
            var mockedStream = new Mock<Stream>();
            var transmission = new Transmission(this.transmissionType, this.filename).AddDefaultConstraints();
            mockedStream.Setup(s => s.SetLength(It.IsAny<long>()));
            mockedStream.SetupProperty(s => s.Position);
            using (var progress = new ProgressStream(mockedStream.Object, transmission)) {
                transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                    var t = sender as Transmission;
                    if (e.PropertyName == Utils.NameOf(() => t.Length)) {
                        this.lengthCalls++;
                    }

                    if (e.PropertyName == Utils.NameOf(() => t.Position)) {
                        this.positionCalls++;
                    }
                };
                progress.SetLength(100);
                Assert.AreEqual(1, this.lengthCalls);
                Assert.AreEqual(0, this.positionCalls);
                progress.Position = 50;
                progress.Position = 50;
                Assert.AreEqual(1, this.positionCalls);
                progress.Position = 55;
                Assert.AreEqual(2, this.positionCalls);
                Assert.AreEqual(1, this.lengthCalls);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void Read() {
            using (var stream = new MemoryStream()) {
                var transmission = new Transmission(this.transmissionType, this.filename).AddDefaultConstraints();
                transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                    var t = sender as Transmission;
                    if (e.PropertyName == Utils.NameOf(() => t.Position)) {
                        this.positionCalls++;
                        this.position = (long)t.Position;
                    }
                };
                byte[] buffer = new byte[10];
                using (var progress = new ProgressStream(stream, transmission)) {
                    progress.SetLength(buffer.Length * 10);
                    progress.Read(buffer, 0, buffer.Length);
                    Assert.AreEqual(buffer.Length, this.position);
                    Assert.AreEqual(10, transmission.Percent);
                    progress.Read(buffer, 0, buffer.Length);
                    Assert.AreEqual(buffer.Length * 2, this.position);
                    Assert.AreEqual(20, transmission.Percent);
                    progress.Read(buffer, 0, buffer.Length);
                    Assert.AreEqual(buffer.Length * 3, this.position);
                    Assert.AreEqual(30, transmission.Percent);
                    progress.Read(buffer, 0, buffer.Length);
                    Assert.AreEqual(buffer.Length * 4, this.position);
                    Assert.AreEqual(40, transmission.Percent);
                    progress.Read(buffer, 0, buffer.Length / 2);
                    Assert.AreEqual((buffer.Length * 4) + (buffer.Length / 2), this.position);
                    Assert.AreEqual(45, transmission.Percent);
                    progress.Read(buffer, 0, buffer.Length);
                    Assert.AreEqual((buffer.Length * 5) + (buffer.Length / 2), this.position);
                    Assert.AreEqual(55, transmission.Percent);
                    progress.SetLength(buffer.Length * 100);
                    progress.Read(buffer, 0, buffer.Length);
                    Assert.AreEqual((buffer.Length * 6) + (buffer.Length / 2), this.position);
                    Assert.AreEqual(6.5, transmission.Percent);
                }
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void Write() {
            using (var stream = new MemoryStream()) {
                var transmission = new Transmission(this.transmissionType, this.filename).AddDefaultConstraints();
                transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                    var t = sender as Transmission;
                    if (e.PropertyName == Utils.NameOf(() => t.Position)) {
                        this.positionCalls++;
                        this.position = (long)t.Position;
                        this.percent = (double)t.Percent.GetValueOrDefault();
                    }
                };
                byte[] buffer = new byte[10];
                using (var progress = new ProgressStream(stream, transmission)) {
                    progress.SetLength(buffer.Length * 10);
                    progress.Write(buffer, 0, buffer.Length);
                    Assert.AreEqual(buffer.Length, this.position);
                    Assert.AreEqual(10, this.percent);
                    progress.Write(buffer, 0, buffer.Length);
                    Assert.AreEqual(buffer.Length * 2, this.position);
                    Assert.AreEqual(20, this.percent);
                    progress.Write(buffer, 0, buffer.Length);
                    Assert.AreEqual(buffer.Length * 3, this.position);
                    Assert.AreEqual(30, this.percent);
                    progress.Write(buffer, 0, buffer.Length);
                    Assert.AreEqual(buffer.Length * 4, this.position);
                    Assert.AreEqual(40, this.percent);
                    progress.Write(buffer, 0, buffer.Length / 2);
                    Assert.AreEqual((buffer.Length * 4) + (buffer.Length / 2), this.position);
                    Assert.AreEqual(45, this.percent);
                    progress.Write(buffer, 0, buffer.Length);
                    Assert.AreEqual((buffer.Length * 5) + (buffer.Length / 2), this.position);
                    Assert.AreEqual(55, this.percent);
                    progress.SetLength(buffer.Length * 100);
                    progress.Write(buffer, 0, buffer.Length);
                    Assert.AreEqual((buffer.Length * 6) + (buffer.Length / 2), this.position);
                    Assert.AreEqual(6.5, this.percent);
                }
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void SeekTest() {
            using (var stream = new MemoryStream()) {
                var transmission = new Transmission(this.transmissionType, this.filename);
                transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                    var t = sender as Transmission;
                    if (e.PropertyName == Utils.NameOf(() => t.Position)) {
                        this.positionCalls++;
                        this.position = (long)t.Position;
                        this.percent = (double)t.Percent;
                    }
                };
                using (var progress = new ProgressStream(stream, transmission)) {
                    progress.SetLength(100);
                    progress.Seek(10, SeekOrigin.Begin);
                    Assert.AreEqual(10, this.position);
                    Assert.AreEqual(10, this.percent);
                    progress.Seek(10, SeekOrigin.Current);
                    Assert.AreEqual(20, this.position);
                    Assert.AreEqual(20, this.percent);
                    progress.Seek(10, SeekOrigin.Current);
                    Assert.AreEqual(30, this.position);
                    Assert.AreEqual(30, this.percent);
                    progress.Seek(10, SeekOrigin.Current);
                    Assert.AreEqual(40, this.position);
                    Assert.AreEqual(40, this.percent);
                    progress.Seek(5, SeekOrigin.Current);
                    Assert.AreEqual(45, this.position);
                    Assert.AreEqual(45, this.percent);
                    progress.Seek(10, SeekOrigin.Current);
                    Assert.AreEqual(55, this.position);
                    Assert.AreEqual(55, this.percent);
                    progress.SetLength(1000);
                    progress.Seek(10, SeekOrigin.Current);
                    Assert.AreEqual(65, this.position);
                    Assert.AreEqual(6.5, this.percent);

                    progress.Seek(0, SeekOrigin.End);
                    Assert.AreEqual(100, this.percent);
                    Assert.AreEqual(1000, this.position);
                }
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ResumeTest() {
            byte[] inputContent = new byte[100];
            long offset = 100;
            using (var stream = new MemoryStream(inputContent)) 
            using (var offsetstream = new OffsetStream(stream, offset)) {
                var transmission = new Transmission(this.transmissionType, this.filename).AddDefaultConstraints();
                transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                    var t = sender as Transmission;
                    if (e.PropertyName == Utils.NameOf(() => t.Position)) {
                        this.position = (long)t.Position;
                        this.percent = t.Percent.GetValueOrDefault();
                    }
                };
                using (var progress = new ProgressStream(offsetstream, transmission)) {
                    progress.Seek(0, SeekOrigin.Begin);
                    Assert.AreEqual(offset, this.position);
                    Assert.AreEqual(50, this.percent);
                    progress.Seek(10, SeekOrigin.Current);
                    Assert.AreEqual(offset + 10, this.position);
                    progress.Seek(0, SeekOrigin.End);
                    Assert.AreEqual(100, this.percent);
                    Assert.AreEqual(offset + inputContent.Length, this.position);
                    progress.Seek(0, SeekOrigin.Begin);
                    progress.WriteByte(0);
                    Assert.AreEqual(offset + 1, this.position);
                    Assert.AreEqual(50.5, this.percent);
                    progress.WriteByte(0);
                    Assert.AreEqual(offset + 2, this.position);
                    Assert.AreEqual(51, this.percent);
                    progress.Write(new byte[10], 0, 10);
                    Assert.AreEqual(56, this.percent);
                }
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void EnsureBandwidthIsReportedIfProgressIsShorterThanOneSecond() {
            byte[] inputContent = new byte[1024];
            bool isMoreThanZeroReported = false;
            var transmission = new Transmission(this.transmissionType, this.filename);
            transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs args) {
                if ((sender as Transmission).BitsPerSecond > 0) {
                    isMoreThanZeroReported = true;
                }
            };
            using (var inputStream = new MemoryStream(inputContent))
            using (var outputStream = new MemoryStream())
            using (var progressStream = new ProgressStream(inputStream, transmission)) {
                progressStream.CopyTo(outputStream);
                Assert.That(outputStream.Length == inputContent.Length);
            }

            Assert.That(isMoreThanZeroReported, Is.True);
            Assert.That(transmission.BitsPerSecond, Is.Null);
        }

        [Test, Category("Fast"), Category("Streams")]
        public void AbortReadIfTransmissionEventIsAborting() {
            byte[] content = new byte[1024];
            var transmission = new Transmission(this.transmissionType, this.filename);
            using (var stream = new MemoryStream(content))
            using (var progressStream = new ProgressStream(stream, transmission)) {
                transmission.Abort();
                Assert.Throws<AbortException>(() => progressStream.ReadByte());
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void AbortWriteIfTransmissionEventIsAborting() {
            var transmission = new Transmission(this.transmissionType, this.filename);
            using (var stream = new MemoryStream())
            using (var progressStream = new ProgressStream(stream, transmission)) {
                transmission.Abort();
                Assert.Throws<AbortException>(() => progressStream.WriteByte(new byte()));
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void UpdateLengthIfInputStreamGrowsAfterStartReading() {
            using (var stream = new MemoryStream()) {
                var transmission = new Transmission(this.transmissionType, this.filename).AddDefaultConstraints();
                long initialLength = 100;
                long length = initialLength;
                byte[] buffer = new byte[initialLength];
                stream.Write(buffer, 0, buffer.Length);
                using (var progress = new ProgressStream(stream, transmission)) {
                    transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                        var t = sender as Transmission;
                        if (e.PropertyName == Utils.NameOf(() => t.Position)) {
                            Assert.That(t.Position, Is.LessThanOrEqualTo(length));
                            Assert.That(t.Length, Is.Null.Or.LessThanOrEqualTo(length));
                        }
                    };
                    progress.Read(buffer, 0, buffer.Length / 2);
                    stream.Write(buffer, 0, buffer.Length);
                    length = length + buffer.Length;
                    progress.Read(buffer, 0, buffer.Length / 2);
                    progress.Read(buffer, 0, buffer.Length / 2);
                    progress.Read(buffer, 0, buffer.Length / 2);
                    stream.Write(buffer, 0, buffer.Length);
                    length = length + buffer.Length;
                    progress.Read(buffer, 0, buffer.Length);
                }
            }
        }

        [Test, Category("Medium"), Category("Streams")]
        public void PauseAndResumeStream([Values(1,2,5)]int seconds) {
            int length = 1024;
            var start = DateTime.Now;
            byte[] content = new byte[length];
            var transmission = new Transmission(this.transmissionType, this.filename).AddDefaultConstraints();
            using (var stream = new MemoryStream(content))
            using (var progressStream = new ProgressStream(stream, transmission)) {
                transmission.Pause();
                var task = Task.Factory.StartNew(() => {
                    using(var targetStream = new MemoryStream()) {
                        progressStream.CopyTo(targetStream);
                        Assert.That(targetStream.Length, Is.EqualTo(length));
                        var duration = DateTime.Now - start;
                        Assert.That(Math.Round(duration.TotalSeconds), Is.InRange(seconds, seconds + 1));
                    }
                });
                System.Threading.Thread.Sleep(seconds * 1000);
                transmission.Resume();
                task.Wait();
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void BandwidthAfterCloseIsNull([Values(1024, 4096, 1234, 123456)]int length) {
            long? bandwidth = null;
            byte[] content = new byte[length];
            var transmission = new Transmission(this.transmissionType, this.filename).AddDefaultConstraints();
            transmission.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs args) {
                bandwidth = (sender as Transmission).BitsPerSecond;
            };
            using (var stream = new MemoryStream(content))
                using (var progressStream = new ProgressStream(stream, transmission)) {
                var task = Task.Factory.StartNew(() => {
                    using (var targetStream = new MemoryStream()) {
                        progressStream.CopyTo(targetStream);
                    }
                });
                task.Wait();
            }

            Assert.That(bandwidth, Is.Null);
        }

        public Array GetAllTypes(){
            return Enum.GetValues(typeof(TransmissionType));
        }
    }
}