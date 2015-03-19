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

namespace TestLibrary.FileTransmissionTests {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib;
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
        public void CalcBitsPerSecondWithOneSecondDifference() {
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

            underTest.Position = 501;
            underTest.Length = 1000;
            Assert.That(percent, Is.EqualTo(50.1));
        }

        [Test, Category("Fast")]
        public void Pause() {
            var underTest = new Transmission(TransmissionType.DOWNLOAD_NEW_FILE, this.path);
            underTest.Pause();
            Assert.That(underTest.Status == TransmissionStatus.PAUSED);
        }

        [Test, Category("Fast")]
        public void PauseAbortedTransmissionDoesNotChangeTheStatus([Values(TransmissionStatus.ABORTING, TransmissionStatus.ABORTED)]TransmissionStatus status) {
            var underTest = new Transmission(TransmissionType.DOWNLOAD_NEW_FILE, this.path);
            underTest.Status = status;
            underTest.Pause();
            Assert.That(underTest.Status, Is.EqualTo(status));
        }

        [Test, Category("Fast")]
        public void Resume() {
            var underTest = new Transmission(TransmissionType.DOWNLOAD_NEW_FILE, this.path);
            underTest.Resume();
            Assert.That(underTest.Status == TransmissionStatus.TRANSMITTING);
            underTest.Pause();
            underTest.Resume();
            Assert.That(underTest.Status == TransmissionStatus.TRANSMITTING);

            underTest.Abort();
            underTest.Resume();
            Assert.That(underTest.Status == TransmissionStatus.ABORTING);
            underTest.Status = TransmissionStatus.ABORTED;
            underTest.Resume();
            Assert.That(underTest.Status == TransmissionStatus.ABORTED);
        }

        [Test, Category("Fast")]
        public void LastModificationDate() {
            var past = DateTime.Now - TimeSpan.FromDays(1);
            int changed = 0;
            var underTest = new Transmission(TransmissionType.DOWNLOAD_NEW_FILE, this.path);
            Assert.That(underTest.LastModification, Is.EqualTo(DateTime.Now).Within(1).Seconds);
            underTest.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                if (e.PropertyName == Utils.NameOf((Transmission t) => t.LastModification)) {
                    Assert.That((sender as Transmission).LastModification, Is.EqualTo(past));
                    changed++;
                }
            };
            underTest.LastModification = past;
            Assert.That(underTest.LastModification, Is.EqualTo(past));
            Assert.That(changed, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void SettingFailedTransmissionExceptionAlsoSetsTheAbortFlag() {
            var underTest = new Transmission(TransmissionType.DOWNLOAD_NEW_FILE, this.path);
            bool changed = false;
            underTest.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                if (e.PropertyName == Utils.NameOf((Transmission t) => t.Status)) {
                    Assert.That((sender as Transmission).Status, Is.EqualTo(TransmissionStatus.ABORTED));
                    changed = true;
                }
            };

            underTest.FailedException = new Exception("generic test exception");

            Assert.That(changed, Is.True, "The status must be changed if an exception is added");
        }

        public Array GetAllTypes(){
            return Enum.GetValues(typeof(TransmissionType));
        }
    }
}