//-----------------------------------------------------------------------
// <copyright file="SuccessfulLoginEventTest.cs" company="GRAU DATA AG">
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
namespace TestLibrary.EventsTests {
    using System;

    using CmisSync.Lib.Events;

    using DotCMIS.Client;

    using NUnit.Framework;

    using Moq;

    [TestFixture, Category("Fast")]
    public class SuccessfulLoginEventTest {
        private readonly Uri url = new Uri("https://demo.deutsche-wolke.de/cmis/browser");
        private readonly ISession session = new Mock<ISession>(MockBehavior.Strict).Object;
        private readonly IFolder rootFolder = new Mock<IFolder>(MockBehavior.Strict).Object;

        [Test]
        public void ConstructorTakesUrlAndSessionAndRootFolder(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSyncSupported,
            [Values(true, false)]bool changeEventsSupported)
        {
            var underTest = new SuccessfulLoginEvent(this.url, this.session, this.rootFolder, pwcIsSupported, selectiveSyncSupported, changeEventsSupported);

            Assert.That(underTest.Session, Is.EqualTo(this.session));
            Assert.That(underTest.RootFolder, Is.EqualTo(this.rootFolder));
            Assert.That(underTest.PrivateWorkingCopySupported, Is.EqualTo(pwcIsSupported));
            Assert.That(underTest.SelectiveSyncSupported, Is.EqualTo(selectiveSyncSupported));
            Assert.That(underTest.ChangeEventsSupported, Is.EqualTo(changeEventsSupported));
        }

        [Test]
        public void ConstructorFailsIfUrlIsNull(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSyncSupported,
            [Values(true, false)]bool changeEventsSupported)
        {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(null, this.session, this.rootFolder, pwcIsSupported, selectiveSyncSupported, changeEventsSupported));
        }

        [Test]
        public void ConstructorFailsIfSessionIsNull(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSyncSupported,
            [Values(true, false)]bool changeEventsSupported)
        {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(this.url, null, this.rootFolder, pwcIsSupported, selectiveSyncSupported, changeEventsSupported));
        }

        [Test]
        public void ConstructorFailsIfRootFolderIsNull(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSyncSupported,
            [Values(true, false)]bool changeEventsSupported)
        {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(this.url, this.session, null, pwcIsSupported, selectiveSyncSupported, changeEventsSupported));
        }
    }
}