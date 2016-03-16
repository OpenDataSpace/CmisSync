//-----------------------------------------------------------------------
// <copyright file="MockOfITransmissionManagerUtil.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils {
    using System;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Queueing;

    using DataSpace.Common.Transmissions;

    using Moq;

    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    public static class MockOfITransmissionManagerUtil {
        public static void VerifyThatNoTransmissionIsCreated(this Mock<ITransmissionFactory> manager) {
            manager.Verify(m => m.CreateTransmission(It.IsAny<TransmissionType>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        public static void VerifyThatTransmissionWasCreatedOnce(this Mock<ITransmissionFactory> manager) {
            manager.Verify(m => m.CreateTransmission(It.IsAny<TransmissionType>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        public static Mock<Transmission> SetupCreateTransmissionOnce(this Mock<ITransmissionFactory> manager, TransmissionType type, string path = null, string cachePath = null) {
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

        public static ITransmissionFactory CreateFactory(AbstractNotifyingRepository repo = null, ITransmissionAggregator aggregator = null) {
            if (repo == null) {
                repo = new Mock<AbstractNotifyingRepository>(new RepoInfo() { DisplayName = "mockedRepo" }).Object;
            }

            if (aggregator == null) {
                aggregator = new TransmissionManager();
            }

            return new CmisSync.Lib.FileTransmission.TransmissionFactory(repo, aggregator);
        }

        public static ITransmissionFactory CreateFactory(this ITransmissionAggregator aggregator, AbstractNotifyingRepository repo = null) {
            if (repo == null) {
                repo = new Mock<AbstractNotifyingRepository>(new RepoInfo() { DisplayName = "mockedRepo" }).Object;
            }

            return new CmisSync.Lib.FileTransmission.TransmissionFactory(repo, aggregator);
        }
    }
}