//-----------------------------------------------------------------------
// <copyright file="VirusDetectedExceptionTest.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.ExceptionTests {
    using System;

    using CmisSync.Lib.Exceptions;

    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using Moq;

    [TestFixture, Category("Fast"), Category("Exception")]
    public class VirusDetectedExceptionTest {
        [Test]
        public void ConstructorFailsIfGivenExceptionIsNull() {
            Assert.Throws<ArgumentNullException>(() => new VirusDetectedException(null));
        }

        [Test]
        public void ConstructorFailsIfGivenExceptionIsNotAVirusException() {
            Assert.Throws<ArgumentException>(() => new VirusDetectedException(new CmisConstraintException()));
        }

        [Test]
        public void ConstructorTakesGivenException() {
            var mockedException = new Mock<CmisConstraintException>("Conflict", "infected file") { CallBase = true };

            var underTest = new VirusDetectedException(mockedException.Object);

            Assert.That(underTest.InnerException, Is.EqualTo(mockedException.Object));
        }

        [Test]
        public void ExceptionLevelIsWarning() {
            var mockedException = new Mock<CmisConstraintException>("Conflict", "infected file") { CallBase = true };

            var underTest = new VirusDetectedException(mockedException.Object);

            Assert.That(underTest.Level, Is.EqualTo(ExceptionLevel.Warning));
        }
    }
}