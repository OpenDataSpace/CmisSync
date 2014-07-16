//-----------------------------------------------------------------------
// <copyright file="GenericHandleDublicatedEventsFilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.EventsTests.EventsFilterTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class GenericHandleDublicatedEventsFilterTest
    {
        private readonly DotCMIS.Exceptions.CmisPermissionDeniedException deniedException = new DotCMIS.Exceptions.CmisPermissionDeniedException();

        [Test, Category("Fast"), Category("EventFilter")]
        public void DefaultConstructorWorks()
        {
            var filter = new GenericHandleDublicatedEventsFilter<ISyncEvent, ISyncEvent>();
            Assert.That(filter.Priority == EventHandlerPriorities.FILTER);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterIgnoresNotMatchingFilterType()
        {
            var filter = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, SuccessfulLoginEvent>();
            Assert.IsFalse(filter.Handle(new Mock<ISyncEvent>().Object));
            Assert.IsFalse(filter.Handle(new Mock<ISyncEvent>().Object));
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterLetsResetTypePassingThrough()
        {
            var filter = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, StartNextSyncEvent>();
            Assert.IsFalse(filter.Handle(new Mock<StartNextSyncEvent>(false).Object));
            Assert.IsFalse(filter.Handle(new Mock<StartNextSyncEvent>(false).Object));
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterLetsFirstFilterTypePassing()
        {
            var filter = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, SuccessfulLoginEvent>();
            Assert.IsFalse(filter.Handle(new Mock<PermissionDeniedEvent>(this.deniedException).Object));
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterHandlesSecondMatchingFilterType()
        {
            var filter = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, SuccessfulLoginEvent>();
            Assert.IsFalse(filter.Handle(new Mock<PermissionDeniedEvent>(this.deniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(this.deniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(this.deniedException).Object));
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterLetMatchingFilterTypePassAfterResetTypeOccured()
        {
            var filter = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, StartNextSyncEvent>();
            Assert.IsFalse(filter.Handle(new Mock<PermissionDeniedEvent>(this.deniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(this.deniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(this.deniedException).Object));
            Assert.IsFalse(filter.Handle(new Mock<StartNextSyncEvent>(false).Object));
            Assert.IsFalse(filter.Handle(new Mock<PermissionDeniedEvent>(this.deniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(this.deniedException).Object));
        }
    }
}
