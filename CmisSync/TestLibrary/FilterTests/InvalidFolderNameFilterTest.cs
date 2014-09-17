//-----------------------------------------------------------------------
// <copyright file="InvalidFolderNameFilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.FilterTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class InvalidFolderNameFilterTest
    {
        [Test, Category("Fast"), Category("EventFilter")]
        public void DefaultConstructor() {
            new InvalidFolderNameFilter();
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void InvalidFolderNames() {
            InvalidFolderNameFilter filter = new InvalidFolderNameFilter();
            string reason;
            Assert.That(filter.CheckFolderName("*", out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False, reason);

            Assert.That(filter.CheckFolderName("?", out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False, reason);

            Assert.That(filter.CheckFolderName(":", out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False, reason);

            Assert.That(filter.CheckFolderName("test Test/ test", out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False, reason);

            Assert.That(filter.CheckFolderName(@"test Test\ test", out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False, reason);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void ValidFolderNames() {
            InvalidFolderNameFilter filter = new InvalidFolderNameFilter();
            string reason;
            Assert.That(filter.CheckFolderName("test", out reason), Is.False);
            Assert.That(string.IsNullOrEmpty(reason), Is.True, reason);

            Assert.That(filter.CheckFolderName("test_test", out reason), Is.False);
            Assert.That(string.IsNullOrEmpty(reason), Is.True, reason);
        }
    }
}
