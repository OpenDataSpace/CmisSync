
namespace TestLibrary.StreamsTests {
    using System;
    using System.IO;

    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Streams;

    using NUnit.Framework;

    using Moq;

    [TestFixture]
    public class TransmissionStreamTest {
        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorFailsIfStreamIsNull() {
            Assert.Throws<ArgumentNullException>(() => {using (new TransmissionStream(null, new Transmission(TransmissionType.DOWNLOAD_MODIFIED_FILE, "path")));});
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorFailsIfTransmissionIsNull() {
            Assert.Throws<ArgumentNullException>(() => {
                using (var stream = new MemoryStream())
                using (new TransmissionStream(stream, null));
            });
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorTakesTransmissionAndStream() {
            var transmission = new Transmission(TransmissionType.DOWNLOAD_MODIFIED_FILE, "path");
            using (var stream = new MemoryStream())
            using (var underTest = new TransmissionStream(stream, transmission)) {
            }
        }
    }
}