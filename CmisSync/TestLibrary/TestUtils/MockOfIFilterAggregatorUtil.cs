//-----------------------------------------------------------------------
// <copyright file="MockOfIFilterAggregatorUtil.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils
{
    using System;

    using CmisSync.Lib.Events.Filter;

    using Moq;

    public static class MockOfIFilterAggregatorUtil
    {
        public static Mock<IFilterAggregator> CreateFilterAggregator() {
            string reason;
            var filter = Mock.Of<IFilterAggregator>(
                f =>
                f.FileNamesFilter == Mock.Of<IgnoredFileNamesFilter>(
                i =>
                i.CheckFile(It.IsAny<string>(), out reason) == false) &&
                f.FolderNamesFilter == Mock.Of<IgnoredFolderNameFilter>(
                i =>
                i.CheckFolderName(It.IsAny<string>(), out reason) == false) &&
                f.InvalidFolderNamesFilter == Mock.Of<InvalidFolderNameFilter>(
                i =>
                i.CheckPath(It.IsAny<string>(), out reason) == false) &&
                f.IgnoredFolderFilter == Mock.Of<IgnoredFoldersFilter>(
                i =>
                i.CheckPath(It.IsAny<string>(), out reason) == false));
            return Mock.Get(filter);
        }
    }
}