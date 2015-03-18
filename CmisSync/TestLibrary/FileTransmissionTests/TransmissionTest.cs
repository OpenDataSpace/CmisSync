//-----------------------------------------------------------------------
// <copyright file="FileTransmissionEventTest.cs" company="GRAU DATA AG">
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
using CmisSync.Lib;

namespace TestLibrary.FileTransmissionTests {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;

    using NUnit.Framework;

    [TestFixture]
    public class TransmissionTest {
        private readonly string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        private readonly string cache = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        [Test, Category("Fast"), TestCaseSource("GetAllTypes")]
        public void ConstructorTakesTypeAndFileName(TransmissionType type) {
            var underTest = new Transmission(type, this.path);
            Assert.That(underTest.Type, Is.EqualTo(type));
            Assert.That(underTest.Path, Is.EqualTo(this.path));
            Assert.That(underTest.CachePath, Is.Null);
            Assert.That(underTest.Status, Is.EqualTo(TransmissionStatus.TRANSMITTING));
        }

        [Test, Category("Fast"), TestCaseSource("GetAllTypes")]
        public void ConstructorTakesTypeFileNameAndCachePath(TransmissionType type) {
            var underTest = new Transmission(type, this.path, this.cache);
            Assert.That(underTest.Type, Is.EqualTo(type));
            Assert.That(underTest.Path, Is.EqualTo(this.path));
            Assert.That(underTest.CachePath, Is.EqualTo(this.cache));
            Assert.That(underTest.Status, Is.EqualTo(TransmissionStatus.TRANSMITTING));
        }

        [Test, Category("Fast"), TestCaseSource("GetAllTypes")]
        public void NotifyLengthChange(TransmissionType type) {
            var underTest = new Transmission(type, this.path);
            long expectedLength = 0;
            int lengthChanged = 0;
            underTest.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                if (e.PropertyName == Utils.NameOf((Transmission t) => t.Length)) {
                    Assert.That((sender as Transmission).Length, Is.EqualTo(expectedLength));
                    lengthChanged++;
                }
            };

            underTest.Length = expectedLength;
            underTest.Length = expectedLength;
            Assert.That(lengthChanged, Is.EqualTo(1));

            expectedLength = 1024;
            underTest.Length = expectedLength;
            Assert.That(lengthChanged, Is.EqualTo(2));
        }

        [Test, Category("Fast")]
        public void CalculateBitsPerSecondWithOneMinuteDifference() {
            DateTime start = DateTime.Now;
            DateTime end = start.AddMinutes(1);
            long? bitsPerSecond = Transmission.CalcBitsPerSecond(start, end, 1);
            Assert.AreEqual(0, bitsPerSecond);
            bitsPerSecond = Transmission.CalcBitsPerSecond(start, end, 60);
            Assert.AreEqual(8, bitsPerSecond);
        }

        [Test, Category("Fast")]
        public void CalcBitsPerSecondWithOneSecondDifference()
        {
            DateTime start = DateTime.Now;
            DateTime end = start.AddSeconds(1);
            long? bitsPerSecond = Transmission.CalcBitsPerSecond(start, end, 1);
            Assert.AreEqual(8, bitsPerSecond);
            bitsPerSecond = Transmission.CalcBitsPerSecond(start, start, 100);
            Assert.Null(bitsPerSecond);
            bitsPerSecond = Transmission.CalcBitsPerSecond(start, end, 100);
            Assert.AreEqual(8 * 100, bitsPerSecond);
        }

        [Test, Category("Fast")]
        public void CalculateBitsPerSecondWithOneMilisecondDifference() {
            DateTime start = DateTime.Now;
            DateTime end = start.AddMilliseconds(1);
            long? bitsPerSecond = Transmission.CalcBitsPerSecond(start, end, 1);
            Assert.AreEqual(8000, bitsPerSecond);
        }

        [Test, Category("Fast")]
        public void CalculationOfBitsPerSecondFailsOnIllegalDifference() {
            DateTime start = DateTime.Now;
            DateTime end = start.AddSeconds(1);
            Assert.Throws<ArgumentException>(() => Transmission.CalcBitsPerSecond(end, start, 100));
        }

        [Test, Category("Fast"), TestCaseSource("GetAllTypes")]
        public void Percent(TransmissionType type) {
            var underTest = new Transmission(type, this.path);
            double? percent = null;
            underTest.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                if (e.PropertyName == Utils.NameOf((Transmission t) => t.Percent)) {
                    percent = (sender as Transmission).Percent;
                }
            };

            Assert.That(underTest.Percent, Is.Null);

            underTest.Length = 100;
            underTest.Position = 0;

            Assert.That(percent, Is.EqualTo(0));
            underTest.Position = 10;
            Assert.That(percent, Is.EqualTo(10));
            underTest.Position = 100;
            Assert.That(percent, Is.EqualTo(100));
            underTest.Length = 1000;
            Assert.That(percent, Is.EqualTo(10));
            underTest.Position = 1000;
            underTest.Length = 2000;
            Assert.That(percent, Is.EqualTo(50));
        }

        public Array GetAllTypes(){
            return Enum.GetValues(typeof(TransmissionType));
        }
    }
}