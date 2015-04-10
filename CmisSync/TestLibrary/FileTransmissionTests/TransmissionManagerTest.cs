//-----------------------------------------------------------------------
// <copyright file="TransmissionManagerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary {
    using System;
    using System.Collections.Generic;
    using System.Text;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TransmissionManagerTest {
        [Test, Category("Fast")]
        public void DefaultConstructorDoesNotFail() {
            new TransmissionManager();
        }

        [Test, Category("Fast")]
        public void CreatingASingleTransmissionIncreasesListCountByOne() {
            var underTest = new TransmissionManager();

            underTest.CreateTransmission(TransmissionType.DOWNLOAD_NEW_FILE, "path");

            Assert.That(underTest.ActiveTransmissions.Count, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void ListedTransmissionIsEqualToAdded() {
            var underTest = new TransmissionManager();
            var trans = underTest.CreateTransmission(TransmissionType.DOWNLOAD_NEW_FILE, "path");

            Assert.That(underTest.ActiveTransmissions[0], Is.EqualTo(trans));
            Assert.That(underTest.ActiveTransmissionsAsList()[0], Is.EqualTo(trans));
        }

        [Test, Category("Fast")]
        public void AFinishedTransmissionIsRemovedFromList() {
            var underTest = new TransmissionManager();
            var trans = underTest.CreateTransmission(TransmissionType.DOWNLOAD_NEW_FILE, "path");

            trans.Status = TransmissionStatus.FINISHED;

            Assert.That(underTest.ActiveTransmissions, Is.Empty);
        }

        [Test, Category("Fast")]
        public void AnAbortedTransmissionIsRemovedFromList() {
            var underTest = new TransmissionManager();
            var trans = underTest.CreateTransmission(TransmissionType.DOWNLOAD_NEW_FILE, "path");

            trans.Status = TransmissionStatus.ABORTED;

            Assert.That(underTest.ActiveTransmissions, Is.Empty);
        }

        [Test, Category("Fast")]
        public void CreatingTwoTransmissionProducesTwoEntriesInList() {
            var underTest = new TransmissionManager();
            underTest.CreateTransmission(TransmissionType.DOWNLOAD_NEW_FILE, "path");
            underTest.CreateTransmission(TransmissionType.DOWNLOAD_NEW_FILE, "path2");

            Assert.That(underTest.ActiveTransmissions.Count, Is.EqualTo(2));
        }

        [Test, Category("Fast")]
        public void CreatingATransmissionFiresEvent() {
            var underTest = new TransmissionManager();

            int eventCounter = 0;
            string path = "path";
            underTest.ActiveTransmissions.CollectionChanged += delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
                eventCounter++;
                Assert.That(e.NewItems.Count, Is.EqualTo(1));
                Assert.That((e.NewItems[0] as Transmission).Type, Is.EqualTo(TransmissionType.DOWNLOAD_NEW_FILE));
                Assert.That((e.NewItems[0] as Transmission).Path, Is.EqualTo(path));
            };

            underTest.CreateTransmission(TransmissionType.DOWNLOAD_NEW_FILE, path);

            Assert.That(eventCounter, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void AFinishedTransmissionFiresEvent() {
            var underTest = new TransmissionManager();
            var trans = underTest.CreateTransmission(TransmissionType.DOWNLOAD_NEW_FILE, "path");
            int eventCounter = 0;

            underTest.ActiveTransmissions.CollectionChanged += delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
                eventCounter++;
                Assert.That(e.NewItems, Is.Null);
                Assert.That(e.OldItems.Count, Is.EqualTo(1));
                Assert.That(e.OldItems[0], Is.EqualTo(trans));
            };
            trans.Status = TransmissionStatus.FINISHED;

            Assert.That(eventCounter, Is.EqualTo(1));
        }
    }
}