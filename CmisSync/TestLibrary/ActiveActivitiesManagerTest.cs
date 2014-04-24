namespace TestLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using CmisSync.Lib;
    using CmisSync.Lib.Events;

    using NUnit.Framework;

    using Moq;

    [TestFixture]
    public class ActiveActivitiesManagerTest
    {
        [Test, Category("Fast")]
        public void DefaultConstructorDoesNotFail()
        {
            new ActiveActivitiesManager();
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingNullAsTransmissionReturnsFalse()
        {
            var manager = new ActiveActivitiesManager();

            manager.AddTransmission(null);
        }

        [Test, Category("Fast")]
        public void AddSingleTransmissionIncreasesListCountByOne()
        {
            var manager = new ActiveActivitiesManager();

            Assert.IsTrue(manager.AddTransmission(new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE,"path")));

            Assert.That(manager.ActiveTransmissions.Count, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void ListedTransmissionIsEqualToAdded()
        {
            var manager = new ActiveActivitiesManager();
            var trans = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, "path");

            Assert.That(manager.AddTransmission(trans), Is.True);

            Assert.That(manager.ActiveTransmissions[0], Is.EqualTo(trans));
            Assert.That(manager.ActiveTransmissionsAsList()[0], Is.EqualTo(trans));
        }

        [Test, Category("Fast")]
        public void AFinishedTransmissionIsRemovedFromList()
        {
            var manager = new ActiveActivitiesManager();
            var trans = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, "path");
            manager.AddTransmission(trans);

            trans.ReportProgress(new TransmissionProgressEventArgs{Completed = true});

            Assert.That(manager.ActiveTransmissions, Is.Empty);
        }

        [Test, Category("Fast")]
        public void AddingTheSameInstanceASecondTimeReturnsFalseAndIsNotListed()
        {
            var manager = new ActiveActivitiesManager();
            var trans = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, "path");

            Assert.That(manager.AddTransmission(trans), Is.True);

            Assert.That(manager.AddTransmission(trans), Is.False);

            Assert.That(manager.ActiveTransmissions.Count, Is.EqualTo(1));
            Assert.That(manager.ActiveTransmissions[0], Is.EqualTo(trans));
        }

        [Test, Category("Fast")]
        public void AnAbortedTransmissionIsRemovedFromList()
        {
            var manager = new ActiveActivitiesManager();
            var trans = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, "path");
            manager.AddTransmission(trans);

            trans.ReportProgress(new TransmissionProgressEventArgs{Aborted = true});

            Assert.That(manager.ActiveTransmissions, Is.Empty);
        }

        [Test, Category("Fast")]
        public void AddingNonEqualTransmissionProducesNewEntryInList()
        {
            var manager = new ActiveActivitiesManager();
            var trans = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, "path");
            var trans2 = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, "path2");

            Assert.That(manager.AddTransmission(trans), Is.True);
            Assert.That(manager.AddTransmission(trans2), Is.True);

            Assert.That(manager.ActiveTransmissions.Count, Is.EqualTo(2));
        }

        [Test, Category("Fast")]
        public void AddingATransmissionFiresEvent()
        {
            var manager = new ActiveActivitiesManager();
            var trans = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, "path");
            int eventCounter = 0;

            manager.ActiveTransmissions.CollectionChanged += delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                eventCounter ++;
                Assert.That(e.NewItems.Count, Is.EqualTo(1));
                Assert.That(e.NewItems[0], Is.EqualTo(trans));
            };
            manager.AddTransmission(trans);

            Assert.That(eventCounter, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void AFinishedTransmissionFiresEvent()
        {
            var manager = new ActiveActivitiesManager();
            var trans = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, "path");
            int eventCounter = 0;
            manager.AddTransmission(trans);

            manager.ActiveTransmissions.CollectionChanged += delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                eventCounter ++;
                Assert.That(e.NewItems, Is.Null);
                Assert.That(e.OldItems.Count, Is.EqualTo(1));
                Assert.That(e.OldItems[0], Is.EqualTo(trans));
            };
            trans.ReportProgress(new TransmissionProgressEventArgs{Completed = true});

            Assert.That(eventCounter, Is.EqualTo(1));
        }
    }
}
