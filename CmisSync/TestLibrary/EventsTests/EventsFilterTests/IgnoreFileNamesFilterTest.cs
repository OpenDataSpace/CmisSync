//-----------------------------------------------------------------------
// <copyright file="IgnoreFileNamesFilterTest.cs" company="GRAU DATA AG">
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
    public class IgnoreFileNamesFilterTest
    {
        [Test, Category("Fast"), Category("EventFilter")]
        public void DefaultConstrutorAddsRequiredFilter()
        {
            var filter = new IgnoredFileNamesFilter();
            string reason;
            Assert.That(filter.CheckFile("bla.sync", out reason), Is.True);
            Assert.That(reason, Is.Not.Null);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void AllowCorrectEventsTest()
        {
            var filter = new IgnoredFileNamesFilter();
            string reason;
            Assert.That(filter.CheckFile("testfile", out reason), Is.False);
            Assert.That(string.IsNullOrEmpty(reason), Is.True);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void HandleIgnoredFileNamesTest()
        {
            List<string> wildcards = new List<string>();
            wildcards.Add("*~");
            var filter = new IgnoredFileNamesFilter { Wildcards = wildcards };
            string reason;
            Assert.That(filter.CheckFile("file~", out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False);
        }
    }
}
