
namespace TestLibrary.TestUtils {
    using System;

    using CmisSync.Lib;
    using CmisSync.Lib.FileTransmission;

    using Moq;

    using NUnit.Framework;
    using NUnit.Framework.Constraints;

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

        public static Transmission AddPropertyContraint(this Transmission transmission, ReusableConstraint constraint, string propertyName) {
            transmission.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                var t = sender as Transmission;
                if (e.PropertyName == propertyName) {
                    Assert.That(t, Has.Property(propertyName));
                    var value = t.GetType().GetProperty(propertyName).GetValue(t, null);
                    if (value != null) {
                        Assert.That(value, constraint, propertyName);
                    }
                }
            };

            return transmission;
        }

        public static Transmission AddLengthConstraint(this Transmission transmission, IResolveConstraint constraint) {
            return transmission.AddPropertyContraint(new ReusableConstraint(constraint), Utils.NameOf(() => transmission.Length));
        }

        public static Transmission AddPercentConstraint(this Transmission transmission, IResolveConstraint constraint) {
            return transmission.AddPropertyContraint(new ReusableConstraint(constraint), Utils.NameOf(() => transmission.Percent));
        }

        public static Transmission AddPositionConstraint(this Transmission transmission, IResolveConstraint constraint) {
            return transmission.AddPropertyContraint(new ReusableConstraint(constraint), Utils.NameOf(() => transmission.Position));
        }

        public static Transmission AddDefaultConstraints(this Transmission transmission) {
            return transmission.AddLengthConstraint(Is.GreaterThanOrEqualTo(0)).AddPercentConstraint(Is.InRange(0, 100)).AddPositionConstraint(Is.GreaterThanOrEqualTo(0));
        }
    }
}