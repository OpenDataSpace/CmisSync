//-----------------------------------------------------------------------
// <copyright file="IgnoredFoldersFilterTest.cs" company="GRAU DATA AG">
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

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IgnoredFoldersFilterTest
    {
        [Test, Category("Fast"), Category("EventFilter")]
        public void NormalConstructor()
        {
            new IgnoredFoldersFilter();
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void AllowCorrectPaths()
        {
            var filter = new IgnoredFoldersFilter();

            string reason;
            Assert.That(filter.CheckPath(Path.GetTempPath(), out reason), Is.False);
            Assert.That(string.IsNullOrEmpty(reason), Is.True);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void ForbidIgnoredFolderNames()
        {
            var ignoredFolder = new List<string>();
            ignoredFolder.Add(Path.GetTempPath());
            var filter = new IgnoredFoldersFilter { IgnoredPaths = ignoredFolder };

            string reason;
            Assert.That(filter.CheckPath(Path.GetTempPath(), out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False);
        }
    }
}
