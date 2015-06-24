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

namespace TestLibrary.FilterTests {
    using System;

    using CmisSync.Lib.Filter;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FilterAggregatorTest {
        private IgnoredFileNamesFilter arg0;
        private IgnoredFolderNameFilter arg1;
        private InvalidFolderNameFilter arg2;
        private IgnoredFoldersFilter arg3;

        [SetUp]
        public void CreateFilter() {
            arg0 = new IgnoredFileNamesFilter();
            arg1 = new IgnoredFolderNameFilter();
            arg2 = new InvalidFolderNameFilter();
            arg3 = new IgnoredFoldersFilter();
        }

        [Test, Category("Fast")]
        public void ConstructorSetAllFilter([Values(true, false, null)]bool? passSymlinkFilter) {
            SymlinkFilter symlinkFilter = passSymlinkFilter == true ? new SymlinkFilter() : null;
            var filter = passSymlinkFilter == false ? new FilterAggregator(this.arg0, this.arg1, this.arg2, this.arg3) : new FilterAggregator(this.arg0, this.arg1, this.arg2, this.arg3, symlinkFilter);
            Assert.That(filter.FileNamesFilter, Is.EqualTo(this.arg0));
            Assert.That(filter.FolderNamesFilter, Is.EqualTo(this.arg1));
            Assert.That(filter.InvalidFolderNamesFilter, Is.EqualTo(this.arg2));
            Assert.That(filter.IgnoredFolderFilter, Is.EqualTo(this.arg3));
            if (symlinkFilter != null) {
                Assert.That(filter.SymlinkFilter, Is.EqualTo(symlinkFilter));
            } else {
                Assert.That(filter.SymlinkFilter, Is.Not.Null);
            }
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfFilter0IsNull() {
            Assert.Throws<ArgumentNullException>(() => new FilterAggregator(null, this.arg1, this.arg2, this.arg3));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfFilter1IsNull() {
            Assert.Throws<ArgumentNullException>(() => new FilterAggregator(this.arg0, null, this.arg2, this.arg3));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfFilter2IsNull() {
            Assert.Throws<ArgumentNullException>(() => new FilterAggregator(this.arg0, this.arg1, null, this.arg3));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfFilter3IsNull() {
            Assert.Throws<ArgumentNullException>(() => new FilterAggregator(this.arg0, this.arg1, this.arg2, null));
        }
    }
}