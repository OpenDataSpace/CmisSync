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

namespace TestLibrary.FileTransmissionTests {
    using System;
    using System.Collections.Generic;
    using System.Text;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using DataSpace.Common.Transmissions;

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

            underTest.Add(new Transmission(TransmissionType.DownloadNewFile, "path"));

            Assert.That(underTest.ActiveTransmissions.Count, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void ListedTransmissionIsEqualToAdded() {
            var underTest = new TransmissionManager();
            var trans = new Transmission(TransmissionType.DownloadNewFile, "path");
            underTest.Add(trans);

            Assert.That(underTest.ActiveTransmissions[0], Is.EqualTo(trans));
            Assert.That(underTest.ActiveTransmissionsAsList()[0], Is.EqualTo(trans));
        }

        [Test, Category("Fast")]
        public void AFinishedTransmissionIsRemovedFromList() {
            var underTest = new TransmissionManager();
            var trans = new Transmission(TransmissionType.DownloadNewFile, "path");
            underTest.Add(trans);

            trans.Status = Status.Finished;

            Assert.That(underTest.ActiveTransmissions, Is.Empty);
        }

        [Test, Category("Fast")]
        public void AnAbortedTransmissionIsRemovedFromList() {
            var underTest = new TransmissionManager();
            var trans = new Transmission(TransmissionType.DownloadNewFile, "path");
            underTest.Add(trans);

            trans.Status = Status.Aborted;

            Assert.That(underTest.ActiveTransmissions, Is.Empty);
        }

        [Test, Category("Fast")]
        public void AddingTwoTransmissionProducesTwoEntriesInList() {
            var underTest = new TransmissionManager();
            underTest.Add(new Transmission(TransmissionType.DownloadNewFile, "path"));
            underTest.Add(new Transmission(TransmissionType.DownloadNewFile, "path2"));

            Assert.That(underTest.ActiveTransmissions.Count, Is.EqualTo(2));
        }

        [Test, Category("Fast")]
        public void AddingTransmissionFiresEvent() {
            var underTest = new TransmissionManager();

            int eventCounter = 0;
            string path = "path";
            underTest.ActiveTransmissions.CollectionChanged += delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
                eventCounter++;
                Assert.That(e.NewItems.Count, Is.EqualTo(1));
                Assert.That((e.NewItems[0] as Transmission).Type, Is.EqualTo(TransmissionType.DownloadNewFile));
                Assert.That((e.NewItems[0] as Transmission).Path, Is.EqualTo(path));
            };

            underTest.Add(new Transmission(TransmissionType.DownloadNewFile, path));

            Assert.That(eventCounter, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void AFinishedTransmissionFiresEvent() {
            var underTest = new TransmissionManager();
            var trans = new Transmission(TransmissionType.DownloadNewFile, "path");
            underTest.Add(trans);
            int eventCounter = 0;

            underTest.ActiveTransmissions.CollectionChanged += delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
                eventCounter++;
                Assert.That(e.NewItems, Is.Null);
                Assert.That(e.OldItems.Count, Is.EqualTo(1));
                Assert.That(e.OldItems[0], Is.EqualTo(trans));
            };
            trans.Status = Status.Finished;

            Assert.That(eventCounter, Is.EqualTo(1));
        }
    }
}