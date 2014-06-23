//-----------------------------------------------------------------------
// <copyright file="IgnoredFolderNamesFilterTest.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.Events.Filter;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IgnoredFolderNamesFilterTest
    {
        [Test, Category("Fast"), Category("EventFilter")]
        public void DefaultConstructor()
        {
            new IgnoredFolderNameFilter();
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterLetsFSEventsPassIfNoWildcardsAreSet()
        {
            var filter = new IgnoredFolderNameFilter();
            string reason;
            Assert.That(filter.CheckFolderName("foldername", out reason), Is.False);
            Assert.That(string.IsNullOrEmpty(reason), Is.True);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterTakesWildcardsWithoutFailure()
        {
            var filter = new IgnoredFolderNameFilter();
            var wildcards = new List<string>();
            wildcards.Add("*.tmp");
            filter.Wildcards = wildcards;
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterTakesEmptyWildcardsWithoutFailure()
        {
            var filter = new IgnoredFolderNameFilter();
            filter.Wildcards = new List<string>();
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FilterFailsTakingNullWildcard()
        {
            var filter = new IgnoredFolderNameFilter();
            filter.Wildcards = null;
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterFiltersEventsMatchingWildcard()
        {
            var wildcards = new List<string>();
            wildcards.Add(".*");
            var filter = new IgnoredFolderNameFilter { Wildcards = wildcards };
            string reason;
            Assert.That(filter.CheckFolderName(".test", out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterLetsEventsPassIfNotMatchingWildcard()
        {
            var wildcards = new List<string>();
            wildcards.Add(".tmp");
            var filter = new IgnoredFolderNameFilter { Wildcards = wildcards };
            string reason;
            Assert.That(filter.CheckFolderName(".cache", out reason), Is.False);
            Assert.That(string.IsNullOrEmpty(reason), Is.True);
        }
    }
}
