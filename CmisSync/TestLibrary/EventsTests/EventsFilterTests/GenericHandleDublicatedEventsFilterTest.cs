using System;
using System.IO;
using System.Collections.Generic;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Events.Filter;

using NUnit.Framework;

using Moq;

namespace TestLibrary.EventsTests.EventsFilterTests
{
    [TestFixture]
    public class GenericHandleDublicatedEventsFilterTest
    {
        private readonly DotCMIS.Exceptions.CmisPermissionDeniedException DeniedException = new DotCMIS.Exceptions.CmisPermissionDeniedException();
        private readonly Uri url = new Uri("http://example.com");

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
            var filter = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, SuccessfulLoginEvent>();
            Assert.IsFalse(filter.Handle(new Mock<SuccessfulLoginEvent>(url).Object));
            Assert.IsFalse(filter.Handle(new Mock<SuccessfulLoginEvent>(url).Object));
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterLetsFirstFilterTypePassing()
        {
            var filter = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, SuccessfulLoginEvent>();
            Assert.IsFalse(filter.Handle(new Mock<PermissionDeniedEvent>(DeniedException).Object));
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterHandlesSecondMatchingFilterType()
        {
            var filter = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, SuccessfulLoginEvent>();
            Assert.IsFalse(filter.Handle(new Mock<PermissionDeniedEvent>(DeniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(DeniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(DeniedException).Object));
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterLetMatchingFilterTypePassAfterResetTypeOccured()
        {
            var filter = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, SuccessfulLoginEvent>();
            Assert.IsFalse(filter.Handle(new Mock<PermissionDeniedEvent>(DeniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(DeniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(DeniedException).Object));
            Assert.IsFalse(filter.Handle(new Mock<SuccessfulLoginEvent>(url).Object));
            Assert.IsFalse(filter.Handle(new Mock<PermissionDeniedEvent>(DeniedException).Object));
            Assert.IsTrue(filter.Handle(new Mock<PermissionDeniedEvent>(DeniedException).Object));
        }
    }
}

