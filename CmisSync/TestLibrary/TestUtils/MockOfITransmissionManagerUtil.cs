
namespace TestLibrary.TestUtils {
    using System;

    using CmisSync.Lib.FileTransmission;

    using Moq;

    public static class MockOfITransmissionManagerUtil {
        public static void VerifyThatNoTransmissionIsCreated(this Mock<ITransmissionManager> manager) {
            manager.Verify(m => m.CreateTransmission(It.IsAny<TransmissionType>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        public static void VerifyThatTransmissionWasCreatedOnce(this Mock<ITransmissionManager> manager) {
            manager.Verify(m => m.CreateTransmission(It.IsAny<TransmissionType>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        public static Mock<Transmission> SetupCreateTransmissionOnce(this Mock<ITransmissionManager> manager, TransmissionType type, string path = null, string cachePath = null) {
            var p = path ?? string.Empty;
            var transmission = new Mock<Transmission>(type, p, cachePath) { CallBase = true };
            manager.Setup(m => m.CreateTransmission(type, p, cachePath)).ReturnsInOrder(transmission.Object, null);
            return transmission;
        }
    }
}